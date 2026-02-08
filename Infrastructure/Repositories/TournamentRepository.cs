using competitions.Application;
using competitions.Application.Ports;
using competitions.Domain.Competitions.Tournaments.Models;
using competitions.Domain.Models;
using competitions.Infrastructure.Entities;
using competitions.Infrastructure.Services;
using competitions.Shared;
using Microsoft.EntityFrameworkCore;

namespace competitions.Infrastructure.Repositories;

public class TournamentRepository(DatabaseService db) : ITournamentRepository
{
    public async Task<Result<Tournament, RepositoryError>> GetByIdAsync(string id, string tenantId)
    {
        try
        {
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
            var entity = new CompetitionEntity
            {
                Id = tournament.Id,
                Name = tournament.Name,
                Discriminator = tournament.Discriminator,
                Type = CompetitionType.Tournament,
                CreatedAt = tournament.CreatedAt,
                TenantId = tournament.TenantId,
            };

            var configEntity = new TournamentConfigEntity
            {
                CompetitionId = tournament.Id,
                Format = tournament.Format,
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
            var entity = await db.Competitions
                .FirstOrDefaultAsync(x => x.Id == tournament.Id && x.TenantId == tournament.TenantId);

            if (entity is null) return RepositoryError.DatabaseConcurrencyError;

            entity.Name = tournament.Name;
            entity.Discriminator = tournament.Discriminator;

            var config = await db.TournamentConfigs
                .FirstOrDefaultAsync(x => x.CompetitionId == tournament.Id && x.TenantId == tournament.TenantId);

            if (config is not null)
            {
                config.Format = tournament.Format;
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