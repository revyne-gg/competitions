namespace competitions.Infrastructure.Entities;

public class RosterEntity
{
    public int Id { get; set; }
    public string LeagueTeamId { get; set; }
    public LeagueTeamEntity LeagueTeam { get; set; }
    public List<RosterPlayerEntity> Players { get; set; }
    public DateTime CreatedAt { get; set; }
    public string TenantId { get; set; }
}