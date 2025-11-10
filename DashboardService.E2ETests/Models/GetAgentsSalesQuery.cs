namespace DashboardService.E2ETests.Models;

public class GetAgentsSalesQuery
{
    public string? AgentLogin { get; set; }
    public string? ProductCode { get; set; }
    public DateTime SalesDateFrom { get; set; }
    public DateTime SalesDateTo { get; set; }
}
