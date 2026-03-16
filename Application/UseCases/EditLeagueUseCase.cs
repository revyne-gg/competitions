using competitions.Application.Ports;
using competitions.Domain.Competitions.Leagues.Models;
using competitions.Domain.Models;
using competitions.Shared;

namespace competitions.Application.UseCases;

public sealed class EditLeagueUseCase(
    ILeagueRepository repo,
    IPermissionService permissionService
)
{
    public async Task<Result<League, AppError>> Execute(
        string leagueId,
        string? name,
        string? description,
        string? game,
        int? bestOf,
        List<string>? mapPool,
        LeagueLegs? legs,
        LeagueStatus? state,
        bool? isRegistrationOpen,
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

        var leagueResult = await repo.GetByIdAsync(leagueId, tenantId);
        if (leagueResult.IsFailure)
            return leagueResult.Error == RepositoryError.NotFound ? AppError.NotFound : AppError.InternalError;

        var league = leagueResult.Value!;

        if (league.HasStarted)
        {
            bool hasConfigChanges =
                game is not null ||
                bestOf is not null ||
                mapPool is not null ||
                legs is not null ||
                state is not null ||
                isRegistrationOpen is not null ||
                isAutoAccept is not null;

            if (hasConfigChanges) return AppError.BadRequest;
        }

        if (name is not null) league.Name = name;
        if (description is not null) league.Description = description;

        if (!league.HasStarted)
        {
            if (game is not null) league.Game = game;
            if (bestOf is not null) league.BestOf = bestOf.Value;
            if (mapPool is not null) league.MapPool = mapPool;
            if (legs is not null) league.Legs = legs.Value;
            if (state is not null) league.State = state.Value;
            if (isRegistrationOpen is not null) league.IsRegistrationOpen = isRegistrationOpen.Value;
            if (isAutoAccept is not null) league.IsAutoAccept = isAutoAccept.Value;
        }

        var updateResult = await repo.Update(league);
        if (updateResult.IsFailure) return AppError.InternalError;

        return league;
    }
}
