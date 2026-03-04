namespace competitions.Domain.Models;

public class DivisionGroup
{
    public string Id { get; set; }
    public string DivisionId { get; set; }
    public string LeagueId { get; set; }
    public string Name { get; set; }
    public string Slug { get; set; }
    public int Order { get; set; }
    public string TenantId { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<DivisionGroupTeam> Teams { get; set; } = new();
}