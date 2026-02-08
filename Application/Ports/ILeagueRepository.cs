using competitions.Domain.Models;
using competitions.Shared;

namespace competitions.Application.Ports;

public interface ILeagueRepository
{
    Task<Result<League, RepositoryError>> GetByIdAsync(string id, string tenantId);
    Task<Result<League, RepositoryError>> GetByNameAndDiscriminatorAsync(string name, string discriminator, string tenantId);
    Task<Result<Unit, RepositoryError>> AddAsync(League league);
    Task<Result<Unit, RepositoryError>> Update(League league);
}