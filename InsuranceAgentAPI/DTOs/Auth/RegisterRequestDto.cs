using System.ComponentModel.DataAnnotations;

namespace InsuranceAgentAPI.DTOs.Auth;

public class RegisterRequestDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(50, ErrorMessage = "El nombre no puede exceder 50 caracteres.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [StringLength(50, ErrorMessage = "El apellido no puede exceder 50 caracteres.")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "El formato de correo es inválido.")]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "El número telefónico no es válido.")]
    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    [Required(ErrorMessage = "El número de licencia es obligatorio.")]
    [StringLength(50)]
    public string LicenseNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    public string Password { get; set; } = string.Empty;
}