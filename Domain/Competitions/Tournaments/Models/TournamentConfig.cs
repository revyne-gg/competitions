using competitions.Domain.Models;

namespace competitions.Domain.Competitions.Tournaments.Models;

public class TournamentConfig : CompetitionConfig
{
    public TournamentFormat Format { get; set; }
}
