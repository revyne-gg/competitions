using competitions.Domain.Models;

namespace competitions.Domain.Competitions.Leagues.Models;

public class LeagueConfig : CompetitionConfig
{
    public DateTime? RegistrationPeriodStart { get; set; }
    public DateTime? RegistrationPeriodEnd { get; set; }
    public DateTime? LeaguePeriodStart { get; set; }
    public DateTime? LeaguePeriodEnd { get; set; }
}
