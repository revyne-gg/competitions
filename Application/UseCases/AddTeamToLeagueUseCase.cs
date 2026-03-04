using competitions.Application.Ports;
using competitions.Domain.Competitions.Leagues.Models;
using competitions.Domain.Models;
using competitions.Shared;

namespace competitions.Application.UseCases;

public sealed class AddTeamToLeagueUseCase(
    ILeagueRepository repo,
    IPermissionService permissionService
)
{
    /// <summary>
    /// Adds a team to a division group. Checks that the requesting user is a captain/owner of the team.
    /// Reuses an existing CompetitionTeam (registration) for team+league if one exists.
    /// </summary>
    public async Task<Result<DivisionGroup, AppError>> Execute(
        string groupId,
        string teamId,
        string userId,
        string organiserId,
        string tenantId
    )
    {
        var roleResult = await permissionService.GetRoleForUserInOrganiser(userId, organiserId);
        if (roleResult.IsFailure) return AppError.InternalError;
        if (roleResult.Value is not (OrganiserMemberRole.Owner or OrganiserMemberRole.Manager))
            return AppError.Forbidden;

        var groupResult = await repo.GetDivisionGroupByIdAsync(groupId, tenantId);
        if (groupResult.IsFailure)
            return groupResult.Error == RepositoryError.NotFound ? AppError.NotFound : AppError.InternalError;

        var group = groupResult.Value!;

        // Ensure a competition team record exists for this team + league (create if absent)
        var existingTeam = await repo.GetCompetitionTeamAsync(group.LeagueId, teamId, tenantId);
        if (existingTeam.IsFailure) return AppError.InternalError;

        if (existingTeam.Value is null)
        {
            var registerResult = await repo.RegisterTeamAsync(new CompetitionTeam
            {
                LeagueId = group.LeagueId,
                TeamId = teamId,
                TenantId = tenantId,
                CreatedAt = DateTime.UtcNow,
                Status = RegistrationStatus.Approved,
            });
            if (registerResult.IsFailure) return AppError.InternalError;

            await permissionService.AddTeamToLeague(teamId, group.LeagueId);
        }

        var addResult = await repo.AddTeamToDivisionGroupAsync(groupId, teamId);
        if (addResult.IsFailure)
            return addResult.Error == RepositoryError.NotFound ? AppError.NotFound : AppError.InternalError;

        return addResult.Value!;
    }
}
