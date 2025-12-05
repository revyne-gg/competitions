using leagues.Infrastructure.Entities;

namespace leagues.Domain.Models;

public class LeagueTeam
{
    public string LeagueId { get; set; }
    public string TeamId { get; set; }
    public string TenantId { get; set; }
    public DateTime CreatedAt { get; set; }
}