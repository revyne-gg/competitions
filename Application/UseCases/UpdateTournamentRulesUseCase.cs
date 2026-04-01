using competitions.Application.Ports;
using competitions.Domain.Competitions.Leagues.Models;
using competitions.Domain.Competitions.Shared.Models;
using competitions.Domain.Competitions.Tournaments.Models;
using competitions.Shared;

namespace competitions.Application.UseCases;

public sealed class UpdateTournamentRulesUseCase(
    ITournamentRepository repo,
    IPermissionService permissionService
)
{
    public async Task<Result<TournamentRules, AppError>> Execute(
        string tournamentId,
        string content,
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

        var rules = new TournamentRules
        {
            TournamentId = tournamentId,
            Content = content,
            TenantId = tenantId,
        };

        var result = await repo.UpsertRulesAsync(rules);
        if (result.IsFailure) return AppError.InternalError;

        return result.Value!;
    }
}
