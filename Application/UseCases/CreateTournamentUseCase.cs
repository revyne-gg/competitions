using competitions.Application.Ports;
using competitions.Domain.Competitions.Leagues.Models;
using competitions.Domain.Competitions.Tournaments.Models;
using competitions.Domain.Models;
using competitions.Shared;

namespace competitions.Application.UseCases;

public sealed class CreateTournamentUseCase(
    ITournamentRepository repo,
    IPermissionService permissionService,
    IIDGenerator idGenerator,
    IDiscriminatorGenerator discriminatorGenerator
)
{
    public async Task<Result<Tournament, AppError>> Execute(
        TournamentConfig config,
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
        
        var tournamentId = await idGenerator.Generate();

        var tournament = new Tournament
        {
            Id = $"tournament_{tournamentId}",
            Name = config.Name,
            Format = config.Format,
            SeedingType = config.SeedingType,
            BracketReset = config.BracketReset,
            Discriminator = discriminator,
            Description = config.Description,
            CreatedAt = DateTime.UtcNow,
            TenantId = tenantId,
            OrganiserId = organiserId,
            RealmId = realmId,
            Game = config.Game,
            BestOf = config.BestOf,
            MapPool = config.MapPool,
        };

        var result = await repo.AddAsync(tournament);
        if (result.IsFailure)
        {
            return AppError.InternalError;
        }

        return tournament;
    }
}
