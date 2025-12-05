namespace leagues.Infrastructure.Entities;

public class RosterPlayerEntity
{
    public int Id { get; set; }
    public int RosterId { get; set; }
    public string PlayerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime JoinedAt { get; set; }
    public DateTime? LeftAt { get; set; }
    public RosterEntity Roster { get; set; }
    public string TenantId { get; set; }
}