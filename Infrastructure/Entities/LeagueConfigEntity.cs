using competitions.Domain.Models;
using competitions.Domain.Competitions.Leagues.Models;

namespace competitions.Infrastructure.Entities;

public class LeagueConfigEntity
{
    public long Id { get; set; }
    public string CompetitionId { get; set; }
    public CompetitionEntity Competition { get; set; }
    public string Description { get; set; }
    public string OrganiserId { get; set; }
    public string OrganiserSlug { get; set; }
    public string RealmId { get; set; }
    public string RealmSlug { get; set; }
    public LeagueStatus State { get; set; } = LeagueStatus.Hidden;
    public LeagueLegs Legs { get; set; } = LeagueLegs.OneLeg;
    public bool IsRegistrationOpen { get; set; }
    public DateTime? RegistrationPeriodStart { get; set; }
    public DateTime? RegistrationPeriodEnd { get; set; }
    public DateTime? LeaguePeriodStart { get; set; }
    public DateTime? LeaguePeriodEnd { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
    public string TenantId { get; set; }
}
