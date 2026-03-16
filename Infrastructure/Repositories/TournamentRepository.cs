using competitions.Application;
using competitions.Application.Ports;
using competitions.Domain.Competitions.Tournaments.Models;
using competitions.Domain.Models;
using competitions.Infrastructure.Entities;
using competitions.Infrastructure.Services;
using competitions.Shared;
using Microsoft.EntityFrameworkCore;

namespace competitions.Infrastructure.Repositories;

public class TournamentRepository(IDbContextFactory<DatabaseService> dbFactory) : ITournamentRepository
{
    public async Task<Result<Tournament, RepositoryError>> GetByIdAsync(string id, string tenantId)
    {
        try
        {
            var db = await dbFactory.CreateDbContextAsync();

            var competition = await db.Competitions
                .FirstOrDefaultAsync(x => x.Id == id && x.Type == CompetitionType.Tournament && x.TenantId == tenantId);

            if (competition is null)
            {
                return RepositoryError.NotFound;
            }

            var config = await db.TournamentConfigs
                .FirstOrDefaultAsync(x => x.CompetitionId == id && x.TenantId == tenantId);

            return competition.ToTournamentDomain(config);
        }
        catch (Exception e)
        {
            return RepositoryError.DatabaseError;
        }
    }

    public async Task<Result<Tournament?, RepositoryError>> GetByNameAndDiscriminatorAsync(
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
                    x.Type == CompetitionType.Tournament &&
                    x.TenantId == tenantId
                );

            if (competition is null)
            {
                return RepositoryError.NotFound;
            }

            var config = await db.TournamentConfigs
                .FirstOrDefaultAsync(x => x.CompetitionId == competition.Id && x.TenantId == tenantId);

            return competition.ToTournamentDomain(config);
        }
        catch (Exception e)
        {
            return RepositoryError.DatabaseError;
        }
    }

    public async Task<Result<Unit, RepositoryError>> AddAsync(Tournament tournament)
    {
        try
        {
            var db = await dbFactory.CreateDbContextAsync();

            var entity = new CompetitionEntity
            {
                Id = tournament.Id,
                Name = tournament.Name,
                Discriminator = tournament.Discriminator,
                Type = CompetitionType.Tournament,
                CreatedAt = tournament.CreatedAt,
                TenantId = tournament.TenantId,
                Game = tournament.Game,
                BestOf = tournament.BestOf,
                MapPool = tournament.MapPool,
            };

            var configEntity = new TournamentConfigEntity
            {
                CompetitionId = tournament.Id,
                Format = tournament.Format,
                SeedingType = tournament.SeedingType,
                BracketReset = tournament.BracketReset,
                TenantId = tournament.TenantId,
            };

            await db.Competitions.AddAsync(entity);
            await db.TournamentConfigs.AddAsync(configEntity);
            await db.SaveChangesAsync();

            return Unit.Value;
        }
        catch (Exception e)
        {
            return RepositoryError.DatabaseError;
        }
    }

    public async Task<Result<Unit, RepositoryError>> Update(Tournament tournament)
    {
        try
        {
            var db = await dbFactory.CreateDbContextAsync();

            var entity = await db.Competitions
                .FirstOrDefaultAsync(x => x.Id == tournament.Id && x.TenantId == tournament.TenantId);

            if (entity is null) return RepositoryError.DatabaseConcurrencyError;

            entity.Name = tournament.Name;
            entity.Discriminator = tournament.Discriminator;
            entity.Game = tournament.Game;
            entity.BestOf = tournament.BestOf;
            entity.MapPool = tournament.MapPool;

            var config = await db.TournamentConfigs
                .FirstOrDefaultAsync(x => x.CompetitionId == tournament.Id && x.TenantId == tournament.TenantId);

            if (config is not null)
            {
                config.Format = tournament.Format;
                config.SeedingType = tournament.SeedingType;
                config.BracketReset = tournament.BracketReset;
            }

            await db.SaveChangesAsync();

            return Unit.Value;
        }
        catch (Exception e)
        {
            return RepositoryError.DatabaseError;
        }
    }
}