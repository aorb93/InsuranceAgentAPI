namespace InsuranceAgentAPI.Entities;

public class AgentLoginLog
{
    public long LoginLogId { get; set; }
    public int? AgentId { get; set; }
    public string AttemptedEmail { get; set; } = string.Empty;
    public bool IsSuccessful { get; set; }
    public string? FailureReason { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime LoginAt { get; set; } = DateTime.UtcNow;

    // Propiedad de navegación
    public Agent? Agent { get; set; }
}