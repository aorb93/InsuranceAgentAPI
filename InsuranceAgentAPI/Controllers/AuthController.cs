using InsuranceAgentAPI.DTOs.Auth;
using InsuranceAgentAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceAgentAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Registrar un nuevo agente de seguros
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        try
        {
            var response = await _authService.RegisterAsync(request, GetIpAddress());
            return Ok(response);
        }
        catch (BadHttpRequestException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Autenticar un agente y generar AccessToken + RefreshToken
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            var response = await _authService.LoginAsync(request, GetIpAddress(), GetUserAgent());
            return Ok(response);
        }
        catch (BadHttpRequestException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Renovar un AccessToken expirado mediante un RefreshToken válido
    /// </summary>
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        try
        {
            var response = await _authService.RefreshTokenAsync(request, GetIpAddress());
            return Ok(response);
        }
        catch (BadHttpRequestException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Revocar manualmente un RefreshToken (Cierre de sesión)
    /// </summary>
    [Authorize]
    [HttpPost("revoke-token")]
    public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenRequestDto request)
    {
        var result = await _authService.RevokeTokenAsync(request.RefreshToken, GetIpAddress());
        if (!result)
            return BadRequest(new { message = "El token es inválido, ha expirado o ya fue revocado previamente." });

        return Ok(new { message = "Refresh token revocado exitosamente." });
    }

    // =========================================================================
    // MÉTODOS DE EXTRACCIÓN DE METADATOS HTTP
    // =========================================================================

    private string? GetIpAddress()
    {
        if (Request.Headers.ContainsKey("X-Forwarded-For"))
            return Request.Headers["X-Forwarded-For"];

        return HttpContext.Connection.RemoteIpAddress?.ToString();
    }

    private string? GetUserAgent()
    {
        return Request.Headers.UserAgent.ToString();
    }
}