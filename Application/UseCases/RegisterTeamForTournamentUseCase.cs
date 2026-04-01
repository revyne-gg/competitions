using competitions.Application.Ports;
using competitions.Domain.Competitions.Tournaments.Models;
using competitions.Domain.Models;
using competitions.Shared;

namespace competitions.Application.UseCases;

public sealed class RegisterTeamForTournamentUseCase(
    ITournamentRepository repo,
    ILogger<RegisterTeamForTournamentUseCase> logger
)
{
    public async Task<Result<TournamentTeam, AppError>> Execute(
        string tournamentId,
        string teamId,
        string password,
        string userId,
        string tenantId
    )
    {
        var tournamentResult = await repo.GetByIdAsync(tournamentId, tenantId);
        if (tournamentResult.IsFailure)
            return tournamentResult.Error == RepositoryError.NotFound ? AppError.NotFound : AppError.InternalError;

        var tournament = tournamentResult.Value!;

        switch (tournament.RegistrationType)
        {
            case RegistrationType.InviteOnly:
                logger.LogWarning("Team {TeamId} tried to register for invite-only tournament {TournamentId}", teamId, tournamentId);
                return AppError.Forbidden;

            case RegistrationType.Password:
                if (string.IsNullOrEmpty(password) || password != tournament.RegistrationPassword)
                {
                    logger.LogWarning("Team {TeamId} provided wrong password for tournament {TournamentId}", teamId, tournamentId);
                    return AppError.BadRequest;
                }
                break;

            case RegistrationType.Open:
            default:
                break;
        }

        var registration = new TournamentTeam
        {
            TournamentId = tournamentId,
            TeamId = teamId,
            TenantId = tenantId,
            CreatedAt = DateTime.UtcNow,
            Status = RegistrationStatus.Pending,
        };

        var result = await repo.AddRegistrationAsync(registration);
        if (result.IsFailure)
            return result.Error == RepositoryError.AlreadyExists ? AppError.Conflict : AppError.InternalError;

        return result.Value!;
    }
}
