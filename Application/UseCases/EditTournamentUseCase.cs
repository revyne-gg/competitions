using competitions.Application.Ports;
using competitions.Domain.Competitions.Leagues.Models;
using competitions.Domain.Competitions.Tournaments.Models;
using competitions.Shared;

namespace competitions.Application.UseCases;

public sealed class EditTournamentUseCase(
    ITournamentRepository repo,
    IPermissionService permissionService
)
{
    public async Task<Result<Tournament, AppError>> Execute(
        string tournamentId,
        string? name,
        string? description,
        string? game,
        int? bestOf,
        List<string>? mapPool,
        bool? isAutoAccept,
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

        if (name is not null) tournament.Name = name;
        if (description is not null) tournament.Description = description;
        if (game is not null) tournament.Game = game;
        if (bestOf is not null) tournament.BestOf = bestOf.Value;
        if (mapPool is not null) tournament.MapPool = mapPool;
        if (isAutoAccept is not null) tournament.IsAutoAccept = isAutoAccept.Value;

        var updateResult = await repo.Update(tournament);
        if (updateResult.IsFailure) return AppError.InternalError;

        return tournament;
    }
}
