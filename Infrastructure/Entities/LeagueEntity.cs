namespace competitions.Infrastructure.Entities;

public class LeagueEntity
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Slug { get; set; }
    public string Description { get; set; }
    public List<LeagueTeamEntity> Teams { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
    public string OrganiserId { get; set; }
    public string OrganiserSlug { get; set; }
    public string RealmId { get; set; }
    public string RealmSlug { get; set; }
    public string TenantId { get; set; }
}