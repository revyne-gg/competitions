using competitions.Domain.Competitions.Tournaments.Models;

namespace competitions.Infrastructure.Entities;

public class TournamentStageEntity
{
    public long Id { get; set; }
    public string CompetitionId { get; set; }
    public CompetitionEntity Competition { get; set; }
    public string Name { get; set; }
    public TournamentFormat Format { get; set; }
    public int Order { get; set; }
    public int Advancing { get; set; }
    public string TenantId { get; set; }

    // Format-specific config stored as JSON
    public string? FormatConfigJson { get; set; }
}
