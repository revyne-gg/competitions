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
    IDiscriminatorGenerator discriminatorGenerator,
    ILogger<CreateTournamentUseCase> logger
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
        logger.LogWarning("CreateTournament permission check: userId={UserId}, organiserId={OrganiserId}, realmId={RealmId}", userId, organiserId, realmId);

        var role = await permissionService.GetRoleForUserInOrganiser(userId, organiserId);
        logger.LogWarning("Organiser role check result: IsSuccess={IsSuccess}, Value={Value}", role.IsSuccess, role.IsSuccess ? role.Value.ToString() : "N/A");
        var hasPermission = role.IsSuccess && role.Value is OrganiserMemberRole.Owner or OrganiserMemberRole.Manager;

        if (!hasPermission)
        {
            var realmRole = await permissionService.GetRoleForUserInRealm(userId, realmId);
            logger.LogWarning("Realm role check result: IsSuccess={IsSuccess}, Value={Value}", realmRole.IsSuccess, realmRole.IsSuccess ? realmRole.Value.ToString() : "N/A");
            hasPermission = realmRole.IsSuccess && realmRole.Value is RealmMemberRole.Admin;
        }

        if (!hasPermission)
        {
            logger.LogWarning("Permission denied for userId={UserId}", userId);
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
        
        // Validate stages
        if (config.Stages.Count > 0)
        {
            for (int i = 0; i < config.Stages.Count; i++)
            {
                var stage = config.Stages[i];
                var isLastStage = i == config.Stages.Count - 1;

                // Last stage must not have advancing teams
                if (isLastStage && stage.Advancing > 0)
                    return AppError.BadRequest;

                // Non-last stages must advance at least 2 teams
                if (!isLastStage && stage.Advancing < 2)
                    return AppError.BadRequest;

                // Advancing teams cannot exceed max participants
                if (config.MaxParticipants > 0 && stage.Advancing > config.MaxParticipants)
                    return AppError.BadRequest;

                // Each subsequent stage's participants can't exceed previous stage's advancing count
                if (i > 0 && config.Stages[i - 1].Advancing > 0 && stage.Advancing > config.Stages[i - 1].Advancing)
                    return AppError.BadRequest;
            }
        }

        var tournamentId = await idGenerator.Generate();

        var tournament = new Tournament
        {
            Id = $"tournament_{tournamentId}",
            Name = config.Name,
            Format = config.Format,
            SeedingType = config.SeedingType,
            BracketReset = config.BracketReset,
            MaxParticipants = config.MaxParticipants,
            Discriminator = discriminator,
            Description = config.Description,
            CreatedAt = DateTime.UtcNow,
            TenantId = tenantId,
            OrganiserId = organiserId,
            RealmId = realmId,
            Game = config.Game,
            BestOf = config.BestOf,
            MapPool = config.MapPool,
            Stages = config.Stages.Select((s, idx) => new Stage
            {
                Name = s.Name,
                Format = s.Format,
                Order = idx,
                Advancing = s.Advancing,
                SingleEliminationConfig = s.SingleEliminationConfig,
                DoubleEliminationConfig = s.DoubleEliminationConfig,
                SwissConfig = s.SwissConfig,
                RoundRobinConfig = s.RoundRobinConfig,
            }).ToList(),
        };

        var result = await repo.AddAsync(tournament);
        if (result.IsFailure)
        {
            return AppError.InternalError;
        }

        return tournament;
    }
}
