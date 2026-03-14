using competitions.Application;
using competitions.Application.Ports;
using competitions.Domain.Models;
using competitions.Infrastructure.Entities;
using competitions.Infrastructure.Services;
using competitions.Shared;
using Microsoft.EntityFrameworkCore;

namespace competitions.Infrastructure.Repositories;

public class LeagueRepository(IDbContextFactory<DatabaseService> dbFactory) : ILeagueRepository
{
    public async Task<Result<League, RepositoryError>> GetByIdAsync(string id, string tenantId)
    {
        try
        {
            var db = await dbFactory.CreateDbContextAsync();
            var competition = await db.Competitions
                .FirstOrDefaultAsync(x => x.Id == id && x.Type == CompetitionType.League && x.TenantId == tenantId);

            if (competition is null)
            {
                return RepositoryError.NotFound;
            }

            var config = await db.LeagueConfigs
                .FirstOrDefaultAsync(x => x.CompetitionId == id && x.TenantId == tenantId);

            return competition.ToLeagueDomain();
        }
        catch (Exception e)
        {
            return RepositoryError.DatabaseError;
        }
    }

    public async Task<Result<League, RepositoryError>> GetByNameAndDiscriminatorAsync(
        string name,
        string discriminator,
        string tenantId
    )
    {
        try
        {
            var db = await dbFactory.CreateDbContextAsync();

            var competition = await db.Competitions
                .FirstOrDefaultAsync(x =>
                    x.Name == name &&
                    x.Discriminator == discriminator &&
                    x.Type == CompetitionType.League &&
                    x.TenantId == tenantId
                );

            if (competition is null)
            {
                return RepositoryError.NotFound;
            }

            var config = await db.LeagueConfigs
                .FirstOrDefaultAsync(x => x.CompetitionId == competition.Id && x.TenantId == tenantId);

            return competition.ToLeagueDomain();
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
            var db = await dbFactory.CreateDbContextAsync();

            var entity = new CompetitionEntity
            {
                Id = league.Id,
                Name = league.Name,
                Discriminator = league.Discriminator,
                Type = CompetitionType.League,
                CreatedAt = league.CreatedAt,
                TenantId = league.TenantId,
            };

            var configEntity = new LeagueConfigEntity
            {
                CompetitionId = league.Id,
                Description = league.Description,
                OrganiserId = league.OrganiserId,
                RealmId = league.RealmId,
                State = league.State,
                Legs = league.Legs,
                IsRegistrationOpen = league.IsRegistrationOpen,
                RegistrationPeriodStart = league.RegistrationPeriodStart,
                RegistrationPeriodEnd = league.RegistrationPeriodEnd,
                LeaguePeriodStart = league.LeaguePeriodStart,
                LeaguePeriodEnd = league.LeaguePeriodEnd,
                DeletedAt = league.DeletedAt,
                DeletedBy = league.DeletedBy,
                TenantId = league.TenantId,
            };

            await db.Competitions.AddAsync(entity);
            await db.LeagueConfigs.AddAsync(configEntity);
            await db.SaveChangesAsync();

            return Unit.Value;
        }
        catch (Exception e)
        {
            return RepositoryError.DatabaseError;
        }
    }

    public async Task<Result<Unit, RepositoryError>> Update(League league)
    {
        try
        {
            var db = await dbFactory.CreateDbContextAsync();

            var entity = await db.Competitions
                .FirstOrDefaultAsync(x => x.Id == league.Id && x.TenantId == league.TenantId);

            if (entity is null) return RepositoryError.DatabaseConcurrencyError;

            entity.Name = league.Name;

            var config = await db.LeagueConfigs
                .FirstOrDefaultAsync(x => x.CompetitionId == league.Id && x.TenantId == league.TenantId);

            if (config is not null)
            {
                config.Description = league.Description;
                config.OrganiserId = league.OrganiserId;
                config.RealmId = league.RealmId;
                config.State = league.State;
                config.Legs = league.Legs;
                config.IsRegistrationOpen = league.IsRegistrationOpen;
                config.RegistrationPeriodStart = league.RegistrationPeriodStart;
                config.RegistrationPeriodEnd = league.RegistrationPeriodEnd;
                config.LeaguePeriodStart = league.LeaguePeriodStart;
                config.LeaguePeriodEnd = league.LeaguePeriodEnd;
                config.DeletedAt = league.DeletedAt;
                config.DeletedBy = league.DeletedBy;
            }

            await db.SaveChangesAsync();

            return Unit.Value;
        }
        catch (Exception e)
        {
            return RepositoryError.DatabaseError;
        }
    }

