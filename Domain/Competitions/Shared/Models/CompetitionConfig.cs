namespace competitions.Domain.Models;

public class CompetitionConfig
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string RealmId { get; set; }
    public string TenantId { get; set; }
    public string Game { get; set; }
    public int BestOf { get; set; } = 1;
    public List<string>? MapPool { get; set; }
}