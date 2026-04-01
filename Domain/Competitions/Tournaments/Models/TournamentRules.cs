namespace competitions.Domain.Competitions.Tournaments.Models;

public class TournamentRules
{
    public string TournamentId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string TenantId { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
