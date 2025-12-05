namespace leagues.Domain.Models;

public class RosterPlayer
{
    public int RosterId { get; set; }
    public string PlayerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime JoinedAt { get; set; }
    public DateTime? LeftAt { get; set; }
    public string TenantId { get; set; }
}