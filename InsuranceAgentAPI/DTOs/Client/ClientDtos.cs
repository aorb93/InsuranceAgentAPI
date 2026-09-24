using System.ComponentModel.DataAnnotations;

namespace InsuranceAgentAPI.DTOs;

public class ClientDto
{
    public int Id { get; set; }
    public Guid Guid { get; set; }
    public string AgentId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? IdentificationNumber { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? City { get; set; }
    public int ClientType { get; set; } = 1; // 1 = Cliente, 2 = Prospecto
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateClientDto
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo electrónico es obligatorio")]
    [EmailAddress(ErrorMessage = "El formato de correo no es válido")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio")]
    public string Phone { get; set; } = string.Empty;

    public string? IdentificationNumber { get; set; }

    public DateTime? BirthDate { get; set; }

    public string? City { get; set; }

    [Required(ErrorMessage = "El tipo de registro es obligatorio")]
    [Range(1, 2, ErrorMessage = "El tipo de registro debe ser 1 (Cliente) o 2 (Prospecto)")]
    public int ClientType { get; set; } = 1;

    public bool IsActive { get; set; } = true;
}

public class UpdateClientDto
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo electrónico es obligatorio")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio")]
    public string Phone { get; set; } = string.Empty;

    public string? IdentificationNumber { get; set; }

    public DateTime? BirthDate { get; set; }

    public string? City { get; set; }

    [Required(ErrorMessage = "El tipo de registro es obligatorio")]
    [Range(1, 2, ErrorMessage = "El tipo de registro debe ser 1 (Cliente) o 2 (Prospecto)")]
    public int ClientType { get; set; } = 1;

    public bool IsActive { get; set; }
}