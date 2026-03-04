using competitions.Domain.Models;
using competitions.Shared;

namespace competitions.Application.Ports;

public interface ILeagueRepository
{
    // Leagues
    Task<Result<League, RepositoryError>> GetByIdAsync(string id, string tenantId);
    Task<Result<League, RepositoryError>> GetByNameAndDiscriminatorAsync(string name, string discriminator, string tenantId);
    Task<Result<Unit, RepositoryError>> AddAsync(League league);
    Task<Result<Unit, RepositoryError>> Update(League league);

    // Divisions
    Task<Result<List<Division>, RepositoryError>> GetDivisionsByLeagueAsync(string leagueId, string tenantId);
    Task<Result<Division, RepositoryError>> GetDivisionByIdAsync(string divisionId, string tenantId);
    Task<Result<Unit, RepositoryError>> AddDivisionAsync(Division division);
    Task<Result<Unit, RepositoryError>> DeleteDivisionAsync(string divisionId, string tenantId);

    // Division Groups
    Task<Result<List<DivisionGroup>, RepositoryError>> GetDivisionGroupsByDivisionAsync(string divisionId, string tenantId);
    Task<Result<DivisionGroup, RepositoryError>> GetDivisionGroupByIdAsync(string groupId, string tenantId);
    Task<Result<Unit, RepositoryError>> AddDivisionGroupAsync(DivisionGroup group);
    Task<Result<Unit, RepositoryError>> DeleteDivisionGroupAsync(string groupId, string tenantId);
    Task<Result<DivisionGroup, RepositoryError>> AddTeamToDivisionGroupAsync(string groupId, string teamId);

    // Registrations
    Task<Result<CompetitionTeam?, RepositoryError>> GetCompetitionTeamAsync(string leagueId, string teamId, string tenantId);
    Task<Result<Unit, RepositoryError>> RegisterTeamAsync(CompetitionTeam team);
    Task<Result<Unit, RepositoryError>> UnregisterTeamAsync(string leagueId, string teamId, string tenantId);
    Task<Result<List<CompetitionTeam>, RepositoryError>> GetRegistrationsAsync(string leagueId, string tenantId);
    Task<Result<CompetitionTeam, RepositoryError>> GetRegistrationAsync(string leagueId, string teamId, string tenantId);
}