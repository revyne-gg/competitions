using competitions.Application;
using competitions.Application.Ports;
using competitions.Domain.Models;
using competitions.Infrastructure.Entities;
using competitions.Infrastructure.Services;
using competitions.Shared;
using Microsoft.EntityFrameworkCore;

namespace competitions.Infrastructure.Repositories;

public class LeagueRepository(DatabaseService db): ILeagueRepository
{
    public async Task<Result<League?, RepositoryError>> GetByIdAsync(string id, string tenantId)
    {
        try
        {
            var league = await db.Leagues.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);

            return league?.ToDomain();
        }
        catch (Exception e)
        {
            return RepositoryError.DatabaseError;
        }
    }

    public async Task<Result<League?, RepositoryError>> GetByNameAsync(string name, string realmId, string tenantId)
    {
        try
        {
            var league = await db.Leagues.FirstOrDefaultAsync(x => x.Name == name && x.RealmId == realmId && x.TenantId == tenantId);

            return league?.ToDomain();
        }
        catch (Exception e)
        {
            return RepositoryError.DatabaseError;
        }
    }

    public async Task<Result<League?, RepositoryError>> GetBySlugAsync(string organiserSlug, string realmSlug, string leagueSlug, string tenantId)
    {
        try
        {
            var league = await db.Leagues
                .FirstOrDefaultAsync(x => 
                    x.OrganiserSlug == organiserSlug && 
                    x.RealmSlug == realmSlug && 
                    x.Slug == leagueSlug &&
                    x.TenantId == tenantId
                );

            return league?.ToDomain();
        }
        catch (Exception e)
        {
            return RepositoryError.DatabaseError;
        }
    }

    public async Task<Result<Unit, RepositoryError>> AddAsync(League league)
    {
        try
        {
            var entity = new LeagueEntity
            {
                Id = league.Id,
                Name = league.Name,
                Description = league.Description,
                OrganiserId = league.OrganiserId,
                OrganiserSlug = league.OrganiserSlug,
                RealmId = league.RealmId,
                RealmSlug = league.RealmSlug,
                TenantId = league.TenantId,
                CreatedAt = league.CreatedAt,
                UpdatedAt = league.CreatedAt,
                DeletedAt = league.DeletedAt,
                DeletedBy = league.DeletedBy
            };

            await db.Leagues.AddAsync(entity);
            await db.SaveChangesAsync();

            return Unit.Value;
        } 
        catch (Exception e)
        {
            return RepositoryError.DatabaseError;
        }
    }

    public async Task<Result<int, RepositoryError>> Update(League league)
    {
        try
        {
            var entity = await db.Leagues.FirstOrDefaultAsync(x => x.Id == league.Id && x.TenantId == league.TenantId);
            if (entity is null)
            {
                return RepositoryError.DatabaseConcurrencyError;
            }

            entity.Name = league.Name;
            entity.Description = league.Description;
            entity.UpdatedAt = DateTime.UtcNow;

            return await db.SaveChangesAsync();
        }
        catch (Exception e)
        {
            return RepositoryError.DatabaseError;
        }
    }

    public async Task<Result<Unit, RepositoryError>> AddTeamToLeague(League league, LeagueTeam team)
    {
        try
        {
            var teamEntity = new LeagueTeamEntity
            {
                LeagueId = league.Id,
                TeamId = team.TeamId,
                CreatedAt = team.CreatedAt,
                TenantId = league.TenantId
            };
            
            db.LeagueTeams.Add(teamEntity);

            await db.SaveChangesAsync();
            
            return Unit.Value;
        }
        catch (Exception e)
        {
            return RepositoryError.DatabaseError;
        }
    }

    public async Task<Result<Unit, RepositoryError>> RemoveTeamFromLeague(League league, LeagueTeam team)
    {
        try
        {
            var teamEntity = await db.LeagueTeams.FirstOrDefaultAsync(x => x.LeagueId == league.Id && x.TeamId == team.TeamId && x.TenantId == league.TenantId);
            if (teamEntity is null)
            {
                return RepositoryError.DatabaseConcurrencyError;
            }
            
            db.LeagueTeams.Remove(teamEntity);

            await db.SaveChangesAsync();
            
            return Unit.Value;
        }
        catch (Exception e)
        {
            return RepositoryError.DatabaseError;
        }
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