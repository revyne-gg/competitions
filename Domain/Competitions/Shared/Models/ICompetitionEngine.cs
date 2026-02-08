using competitions.Domain.Competitions.Matches.Models;
using competitions.Domain.Models;
using competitions.Shared;

namespace competitions.Domain.Competitions.Shared.Models;

public interface ICompetitionEngine
{
    CompetitionType Type { get; }
    Task<Result<Competition, CompetitionError>> Setup(CompetitionConfig config);
    Task<Result<Unit, CompetitionError>> OnMatchFinished(Match match);
    Task<Result<Unit, CompetitionError>> Tick();
}