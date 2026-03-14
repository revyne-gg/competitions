using competitions.Domain.Competitions.Leagues.Models;
using competitions.Domain.Competitions.Shared.Models;

namespace competitions.Domain.Models;

public class League : Competition
{
    public override CompetitionType Type => CompetitionType.League;

    public LeagueStatus State { get; set; } = LeagueStatus.Hidden;
    public LeagueLegs Legs { get; set; } = LeagueLegs.OneLeg;
    public bool IsRegistrationOpen { get; set; }
    public DateTime? RegistrationPeriodStart { get; set; }
    public DateTime? RegistrationPeriodEnd { get; set; }
    public DateTime? LeaguePeriodStart { get; set; }
    public DateTime? LeaguePeriodEnd { get; set; }

    public bool HasStarted => State is LeagueStatus.Live or LeagueStatus.Finished;

    public bool CanCreateDivision() => !HasStarted;
    public bool CanRegisterTeam(OrganiserMemberRole role) => IsRegistrationOpen && role is OrganiserMemberRole.Owner or OrganiserMemberRole.Manager;
}