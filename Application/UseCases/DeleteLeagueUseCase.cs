using competitions.Application.Ports;
using competitions.Domain.Competitions.Leagues.Models;
using competitions.Domain.Models;
using competitions.Shared;

namespace competitions.Application.UseCases;

public sealed class DeleteLeagueUseCase(
    ILeagueRepository repo,
    IPermissionService permissionService
)
{
    public async Task<Result<Unit, AppError>> Execute(
        string leagueId,
        string organiserId,
        string userId,
        string tenantId
    )
    {
        var role = await permissionService.GetRoleForUserInOrganiser(userId, organiserId);
        if (role.IsFailure) return AppError.InternalError;
        if (role.Value is not (OrganiserMemberRole.Owner or OrganiserMemberRole.Manager))
            return AppError.Forbidden;

        var leagueResult = await repo.GetByIdAsync(leagueId, tenantId);
        if (leagueResult.IsFailure)
            return leagueResult.Error == RepositoryError.NotFound ? AppError.NotFound : AppError.InternalError;

        var league = leagueResult.Value!;
        league.Anonymise(userId);

        var updateResult = await repo.Update(league);
        if (updateResult.IsFailure) return AppError.InternalError;

        return Unit.Value;
    }
}