    // ── Divisions ──────────────────────────────────────────────────────────────

    public async Task<Result<List<Division>, RepositoryError>> GetDivisionsByLeagueAsync(string leagueId, string tenantId)
    {
        try
        {
            var db = await dbFactory.CreateDbContextAsync();
            var entities = await db.Divisions
                .Where(x => x.CompetitionId == leagueId && x.TenantId == tenantId)
                .OrderBy(x => x.Order)
                .ToListAsync();
            return entities.Select(e => e.ToDomain()).ToList();
        }
        catch { return RepositoryError.DatabaseError; }
    }

    public async Task<Result<Division, RepositoryError>> GetDivisionByIdAsync(string divisionId, string tenantId)
    {
        try
        {
            var db = await dbFactory.CreateDbContextAsync();
            var entity = await db.Divisions
                .FirstOrDefaultAsync(x => x.Id == divisionId && x.TenantId == tenantId);
            if (entity is null) return RepositoryError.NotFound;
            return entity.ToDomain();
        }
        catch { return RepositoryError.DatabaseError; }
    }

    public async Task<Result<Unit, RepositoryError>> AddDivisionAsync(Division division)
    {
        try
        {
            var db = await dbFactory.CreateDbContextAsync();
            await db.Divisions.AddAsync(new DivisionEntity
            {
                Id = division.Id,
                CompetitionId = division.LeagueId,
                Name = division.Name,
                Slug = division.Slug,
                Order = division.Order,
                BestOf = division.BestOf,
                MaxTeamsPerGroup = division.MaxTeamsPerGroup,
                TenantId = division.TenantId,
                CreatedAt = division.CreatedAt,
            });
            await db.SaveChangesAsync();
            return Unit.Value;
        }
        catch { return RepositoryError.DatabaseError; }
    }

    public async Task<Result<Unit, RepositoryError>> DeleteDivisionAsync(string divisionId, string tenantId)
    {
        try
        {
            var db = await dbFactory.CreateDbContextAsync();
            var entity = await db.Divisions.FirstOrDefaultAsync(x => x.Id == divisionId && x.TenantId == tenantId);
            if (entity is null) return RepositoryError.NotFound;
            db.Divisions.Remove(entity);
            await db.SaveChangesAsync();
            return Unit.Value;
        }
        catch { return RepositoryError.DatabaseError; }
    }

    // ── Division Groups ────────────────────────────────────────────────────────

    public async Task<Result<List<DivisionGroup>, RepositoryError>> GetDivisionGroupsByDivisionAsync(string divisionId, string tenantId)
    {
        try
        {
            var db = await dbFactory.CreateDbContextAsync();
            var entities = await db.DivisionGroups
                .Include(x => x.Teams)
                .Where(x => x.DivisionId == divisionId && x.TenantId == tenantId)
                .OrderBy(x => x.Order)
                .ToListAsync();
            return entities.Select(e => e.ToDomain()).ToList();
        }
        catch { return RepositoryError.DatabaseError; }
    }

    public async Task<Result<DivisionGroup, RepositoryError>> GetDivisionGroupByIdAsync(string groupId, string tenantId)
    {
        try
        {
            var db = await dbFactory.CreateDbContextAsync();
            var entity = await db.DivisionGroups
                .Include(x => x.Teams)
                .FirstOrDefaultAsync(x => x.Id == groupId && x.TenantId == tenantId);
            if (entity is null) return RepositoryError.NotFound;
            return entity.ToDomain();
        }
        catch { return RepositoryError.DatabaseError; }
    }

    public async Task<Result<Unit, RepositoryError>> AddDivisionGroupAsync(DivisionGroup group)
    {
        try
        {
            var db = await dbFactory.CreateDbContextAsync();
            await db.DivisionGroups.AddAsync(new DivisionGroupEntity
            {
                Id = group.Id,
                DivisionId = group.DivisionId,
                Name = group.Name,
                Slug = group.Slug,
                Order = group.Order,
                TenantId = group.TenantId,
                CreatedAt = group.CreatedAt,
            });
            await db.SaveChangesAsync();
            return Unit.Value;
        }
        catch { return RepositoryError.DatabaseError; }
    }

