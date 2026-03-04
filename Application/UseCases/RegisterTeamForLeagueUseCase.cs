using competitions.Application.Ports;
using competitions.Domain.Competitions.Leagues.Models;
using competitions.Domain.Models;
using competitions.Shared;

namespace competitions.Application.UseCases;

public sealed class RegisterTeamForLeagueUseCase(
    ILeagueRepository repo,
    IPermissionService permissionService
)
{
    public async Task<Result<CompetitionTeam, AppError>> Execute(
        string leagueId,
        string teamId,
        string userId,
        string tenantId
    )
    {
        // User must be captain or owner of the team to register it
        var teamRoleResult = await permissionService.GetRoleForUserInTeam(userId, teamId);
        if (teamRoleResult.IsFailure) return AppError.InternalError;
        if (teamRoleResult.Value is not (TeamMemberRole.Captain or TeamMemberRole.Member))
            return AppError.Forbidden;

        var leagueResult = await repo.GetByIdAsync(leagueId, tenantId);
        if (leagueResult.IsFailure)
            return leagueResult.Error == RepositoryError.NotFound ? AppError.NotFound : AppError.InternalError;

        var league = leagueResult.Value!;
        if (!league.IsRegistrationOpen) return AppError.BadRequest;

        // Check if already registered
        var existingResult = await repo.GetCompetitionTeamAsync(leagueId, teamId, tenantId);
        if (existingResult.IsFailure) return AppError.InternalError;
        if (existingResult.Value is not null) return existingResult.Value;

        var registration = new CompetitionTeam
        {
            LeagueId = leagueId,
            TeamId = teamId,
            TenantId = tenantId,
            CreatedAt = DateTime.UtcNow,
            Status = RegistrationStatus.Pending,
        };

        var addResult = await repo.RegisterTeamAsync(registration);
        if (addResult.IsFailure) return AppError.InternalError;

        return registration;
    }
}
