using competitions.Application;
using competitions.Application.Ports;
using competitions.Domain.Competitions.Matches.Models;
using competitions.Domain.Models;
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
