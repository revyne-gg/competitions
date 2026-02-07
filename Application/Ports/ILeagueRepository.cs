using competitions.Domain.Models;
using competitions.Shared;

namespace competitions.Application.Ports;

public interface ILeagueRepository
{
    Task<Result<League?, RepositoryError>> GetByIdAsync(string id, string tenantId);
    Task<Result<League?, RepositoryError>> GetByNameAsync(string name, string realmId, string tenantId);
    Task<Result<League?, RepositoryError>> GetBySlugAsync(string organiserSlug, string realmSlug, string leagueSlug, string tenantId);
    Task<Result<Unit, RepositoryError>> AddAsync(League league);
    Task<Result<int, RepositoryError>> Update(League league);
    Task<Result<Unit, RepositoryError>> AddTeamToLeague(League league, LeagueTeam team);
    Task<Result<Unit, RepositoryError>> RemoveTeamFromLeague(League league, LeagueTeam team);
    Task<Result<Unit, RepositoryError>> Remove(League league);
    Task<Result<List<League>, RepositoryError>> GetLeagues(int page, int pageSize, string realmId, string tenantId);
    Task<Result<int, RepositoryError>> GetLeaguesCount(string realmId, string tenantId);
    Task<Result<List<LeagueTeam>, RepositoryError>> GetLeagueMembershipsForTeamIds(int page, int pageSize, List<string> teamIds, string tenantId);
    Task<Result<int, RepositoryError>> GetLeagueMembershipsForTeamIdsCount(int page, int pageSize, List<string> teamIds, string tenantId);
}