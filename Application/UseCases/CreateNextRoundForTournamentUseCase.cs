using competitions.Application.Ports;
using competitions.Domain.Competitions.Matches.Models;
using competitions.Domain.Competitions.Tournaments.Models;
using competitions.Shared;

namespace competitions.Application.UseCases;

public sealed class CreateNextRoundForTournamentUseCase(
    ITournamentRepository tournamentRepo,
    IMatchRepository matchRepo,
    IIDGenerator idGenerator
)
{
    public async Task<Result<List<Match>, AppError>> Execute(string tournamentId, string tenantId)
    {
        var tournamentResult = await tournamentRepo.GetByIdAsync(tournamentId, tenantId);
        if (tournamentResult.IsFailure)
            return tournamentResult.Error == RepositoryError.NotFound ? AppError.NotFound : AppError.InternalError;

        var tournament = tournamentResult.Value;

        if (tournament.Format is not (TournamentFormat.SingleElimination or TournamentFormat.DoubleElimination))
            return AppError.BadRequest;

        var matchesResult = await matchRepo.GetByCompetitionIdAsync(tournamentId, tenantId);
        if (matchesResult.IsFailure)
            return AppError.InternalError;

        var allMatches = matchesResult.Value;

        if (allMatches.Count == 0)
            return AppError.BadRequest; // No matches yet; call CreateMatchesForTournament first

        var currentRound = allMatches.Max(m => m.Round ?? 0);
        var currentRoundMatches = allMatches.Where(m => m.Round == currentRound).ToList();

        if (currentRoundMatches.Any(m => m.WinnerTeamId is null))
            return AppError.Conflict; // Current round still in progress

        var winners = currentRoundMatches
            .OrderBy(m => m.CreatedAt)
            .Select(m => m.WinnerTeamId!)
            .ToList();

        if (winners.Count == 1)
            return AppError.Conflict; // Tournament complete — one champion remains

        if (winners.Count % 2 != 0)
            return AppError.BadRequest; // Unexpected odd number of winners

        var now = DateTime.UtcNow;
        var nextRoundMatches = new List<Match>();
        int nextRound = currentRound + 1;

        for (int i = 0; i < winners.Count; i += 2)
        {
            var rawId = await idGenerator.Generate();
            nextRoundMatches.Add(new Match
            {
                Id = $"match_{rawId}",
                CompetitionId = tournamentId,
                HomeTeamId = winners[i],
                AwayTeamId = winners[i + 1],
                Round = nextRound,
                TenantId = tenantId,
                CreatedAt = now,
                Meta = new MatchMeta(),
            });
        }

        var saveResult = await matchRepo.AddRangeAsync(nextRoundMatches);
        if (saveResult.IsFailure)
            return AppError.InternalError;

        return nextRoundMatches;
    }
}
