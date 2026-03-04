using competitions.Application.Ports;
using competitions.Domain.Competitions.Leagues.Models;
using competitions.Domain.Models;
using competitions.Shared;

namespace competitions.Application.UseCases;

public sealed class UnregisterTeamFromLeagueUseCase(
    ILeagueRepository repo,
    IPermissionService permissionService
)
{
    public async Task<Result<Unit, AppError>> Execute(
        string leagueId,
        string teamId,
        string userId,
        string tenantId
    )
    {
        var teamRoleResult = await permissionService.GetRoleForUserInTeam(userId, teamId);
        if (teamRoleResult.IsFailure) return AppError.InternalError;
        if (teamRoleResult.Value is not (TeamMemberRole.Captain or TeamMemberRole.Member))
            return AppError.Forbidden;

        var leagueResult = await repo.GetByIdAsync(leagueId, tenantId);
        if (leagueResult.IsFailure)
            return leagueResult.Error == RepositoryError.NotFound ? AppError.NotFound : AppError.InternalError;

        if (leagueResult.Value!.HasStarted) return AppError.BadRequest;

        var unregResult = await repo.UnregisterTeamAsync(leagueId, teamId, tenantId);
        if (unregResult.IsFailure)
            return unregResult.Error == RepositoryError.NotFound ? AppError.NotFound : AppError.InternalError;

        await permissionService.RemoveTeamFromLeague(teamId, leagueId);

        return Unit.Value;
    }
}
