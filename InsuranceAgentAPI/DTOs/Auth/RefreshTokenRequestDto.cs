using System.ComponentModel.DataAnnotations;

namespace InsuranceAgentAPI.DTOs.Auth;

public class RefreshTokenRequestDto
{
    [Required(ErrorMessage = "El Refresh Token es obligatorio.")]
    public string RefreshToken { get; set; } = string.Empty;
}