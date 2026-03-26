using competitions.Domain.Competitions.Tournaments.Models;
using competitions.Domain.Models;
using competitions.Shared;

namespace competitions.Application.Ports;

public interface ITournamentRepository
{
    Task<Result<Tournament, RepositoryError>> GetByIdAsync(string id, string tenantId);
    Task<Result<List<Tournament>, RepositoryError>> GetByRealmIdAsync(string realmId, string tenantId);
    Task<Result<Tournament, RepositoryError>> GetByNameAndDiscriminatorAsync(string name, string discriminator, string tenantId);
    Task<Result<Unit, RepositoryError>> AddAsync(Tournament tournament);
    Task<Result<Unit, RepositoryError>> Update(Tournament tournament);
    Task<Result<(List<TournamentTeam> Items, int TotalCount), RepositoryError>> GetRegistrationsAsync(string tournamentId, string tenantId, int page, int pageSize);
}