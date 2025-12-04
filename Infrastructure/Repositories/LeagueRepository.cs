using leagues.Application;
using leagues.Application.Ports;
using leagues.Domain.Models;
using leagues.Shared;

namespace leagues.Infrastructure.Repositories;

public class LeagueRepository: ILeagueRepository
{
    public async Task<Result<League?, RepositoryError>> GetByIdAsync(string id, string tenantId)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<League?, RepositoryError>> GetByNameAsync(string name, string realmId, string tenantId)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<League?, RepositoryError>> GetBySlugAsync(string organiserSlug, string realmSlug, string tenantId)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<Unit, RepositoryError>> AddAsync(League league)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<int, RepositoryError>> Update(League league)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<Unit, RepositoryError>> Remove(League league)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<List<League>, RepositoryError>> GetLeagues(int page, int pageSize, string realmId, string tenantId)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<int, RepositoryError>> GetLeaguesCount(string realmId, string tenantId)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<List<LeagueTeam>, RepositoryError>> GetLeagueMembershipsForTeamIds(int page, int pageSize,
        List<string> teamIds, string tenantId)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<int, RepositoryError>> GetLeagueMembershipsForTeamIdsCount(int page, int pageSize,
        List<string> teamIds, string tenantId)
    {
        throw new NotImplementedException();
    }
}