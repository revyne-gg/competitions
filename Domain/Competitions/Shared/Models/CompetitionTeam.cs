using competitions.Infrastructure.Entities;

namespace competitions.Domain.Models;

public class CompetitionTeam
{
    public required string LeagueId { get; set; }
    public required string TeamId { get; set; }
    public required string TenantId { get; set; }
    public required DateTime CreatedAt { get; set; }
}