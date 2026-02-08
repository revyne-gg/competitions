namespace competitions.Domain.Models;

public class RosterPlayer
{
    public int RosterId { get; set; }
    public required string PlayerId { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required DateTime JoinedAt { get; set; }
    public DateTime? LeftAt { get; set; }
    public required string TenantId { get; set; }
}