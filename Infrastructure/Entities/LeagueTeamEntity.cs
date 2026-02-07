namespace competitions.Infrastructure.Entities;

public class LeagueTeamEntity
{
    public int Id { get; set; }
    public required string LeagueId { get; set; }
    public required string TeamId { get; set; }
    public LeagueEntity League { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required string TenantId { get; set; }
}