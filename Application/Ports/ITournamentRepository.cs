using competitions.Domain.Competitions.Tournaments.Models;
using competitions.Shared;

namespace competitions.Application.Ports;

public interface ITournamentRepository
{
    Task<Result<Tournament, RepositoryError>> GetByIdAsync(string id, string tenantId);
    Task<Result<Tournament, RepositoryError>> GetByNameAndDiscriminatorAsync(string name, string discriminator, string tenantId);
    Task<Result<Unit, RepositoryError>> AddAsync(Tournament tournament);
    Task<Result<Unit, RepositoryError>> Update(Tournament tournament);
}