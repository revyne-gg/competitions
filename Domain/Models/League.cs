namespace leagues.Domain.Models;

public class League
{
    public string Id { get; set; }

    public string Name
    {
        get;
        set
        {
            field = value;
            _slug = value.ToLower().Replace(" ", "-");
        }
    }

    public string Description { get; set; }
    public string Slug => _slug;
    public DateTime CreatedAt { get; set; }
    public string OrganiserId { get; set; }
    public string OrganiserSlug { get; set; }
    public string RealmId { get; set; }
    public string RealmSlug { get; set; }
    public string TenantId { get; set; }
    public List<LeagueTeam> Teams { get; set; }
    public bool IsDeleted { get; private set; } = false;
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    private string _slug;

    public League(
        string id,
        string name,
        string description,
        string organiserId,
        string organiserSlug,
        string realmId,
        string realmSlug,
        string tenantId
    )
    {
        Id = $"league_{id}";
        Name = name;
        Description = description;
        OrganiserId = organiserId;
        OrganiserSlug = organiserSlug;
        RealmId = realmId;
        RealmSlug = realmSlug;
        TenantId = tenantId;
        Teams = [];
        CreatedAt = DateTime.UtcNow;
        IsDeleted = false;
        DeletedAt = null;
        DeletedBy = null;
    }

    private League(): this("", "", "", "", "", "", "", "")
    {
    }
    
    public bool CanBeDeletedBy(OrganiserMemberRole role) => role is OrganiserMemberRole.Owner or OrganiserMemberRole.Manager;
    public bool CanBeUpdatedBy(OrganiserMemberRole role) => role is OrganiserMemberRole.Owner or OrganiserMemberRole.Manager;
    public bool CanCreateLeague(OrganiserMemberRole role) => role is OrganiserMemberRole.Owner or OrganiserMemberRole.Manager;
    public bool CanBeDeletedBy(RealmMemberRole role) => role == RealmMemberRole.Admin;
    public bool CanBeUpdatedBy(RealmMemberRole role) => role is RealmMemberRole.Admin;
    public bool CanCreateLeague(RealmMemberRole role) => role is RealmMemberRole.Admin;

    public void Anonymise(string deletedBy)
    {
        if (IsDeleted)
        {
            return;
        }
        
        Name = $"Deleted-{Guid.CreateVersion7()}";
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;
    }
}