using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InsuranceAgentAPI.Entities;

[Table("Clients")]
public class Client
{
    [Key]
    public int Id { get; set; }

    // ID del agente propietario (Clave foránea)
    [Required]
    public string AgentId { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? IdentificationNumber { get; set; }

    public DateTime? BirthDate { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [Required]
    [MaxLength(20)]
    public int ClientType { get; set; } = 1;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Propiedad de navegación a Pólizas
    public ICollection<Policy> Policies { get; set; } = new List<Policy>();
}