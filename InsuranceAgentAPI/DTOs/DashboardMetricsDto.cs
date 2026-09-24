namespace InsuranceAgentAPI.DTOs;

public class DashboardMetricsDto
{
    public int TotalActiveClients { get; set; }
    public int ActivePolicies { get; set; }
    public int ExpiringPolicies { get; set; }
}