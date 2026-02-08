using competitions.Domain.Competitions.Shared.Models;
using competitions.Domain.Models;

namespace competitions.Domain.Competitions.Tournaments.Models;

public class Tournament : Competition
{
    public override CompetitionType Type => CompetitionType.Tournament;
    public TournamentFormat Format { get; set; }
}