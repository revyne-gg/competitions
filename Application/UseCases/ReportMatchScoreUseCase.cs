using competitions.Application.Ports;
using competitions.Domain.Competitions.Shared.Models;
using competitions.Domain.Models;
using competitions.Shared;

namespace competitions.Application.UseCases;

public sealed class ReportMatchScoreUseCase(
    IMatchRepository matchRepository,
    IEnumerable<ICompetitionEngine> engines
)
{
    public async Task<Result<Unit, AppError>> Execute(
        string matchId,
        uint scoreHome,
        uint scoreAway,
        string tenantId
    )
    {
        var matchResult = await matchRepository.GetByIdAsync(matchId, tenantId);
        if (matchResult.IsFailure)
        {
            return matchResult.Error switch
            {
                RepositoryError.NotFound => AppError.NotFound,
                _ => AppError.InternalError
            };
        }

        var match = matchResult.Value!;
        match.ScoreHome = (int)scoreHome;
        match.ScoreAway = (int)scoreAway;

        if (match.Competition is null)
        {
            return AppError.BadRequest;
        }

        var engine = engines.FirstOrDefault(e => e.Type == match.Competition.Type);
        if (engine is null)
        {
            return AppError.InternalError;
        }

        var engineResult = await engine.OnMatchFinished(match);
        if (engineResult.IsFailure)
        {
            return engineResult.Error switch
            {
                CompetitionError.InvalidArguments => AppError.BadRequest,
                CompetitionError.NotFound => AppError.NotFound,
                _ => AppError.InternalError
            };
        }

        var updateResult = await matchRepository.Update(match);
        if (updateResult.IsFailure)
        {
            return AppError.InternalError;
        }

        return Unit.Value;
    }
}
