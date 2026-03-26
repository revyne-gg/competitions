namespace competitions.Domain.Models;

public class TournamentTeam
{
    public required string TournamentId { get; set; }
    public required string TeamId { get; set; }
    public required string TenantId { get; set; }
    public required DateTime CreatedAt { get; set; }
    public RegistrationStatus Status { get; set; } = RegistrationStatus.Pending;
}
