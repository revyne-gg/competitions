using competitions.Domain.Competitions.Matches.Models;
using competitions.Shared;

namespace competitions.Application.Ports;

public interface IMatchRepository
{
    Task<Result<Match, RepositoryError>> GetByIdAsync(string id, string tenantId);
    Task<Result<Unit, RepositoryError>> Update(Match match);
}
