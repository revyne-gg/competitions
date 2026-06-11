using competitions.Application.Mapping;
using competitions.Application.Ports;
using competitions.Shared;
using Engine = Revyne.Engine.Api;

namespace competitions.Application.UseCases;

public sealed class ReportMatchScoreUseCase(
    IMatchRepository matchRepository,
    Engine.ICompetitionEngine engine
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

        var outcome = engine.ResolveOutcome(match.Competition.ToEngineConfig(), match.ToFinishedMatch());
        if (outcome.IsFailure)
        {
            return outcome.Error.ToAppError();
        }

        match.WinnerTeamId = outcome.Value!.WinnerTeamId;
        match.LoserTeamId = outcome.Value.LoserTeamId;

        var updateResult = await matchRepository.Update(match);
        if (updateResult.IsFailure)
        {
            return AppError.InternalError;
        }

        return Unit.Value;
    }
}
