using competitions.Application.Ports;
using competitions.Domain.Competitions.Leagues.Models;
using competitions.Domain.Models;
using competitions.Shared;

namespace competitions.Application.UseCases;

public sealed class CreateDivisionUseCase(
    ILeagueRepository repo,
    IPermissionService permissionService,
    IIDGenerator idGenerator
)
{
    public async Task<Result<Division, AppError>> Execute(
        string leagueId,
        string name,
        int order,
        int bestOf,
        int maxTeamsPerGroup,
        string tag,
        string organiserId,
        string userId,
        string tenantId
    )
    {
        var roleResult = await permissionService.GetRoleForUserInOrganiser(userId, organiserId);
        if (roleResult.IsFailure) return AppError.InternalError;
        if (roleResult.Value is not (OrganiserMemberRole.Owner or OrganiserMemberRole.Manager))
            return AppError.Forbidden;

        var leagueResult = await repo.GetByIdAsync(leagueId, tenantId);
        if (leagueResult.IsFailure) return leagueResult.Error == RepositoryError.NotFound ? AppError.NotFound : AppError.InternalError;

        var league = leagueResult.Value!;
        if (!league.CanCreateDivision()) return AppError.BadRequest;

        var id = await idGenerator.Generate();
        var slug = $"{name.ToLower().Replace(" ", "-")}-{tag}";

        var division = new Division
        {
            Id = id,
            LeagueId = leagueId,
            Name = name,
            Slug = slug,
            Order = order,
            BestOf = bestOf,
            MaxTeamsPerGroup = maxTeamsPerGroup,
            TenantId = tenantId,
            CreatedAt = DateTime.UtcNow,
        };

        var addResult = await repo.AddDivisionAsync(division);
        if (addResult.IsFailure) return AppError.InternalError;

        return division;
    }
}
