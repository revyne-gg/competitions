using competitions.Domain.Competitions.Leagues.Models;
using competitions.Domain.Models;

namespace competitions.Domain.Competitions.Shared.Models;

public abstract class Competition
{
    public virtual CompetitionType Type { get; set; }
    public string Id { get; set; }
    public string Name { get; set; }
    public string Discriminator { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public string RealmId { get; set; }
    public string OrganiserId { get; set; }
    public string TenantId { get; set; }
    
    public bool IsDeleted { get; set; }
    public string? DeletedBy { get; set; }
    public DateTime? DeletedAt { get; set; }

    public virtual void Anonymise(string deletedBy)
    {
        Name = $"Deleted-{Guid.CreateVersion7()}";
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;
    }
        
    public bool CanBeDeletedBy(OrganiserMemberRole role) => role is OrganiserMemberRole.Owner or OrganiserMemberRole.Manager;
    public bool CanBeUpdatedBy(OrganiserMemberRole role) => role is OrganiserMemberRole.Owner or OrganiserMemberRole.Manager;
    public bool CanCreateLeague(OrganiserMemberRole role) => role is OrganiserMemberRole.Owner or OrganiserMemberRole.Manager;
    public bool CanBeDeletedBy(RealmMemberRole role) => role == RealmMemberRole.Admin;
    public bool CanBeUpdatedBy(RealmMemberRole role) => role is RealmMemberRole.Admin;
    public bool CanCreateLeague(RealmMemberRole role) => role is RealmMemberRole.Admin;
}