using competitions.Domain.Competitions.Tournaments.Models;

namespace competitions.Infrastructure.Entities;

public class TournamentConfigEntity
{
    public long Id { get; set; }
    public string CompetitionId { get; set; }
    public CompetitionEntity Competition { get; set; }
    public TournamentFormat Format { get; set; }
    public SeedingType SeedingType { get; set; } = SeedingType.Standard;
    public bool BracketReset { get; set; }
    public int MaxParticipants { get; set; }
    public string? OrganiserId { get; set; }
    public string? RealmId { get; set; }
    public string TenantId { get; set; }
}
