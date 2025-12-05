namespace leagues.Infrastructure.Entities;

public class LeagueTeamEntity
{
    public int Id { get; set; }
    public string LeagueId { get; set; }
    public string TeamId { get; set; }
    public LeagueEntity League { get; set; }
    public DateTime CreatedAt { get; set; }
    public string TenantId { get; set; }
}