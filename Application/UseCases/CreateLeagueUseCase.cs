using competitions.Application.DTO;
using competitions.Application.Ports;
using competitions.Domain.Competitions.Leagues.Models;
using competitions.Domain.Models;
using competitions.Shared;

namespace competitions.Application.UseCases;

public sealed class CreateLeagueUseCase(
    ILeagueRepository repo,
    IPermissionService permissionService,
    IIDGenerator idGenerator
)
{
    public async Task<Result<LeagueDTO, AppError>> Execute(
        string name, 
        string description,
        string organiserId, 
        string organiserSlug, 
        string realmId, 
        string realmSlug, 
        string userId, 
        string tenantId
    )
    {
        var role = await permissionService.GetRoleForUserInOrganiser(userId, organiserId);
        if (role.IsFailure)
        {
            return AppError.InternalError;
        }

        var hasPermission = role.Value is OrganiserMemberRole.Owner or OrganiserMemberRole.Manager;

        if (hasPermission)
        {
            var realmRole = await permissionService.GetRoleForUserInRealm(userId, realmId);
            if (realmRole.IsFailure)
            {
                return AppError.InternalError;
            }

            hasPermission = realmRole.Value is RealmMemberRole.Admin;
        }

        if (!hasPermission)
        {
            return AppError.Forbidden;
        }
        
        var existingLeague = await repo.GetByNameAsync(name, realmId, tenantId);
        if (existingLeague.IsFailure)
        {
            return AppError.InternalError;
        }

        if (existingLeague.Value is not null)
        {
            return AppError.Conflict;
        }
        
        var leagueId = await idGenerator.Generate();

        var league = new League(leagueId, name, description, organiserId, organiserSlug, realmId, realmSlug, tenantId);

        await repo.AddAsync(league);

        return league.ToDto();
    }
}