namespace competitions.Domain.Models;

public class Division
{
    public string Id { get; set; }
    public string LeagueId { get; set; }
    public string Name { get; set; }
    public string Slug { get; set; }
    public int Order { get; set; }
    public int BestOf { get; set; }
    public int MaxTeamsPerGroup { get; set; }
    public string TenantId { get; set; }
    public DateTime CreatedAt { get; set; }
}