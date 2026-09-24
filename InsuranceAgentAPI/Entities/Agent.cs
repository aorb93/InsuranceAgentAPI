namespace InsuranceAgentAPI.Entities;

public class Agent
{
    public int AgentId { get; set; }
    public Guid AgentGuid { get; set; } = Guid.NewGuid();
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string LicenseNumber { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Propiedades de navegación
    public ICollection<AgentRefreshToken> RefreshTokens { get; set; } = new List<AgentRefreshToken>();
    public ICollection<AgentLoginLog> LoginLogs { get; set; } = new List<AgentLoginLog>();
}