    public async Task<Result<Unit, RepositoryError>> DeleteDivisionGroupAsync(string groupId, string tenantId)
    {
        try
        {
            var db = await dbFactory.CreateDbContextAsync();
            var entity = await db.DivisionGroups.FirstOrDefaultAsync(x => x.Id == groupId && x.TenantId == tenantId);
            if (entity is null) return RepositoryError.NotFound;
            db.DivisionGroups.Remove(entity);
            await db.SaveChangesAsync();
            return Unit.Value;
        }
        catch { return RepositoryError.DatabaseError; }
    }

    public async Task<Result<DivisionGroup, RepositoryError>> AddTeamToDivisionGroupAsync(string groupId, string teamId)
    {
        try
        {
            var db = await dbFactory.CreateDbContextAsync();
            var group = await db.DivisionGroups
                .Include(x => x.Teams)
                .FirstOrDefaultAsync(x => x.Id == groupId);
            if (group is null) return RepositoryError.NotFound;

            if (!group.Teams.Any(t => t.TeamId == teamId))
            {
                group.Teams.Add(new DivisionGroupTeamEntity { GroupId = groupId, TeamId = teamId });
                await db.SaveChangesAsync();
            }

            return group.ToDomain();
        }
        catch { return RepositoryError.DatabaseError; }
    }

    // ── Registrations ──────────────────────────────────────────────────────────

    public async Task<Result<CompetitionTeam?, RepositoryError>> GetCompetitionTeamAsync(string leagueId, string teamId, string tenantId)
    {
        try
        {
            var db = await dbFactory.CreateDbContextAsync();
            var entity = await db.LeagueTeams
                .FirstOrDefaultAsync(x => x.LeagueId == leagueId && x.TeamId == teamId && x.TenantId == tenantId);
            return entity?.ToDomain();
        }
        catch { return RepositoryError.DatabaseError; }
    }

    public async Task<Result<Unit, RepositoryError>> RegisterTeamAsync(CompetitionTeam team)
    {
        try
        {
            var db = await dbFactory.CreateDbContextAsync();
            await db.LeagueTeams.AddAsync(new LeagueTeamEntity
            {
                LeagueId = team.LeagueId,
                TeamId = team.TeamId,
                TenantId = team.TenantId,
                CreatedAt = team.CreatedAt,
                Status = team.Status,
            });
            await db.SaveChangesAsync();
            return Unit.Value;
        }
        catch { return RepositoryError.DatabaseError; }
    }

    public async Task<Result<Unit, RepositoryError>> UnregisterTeamAsync(string leagueId, string teamId, string tenantId)
    {
        try
        {
            var db = await dbFactory.CreateDbContextAsync();
            var entity = await db.LeagueTeams
                .FirstOrDefaultAsync(x => x.LeagueId == leagueId && x.TeamId == teamId && x.TenantId == tenantId);
            if (entity is null) return RepositoryError.NotFound;
            db.LeagueTeams.Remove(entity);
            await db.SaveChangesAsync();
            return Unit.Value;
        }
        catch { return RepositoryError.DatabaseError; }
    }

    public async Task<Result<List<CompetitionTeam>, RepositoryError>> GetRegistrationsAsync(string leagueId, string tenantId)
    {
        try
        {
            var db = await dbFactory.CreateDbContextAsync();
            var entities = await db.LeagueTeams
                .Where(x => x.LeagueId == leagueId && x.TenantId == tenantId)
                .ToListAsync();
            return entities.Select(e => e.ToDomain()).ToList();
        }
        catch { return RepositoryError.DatabaseError; }
    }

    public async Task<Result<CompetitionTeam, RepositoryError>> GetRegistrationAsync(string leagueId, string teamId, string tenantId)
    {
        try
        {
            var db = await dbFactory.CreateDbContextAsync();
            var entity = await db.LeagueTeams
                .FirstOrDefaultAsync(x => x.LeagueId == leagueId && x.TeamId == teamId && x.TenantId == tenantId);
            if (entity is null) return RepositoryError.NotFound;
            return entity.ToDomain();
        }
        catch { return RepositoryError.DatabaseError; }
    }
}
