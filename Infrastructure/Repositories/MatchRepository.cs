using competitions.Application;
using competitions.Application.Ports;
using competitions.Domain.Competitions.Matches.Models;
using competitions.Domain.Models;
using competitions.Infrastructure.Entities;
using competitions.Infrastructure.Services;
using competitions.Shared;
using Microsoft.EntityFrameworkCore;

namespace competitions.Infrastructure.Repositories;

public class MatchRepository(IDbContextFactory<DatabaseService> dbFactory) : IMatchRepository
{
    public async Task<Result<Match, RepositoryError>> GetByIdAsync(string id, string tenantId)
    {
        try
        {
            var db = await dbFactory.CreateDbContextAsync();

            var entity = await db.Matches
                .Include(m => m.Competition)
                .FirstOrDefaultAsync(m => m.Id == id && m.TenantId == tenantId);

            if (entity is null)
            {
                return RepositoryError.NotFound;
            }

            return entity.ToMatchDomain();
        }
        catch (Exception)
        {
            return RepositoryError.DatabaseError;
        }
    }

    public async Task<Result<List<Match>, RepositoryError>> GetByCompetitionIdAsync(string competitionId, string tenantId)
    {
        try
        {
            var db = await dbFactory.CreateDbContextAsync();
            var entities = await db.Matches
                .Where(m => m.CompetitionId == competitionId && m.TenantId == tenantId)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();
            return entities.Select(e => e.ToMatchDomain()).ToList();
        }
        catch (Exception)
        {
            return RepositoryError.DatabaseError;
        }
    }

    public async Task<Result<Unit, RepositoryError>> AddAsync(Match match)
    {
        try
        {
            var db = await dbFactory.CreateDbContextAsync();
            await db.Matches.AddAsync(new MatchEntity
            {
                Id = match.Id,
                CompetitionId = match.CompetitionId,
                HomeTeamId = match.HomeTeamId,
                AwayTeamId = match.AwayTeamId,
                Round = match.Round,
                MatchDate = match.MatchDate,
                CreatedAt = match.CreatedAt,
                TenantId = match.TenantId,
            });
            await db.SaveChangesAsync();
            return Unit.Value;
        }
        catch (Exception)
        {
            return RepositoryError.DatabaseError;
        }
    }

    public async Task<Result<Unit, RepositoryError>> AddRangeAsync(List<Match> matches)
    {
        try
        {
            var db = await dbFactory.CreateDbContextAsync();
            var entities = matches.Select(m => new MatchEntity
            {
                Id = m.Id,
                CompetitionId = m.CompetitionId,
                HomeTeamId = m.HomeTeamId,
                AwayTeamId = m.AwayTeamId,
                Round = m.Round,
                MatchDate = m.MatchDate,
                CreatedAt = m.CreatedAt,
                TenantId = m.TenantId,
            }).ToList();
            await db.Matches.AddRangeAsync(entities);
            await db.SaveChangesAsync();
            return Unit.Value;
        }
        catch (DbUpdateConcurrencyException)
        {
            return RepositoryError.DatabaseConcurrencyError;
        }
        catch (Exception)
        {
            return RepositoryError.DatabaseError;
        }
    }

    public async Task<Result<Unit, RepositoryError>> Update(Match match)
    {
        try
        {
            var db = await dbFactory.CreateDbContextAsync();

            var entity = await db.Matches
                .FirstOrDefaultAsync(m => m.Id == match.Id && m.TenantId == match.TenantId);

            if (entity is null)
            {
                return RepositoryError.NotFound;
            }

            entity.ScoreHome = match.ScoreHome;
            entity.ScoreAway = match.ScoreAway;
            entity.WinnerTeamId = match.WinnerTeamId;
            entity.LoserTeamId = match.LoserTeamId;

            await db.SaveChangesAsync();
            return Unit.Value;
        }
        catch (DbUpdateConcurrencyException)
        {
            return RepositoryError.DatabaseConcurrencyError;
        }
        catch (Exception)
        {
            return RepositoryError.DatabaseError;
        }
    }
}
