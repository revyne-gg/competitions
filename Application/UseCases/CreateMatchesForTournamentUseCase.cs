using competitions.Application.Mapping;
using competitions.Application.Ports;
using competitions.Domain.Competitions.Matches.Models;
using competitions.Domain.Competitions.Tournaments.Models;
using competitions.Shared;
using Engine = Revyne.Engine.Api;

namespace competitions.Application.UseCases;

public sealed class CreateMatchesForTournamentUseCase(
    ITournamentRepository tournamentRepo,
    IMatchRepository matchRepo,
    IIDGenerator idGenerator,
    Engine.ICompetitionEngine engine
)
{
    public async Task<Result<List<Match>, AppError>> Execute(
        string tournamentId,
        List<string> teamIds,
        string tenantId
    )
    {
        if (teamIds.Count < 2)
            return AppError.BadRequest;

        var tournamentResult = await tournamentRepo.GetByIdAsync(tournamentId, tenantId);
        if (tournamentResult.IsFailure)
            return tournamentResult.Error == RepositoryError.NotFound ? AppError.NotFound : AppError.InternalError;

        var tournament = tournamentResult.Value;

        if (tournament.Format is not (TournamentFormat.SingleElimination or TournamentFormat.DoubleElimination))
            return AppError.BadRequest;

        var existingResult = await matchRepo.GetByCompetitionIdAsync(tournamentId, tenantId);
        if (existingResult.IsFailure)
            return AppError.InternalError;

        if (existingResult.Value.Count > 0)
            return AppError.Conflict;

        var generated = engine.GenerateInitialMatches(tournament.ToEngineConfig(), teamIds);
        if (generated.IsFailure)
            return generated.Error.ToAppError();

        var now = DateTime.UtcNow;
        var matches = new List<Match>();

        foreach (var spec in generated.Value!)
        {
            var rawId = await idGenerator.Generate();
            matches.Add(new Match
            {
                Id = $"match_{rawId}",
                CompetitionId = tournamentId,
                HomeTeamId = spec.HomeTeamId,
                AwayTeamId = spec.AwayTeamId,
                Round = spec.Round,
                TenantId = tenantId,
                CreatedAt = now,
                Meta = new MatchMeta(),
            });
        }

        var saveResult = await matchRepo.AddRangeAsync(matches);
        if (saveResult.IsFailure)
            return AppError.InternalError;

        return matches;
    }
}
