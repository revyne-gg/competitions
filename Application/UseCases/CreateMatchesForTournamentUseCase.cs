using competitions.Application.Ports;
using competitions.Domain.Competitions.Matches.Models;
using competitions.Domain.Competitions.Tournaments.Models;
using competitions.Shared;

namespace competitions.Application.UseCases;

public sealed class CreateMatchesForTournamentUseCase(
    ITournamentRepository tournamentRepo,
    IMatchRepository matchRepo,
    IIDGenerator idGenerator
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

        // Standard bracket seeding: 1 vs n, 2 vs n-1, ...
        var pairs = SeedBracket(teamIds);
        var now = DateTime.UtcNow;
        var matches = new List<Match>();

        foreach (var (home, away) in pairs)
        {
            var rawId = await idGenerator.Generate();
            matches.Add(new Match
            {
                Id = $"match_{rawId}",
                CompetitionId = tournamentId,
                HomeTeamId = home,
                AwayTeamId = away,
                Round = 1,
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

    // 1 vs n, 2 vs n-1, ... (standard bracket seeding)
    // Middle team (odd count) is skipped and would advance as a bye — caller should ensure even count.
    private static List<(string Home, string Away)> SeedBracket(List<string> teams)
    {
        var pairs = new List<(string, string)>();
        int count = teams.Count;
        for (int i = 0; i < count / 2; i++)
        {
            pairs.Add((teams[i], teams[count - 1 - i]));
        }
        return pairs;
    }
}
