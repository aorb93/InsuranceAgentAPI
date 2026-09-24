using InsuranceAgentAPI.DTOs.Auth;

namespace InsuranceAgentAPI.Services;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, string? ipAddress);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request, string? ipAddress, string? userAgent);
    Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request, string? ipAddress);
    Task<bool> RevokeTokenAsync(string token, string? ipAddress);
}