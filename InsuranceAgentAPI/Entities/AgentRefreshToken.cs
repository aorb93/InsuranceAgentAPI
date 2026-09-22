namespace InsuranceAgentAPI.Entities;

public class AgentRefreshToken
{
    public int RefreshTokenId { get; set; }
    public int AgentId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedByIp { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? RevokedByIp { get; set; }
    public string? ReplacedByToken { get; set; }
    public string? ReasonRevoked { get; set; }

    // Propiedad de navegación
    public Agent Agent { get; set; } = null!;
}