using competitions.Application.Mapping;
using competitions.Application.Ports;
using competitions.Domain.Competitions.Matches.Models;
using competitions.Shared;
using Engine = Revyne.Engine.Api;

namespace competitions.Application.UseCases;

public sealed class CreateMatchesForLeagueUseCase(
    ILeagueRepository leagueRepo,
    IMatchRepository matchRepo,
    IIDGenerator idGenerator,
    Engine.ICompetitionEngine engine
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

        var engineConfig = league.ToEngineConfig();
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

                // Engine generates the per-group fixtures (incl. second leg).
                var generated = engine.GenerateInitialMatches(engineConfig, teams);
                if (generated.IsFailure)
                    return generated.Error.ToAppError();

                foreach (var spec in generated.Value!)
                {
                    var rawId = await idGenerator.Generate();
                    allMatches.Add(new Match
                    {
                        Id = $"match_{rawId}",
                        CompetitionId = leagueId,
                        HomeTeamId = spec.HomeTeamId,
                        AwayTeamId = spec.AwayTeamId,
                        // Round is not meaningful for league round-robin fixtures.
                        TenantId = tenantId,
                        CreatedAt = now,
                        Meta = new MatchMeta(),
                    });
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
}
