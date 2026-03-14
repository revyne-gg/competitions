using competitions.Application.Ports;
using competitions.Domain.Competitions.Leagues.Models;
using competitions.Domain.Competitions.Matches.Models;
using competitions.Shared;

namespace competitions.Application.UseCases;

public sealed class CreateMatchesForLeagueUseCase(
    ILeagueRepository leagueRepo,
    IMatchRepository matchRepo,
    IIDGenerator idGenerator
)
{
    public async Task<Result<List<Match>, AppError>> Execute(string leagueId, string tenantId)
    {
        var leagueResult = await leagueRepo.GetByIdAsync(leagueId, tenantId);
        if (leagueResult.IsFailure)
            return leagueResult.Error == RepositoryError.NotFound ? AppError.NotFound : AppError.InternalError;

        var league = leagueResult.Value;

        var divisionsResult = await leagueRepo.GetDivisionsByLeagueAsync(leagueId, tenantId);
        if (divisionsResult.IsFailure)
            return AppError.InternalError;

        var allMatches = new List<Match>();
        var now = DateTime.UtcNow;

        foreach (var division in divisionsResult.Value)
        {
            var groupsResult = await leagueRepo.GetDivisionGroupsByDivisionAsync(division.Id, tenantId);
            if (groupsResult.IsFailure)
                return AppError.InternalError;

            foreach (var group in groupsResult.Value)
            {
                var teams = group.Teams.Select(t => t.TeamId).ToList();
                if (teams.Count < 2)
                    continue;

                var pairs = GenerateRoundRobinPairs(teams);

                foreach (var (home, away) in pairs)
                {
                    var rawId = await idGenerator.Generate();
                    allMatches.Add(new Match
                    {
                        Id = $"match_{rawId}",
                        CompetitionId = leagueId,
                        HomeTeamId = home,
                        AwayTeamId = away,
                        TenantId = tenantId,
                        CreatedAt = now,
                        Meta = new MatchMeta(),
                    });
                }

                if (league.Legs == LeagueLegs.TwoLegs)
                {
                    foreach (var (home, away) in pairs)
                    {
                        var rawId = await idGenerator.Generate();
                        allMatches.Add(new Match
                        {
                            Id = $"match_{rawId}",
                            CompetitionId = leagueId,
                            HomeTeamId = away,
                            AwayTeamId = home,
                            TenantId = tenantId,
                            CreatedAt = now,
                            Meta = new MatchMeta(),
                        });
                    }
                }
            }
        }

        if (allMatches.Count == 0)
            return AppError.BadRequest;

        var saveResult = await matchRepo.AddRangeAsync(allMatches);
        if (saveResult.IsFailure)
            return AppError.InternalError;

        return allMatches;
    }

    private static List<(string Home, string Away)> GenerateRoundRobinPairs(List<string> teams)
    {
        var pairs = new List<(string, string)>();
        for (int i = 0; i < teams.Count; i++)
        {
            for (int j = i + 1; j < teams.Count; j++)
            {
                pairs.Add((teams[i], teams[j]));
            }
        }
        return pairs;
    }
}
