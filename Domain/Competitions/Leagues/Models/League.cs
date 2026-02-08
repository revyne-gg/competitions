using competitions.Domain.Competitions.Leagues.Models;
using competitions.Domain.Competitions.Shared.Models;

namespace competitions.Domain.Models;

public class League : Competition
{
    public override CompetitionType Type => CompetitionType.League;
}