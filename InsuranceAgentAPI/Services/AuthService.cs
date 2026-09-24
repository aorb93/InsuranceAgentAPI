using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using InsuranceAgentAPI.Data;
using InsuranceAgentAPI.DTOs.Auth;
using InsuranceAgentAPI.Entities;
using InsuranceAgentAPI.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using BCrypt.Net;

namespace InsuranceAgentAPI.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly JwtSettings _jwtSettings;

    public AuthService(ApplicationDbContext context, IOptions<JwtSettings> jwtSettings)
    {
        _context = context;
        _jwtSettings = jwtSettings.Value;
    }

    // =========================================================================
    // REGISTRO DE AGENTE
    // =========================================================================
    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, string? ipAddress)
    {
        // 1. Validar que el correo no esté registrado
        if (await _context.Agents.AnyAsync(a => a.Email.ToLower() == request.Email.ToLower()))
        {
            throw new BadHttpRequestException("El correo electrónico ya se encuentra registrado.");
        }

        // 2. Validar número de licencia único
        if (await _context.Agents.AnyAsync(a => a.LicenseNumber == request.LicenseNumber))
        {
            throw new BadHttpRequestException("El número de licencia ingresado ya está registrado.");
        }

        // 3. Crear entidad Agent con contraseña encriptada (BCrypt)
        var agent = new Agent
        {
            AgentGuid = Guid.NewGuid(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = request.Email.Trim().ToLower(),
            PhoneNumber = request.PhoneNumber?.Trim(),
            LicenseNumber = request.LicenseNumber.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Agents.Add(agent);
        await _context.SaveChangesAsync();

        // 4. Generar Tokens
        var accessToken = GenerateJwtToken(agent, out DateTime expiresAt);
        var refreshToken = GenerateRefreshToken(ipAddress);

        agent.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        return new AuthResponseDto
        {
            AgentGuid = agent.AgentGuid,
            FullName = $"{agent.FirstName} {agent.LastName}",
            Email = agent.Email,
            LicenseNumber = agent.LicenseNumber,
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            AccessTokenExpiresAt = expiresAt
        };
    }

    // =========================================================================
    // INICIO DE SESIÓN (LOGIN)
    // =========================================================================
    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, string? ipAddress, string? userAgent)
    {
        var emailLower = request.Email.Trim().ToLower();
        var agent = await _context.Agents
            .Include(a => a.RefreshTokens)
            .FirstOrDefaultAsync(a => a.Email.ToLower() == emailLower);

        // Validar si el usuario existe y si la contraseña coincide
        if (agent == null || !BCrypt.Net.BCrypt.Verify(request.Password, agent.PasswordHash))
        {
            await LogLoginAttemptAsync(agent?.AgentId, emailLower, false, "Credenciales inválidas", ipAddress, userAgent);
            throw new BadHttpRequestException("Correo electrónico o contraseña incorrectos.");
        }

        if (!agent.IsActive)
        {
            await LogLoginAttemptAsync(agent.AgentId, emailLower, false, "Cuenta inactiva", ipAddress, userAgent);
            throw new BadHttpRequestException("Su cuenta se encuentra inactiva. Contacte al administrador.");
        }

        // Generar nuevos tokens
        var accessToken = GenerateJwtToken(agent, out DateTime expiresAt);
        var refreshToken = GenerateRefreshToken(ipAddress);

        agent.RefreshTokens.Add(refreshToken);

        // Registrar intento exitoso
        await LogLoginAttemptAsync(agent.AgentId, emailLower, true, null, ipAddress, userAgent);

        await _context.SaveChangesAsync();

        return new AuthResponseDto
        {
            AgentGuid = agent.AgentGuid,
            FullName = $"{agent.FirstName} {agent.LastName}",
            Email = agent.Email,
            LicenseNumber = agent.LicenseNumber,
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            AccessTokenExpiresAt = expiresAt
        };
    }

    // =========================================================================
    // REFRESH TOKEN (Rotación de Tokens)
    // =========================================================================
    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request, string? ipAddress)
    {
        var agent = await _context.Agents
            .Include(a => a.RefreshTokens)
            .FirstOrDefaultAsync(a => a.RefreshTokens.Any(t => t.Token == request.RefreshToken));

        if (agent == null)
        {
            throw new BadHttpRequestException("Refresh Token inválido.");
        }

        var existingRefreshToken = agent.RefreshTokens.Single(t => t.Token == request.RefreshToken);

        if (existingRefreshToken.RevokedAt != null)
        {
            // Seguridad: Si se intenta usar un token revocado, se revocan todos sus tokens por sospecha de robo
            RevokeAllChildTokens(agent, ipAddress, "Intento de reuso de Refresh Token revocado");
            await _context.SaveChangesAsync();
            throw new BadHttpRequestException("Refresh Token revocado por razones de seguridad.");
        }

        if (existingRefreshToken.ExpiresAt <= DateTime.UtcNow)
        {
            throw new BadHttpRequestException("El Refresh Token ha expirado. Por favor, inicie sesión nuevamente.");
        }

        // Generar nuevo par de tokens (Rotación)
        var newRefreshToken = GenerateRefreshToken(ipAddress);
        existingRefreshToken.RevokedAt = DateTime.UtcNow;
        existingRefreshToken.RevokedByIp = ipAddress;
        existingRefreshToken.ReplacedByToken = newRefreshToken.Token;
        existingRefreshToken.ReasonRevoked = "Reemplazado por nuevo Token";

        agent.RefreshTokens.Add(newRefreshToken);

        var accessToken = GenerateJwtToken(agent, out DateTime expiresAt);
        await _context.SaveChangesAsync();

        return new AuthResponseDto
        {
            AgentGuid = agent.AgentGuid,
            FullName = $"{agent.FirstName} {agent.LastName}",
            Email = agent.Email,
            LicenseNumber = agent.LicenseNumber,
            AccessToken = accessToken,
            RefreshToken = newRefreshToken.Token,
            AccessTokenExpiresAt = expiresAt
        };
    }

    // =========================================================================
    // REVOCAR TOKEN (LOGOUT)
    // =========================================================================
    public async Task<bool> RevokeTokenAsync(string token, string? ipAddress)
    {
        var agent = await _context.Agents
            .Include(a => a.RefreshTokens)
            .FirstOrDefaultAsync(a => a.RefreshTokens.Any(t => t.Token == token));

        if (agent == null) return false;

        var refreshToken = agent.RefreshTokens.Single(t => t.Token == token);

        if (refreshToken.RevokedAt != null || refreshToken.ExpiresAt <= DateTime.UtcNow)
            return false;

        refreshToken.RevokedAt = DateTime.UtcNow;
        refreshToken.RevokedByIp = ipAddress;
        refreshToken.ReasonRevoked = "Cierre de sesión manual";

        await _context.SaveChangesAsync();
        return true;
    }

    // =========================================================================
    // MÉTODOS PRIVADOS AUXILIARES
    // =========================================================================

    private string GenerateJwtToken(Agent agent, out DateTime expiresAt)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSettings.Key);

        expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, agent.AgentGuid.ToString()),
            new(ClaimTypes.Email, agent.Email),
            new(ClaimTypes.Name, $"{agent.FirstName} {agent.LastName}"),
            new("LicenseNumber", agent.LicenseNumber)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAt,
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private AgentRefreshToken GenerateRefreshToken(string? ipAddress)
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);

        return new AgentRefreshToken
        {
            Token = Convert.ToBase64String(randomBytes),
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenDurationInDays),
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = ipAddress
        };
    }

    private async Task LogLoginAttemptAsync(int? agentId, string email, bool isSuccessful, string? failureReason, string? ipAddress, string? userAgent)
    {
        var log = new AgentLoginLog
        {
            AgentId = agentId,
            AttemptedEmail = email,
            IsSuccessful = isSuccessful,
            FailureReason = failureReason,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            LoginAt = DateTime.UtcNow
        };

        _context.AgentLoginLogs.Add(log);
        await _context.SaveChangesAsync();
    }

    private static void RevokeAllChildTokens(Agent agent, string? ipAddress, string reason)
    {
        foreach (var token in agent.RefreshTokens.Where(t => t.RevokedAt == null))
        {
            token.RevokedAt = DateTime.UtcNow;
            token.RevokedByIp = ipAddress;
            token.ReasonRevoked = reason;
        }
    }
}