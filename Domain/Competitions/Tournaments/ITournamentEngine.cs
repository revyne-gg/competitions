using competitions.Domain.Competitions.Matches.Models;
using competitions.Domain.Competitions.Shared.Models;
using competitions.Domain.Competitions.Tournaments.Models;
using competitions.Domain.Models;
using competitions.Shared;

namespace competitions.Domain.Competitions.Tournaments;

public interface ITournamentEngine
{
    TournamentFormat Format { get; }
    Task<Result<Competition, CompetitionError>> Setup(CompetitionConfig config);
    Task<Result<Unit, CompetitionError>> OnMatchFinished(Match match);
    Task<Result<Unit, CompetitionError>> Tick();
}