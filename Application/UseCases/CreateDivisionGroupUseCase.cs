using competitions.Application.Ports;
using competitions.Domain.Competitions.Leagues.Models;
using competitions.Domain.Models;
using competitions.Shared;

namespace competitions.Application.UseCases;

public sealed class CreateDivisionGroupUseCase(
    ILeagueRepository repo,
    IPermissionService permissionService,
    IIDGenerator idGenerator
)
{
    public async Task<Result<DivisionGroup, AppError>> Execute(
        string divisionId,
        string name,
        int order,
        string organiserId,
        string userId,
        string tenantId
    )
    {
        var roleResult = await permissionService.GetRoleForUserInOrganiser(userId, organiserId);
        if (roleResult.IsFailure) return AppError.InternalError;
        if (roleResult.Value is not (OrganiserMemberRole.Owner or OrganiserMemberRole.Manager))
            return AppError.Forbidden;

        var divisionResult = await repo.GetDivisionByIdAsync(divisionId, tenantId);
        if (divisionResult.IsFailure)
            return divisionResult.Error == RepositoryError.NotFound ? AppError.NotFound : AppError.InternalError;

        var division = divisionResult.Value!;

        var leagueResult = await repo.GetByIdAsync(division.LeagueId, tenantId);
        if (leagueResult.IsFailure)
            return leagueResult.Error == RepositoryError.NotFound ? AppError.NotFound : AppError.InternalError;

        if (!leagueResult.Value!.CanCreateDivision()) return AppError.BadRequest;

        var id = await idGenerator.Generate();
        var slug = $"{name.ToLower().Replace(" ", "-")}-{id[..6]}";

        var group = new DivisionGroup
        {
            Id = id,
            DivisionId = divisionId,
            LeagueId = division.LeagueId,
            Name = name,
            Slug = slug,
            Order = order,
            TenantId = tenantId,
            CreatedAt = DateTime.UtcNow,
        };

        var addResult = await repo.AddDivisionGroupAsync(group);
        if (addResult.IsFailure) return AppError.InternalError;

        return group;
    }
}
