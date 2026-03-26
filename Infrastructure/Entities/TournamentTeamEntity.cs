using competitions.Domain.Models;

namespace competitions.Infrastructure.Entities;

public class TournamentTeamEntity
{
    public long Id { get; set; }
    public required string TournamentId { get; set; }
    public required string TeamId { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required string TenantId { get; set; }
    public RegistrationStatus Status { get; set; } = RegistrationStatus.Pending;
}
