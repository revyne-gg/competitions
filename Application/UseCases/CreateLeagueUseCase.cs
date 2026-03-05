using competitions.Application.Ports;
using competitions.Domain.Competitions.Leagues.Models;
using competitions.Domain.Models;
using competitions.Shared;

namespace competitions.Application.UseCases;

public sealed class CreateLeagueUseCase(
    ILeagueRepository repo,
    IPermissionService permissionService,
    IIDGenerator idGenerator,
    IDiscriminatorGenerator discriminatorGenerator
)
{
    public async Task<Result<League, AppError>> Execute(
        LeagueConfig config,
        string organiserId,
        string realmId, 
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
        
        var discriminator = "";
        bool exist = false;
        do
        {
            discriminator = await discriminatorGenerator.Generate();
            var existing = await repo.GetByNameAndDiscriminatorAsync(config.Name, discriminator, tenantId);
            if (existing.IsFailure)
            {
                if (existing.Error == RepositoryError.NotFound)
                {
                    exist = false;
                }
                else
                {
                    return AppError.InternalError;
                }
            }
            else
            {
                exist = true;
            }
        } while (exist);
        
        var leagueId = await idGenerator.Generate();

        var league = new League
        {
            Id = leagueId,
            Name = config.Name,
            Discriminator = discriminator,
            Description = config.Description,
            RealmId = realmId,
            OrganiserId = organiserId,
            TenantId = tenantId,
            RegistrationPeriodStart = config.RegistrationPeriodStart,
            RegistrationPeriodEnd   = config.RegistrationPeriodEnd,
            LeaguePeriodStart       = config.LeaguePeriodStart,
            LeaguePeriodEnd         = config.LeaguePeriodEnd,
        };

        await repo.AddAsync(league);

        return league;
    }
}