using competitions.Application.Ports;
using competitions.Domain.Competitions.Leagues.Models;
using competitions.Domain.Competitions.Tournaments.Models;
using competitions.Shared;

namespace competitions.Application.UseCases;

public sealed class DeleteTournamentUseCase(
    ITournamentRepository repo,
    IPermissionService permissionService
)
{
    public async Task<Result<Unit, AppError>> Execute(
        string tournamentId,
        string organiserId,
        string userId,
        string tenantId
    )
    {
        var role = await permissionService.GetRoleForUserInOrganiser(userId, organiserId);
        if (role.IsFailure) return AppError.InternalError;
        if (role.Value is not (OrganiserMemberRole.Owner or OrganiserMemberRole.Manager))
            return AppError.Forbidden;

        var tournamentResult = await repo.GetByIdAsync(tournamentId, tenantId);
        if (tournamentResult.IsFailure)
            return tournamentResult.Error == RepositoryError.NotFound ? AppError.NotFound : AppError.InternalError;

        var tournament = tournamentResult.Value!;
        tournament.Anonymise(userId);

        var updateResult = await repo.Update(tournament);
        if (updateResult.IsFailure) return AppError.InternalError;

        return Unit.Value;
    }
}
