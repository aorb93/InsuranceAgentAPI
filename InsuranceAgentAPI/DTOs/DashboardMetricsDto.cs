namespace InsuranceAgentAPI.DTOs;

public class DashboardMetricsDto
{
    public int TotalActiveClients { get; set; }
    public int ActivePolicies { get; set; }
    public int ExpiringPolicies { get; set; }
    public decimal TotalNetPremium { get; set; }
    public decimal TotalCommissions { get; set; }
}