using competitions.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace competitions.Infrastructure.Services;

public sealed class DatabaseService : DbContext
{
    public DbSet<CompetitionEntity> Competitions => Set<CompetitionEntity>();
    public DbSet<TournamentConfigEntity> TournamentConfigs => Set<TournamentConfigEntity>();
    public DbSet<LeagueConfigEntity> LeagueConfigs => Set<LeagueConfigEntity>();
    public DbSet<DivisionEntity> Divisions => Set<DivisionEntity>();
    public DbSet<DivisionGroupEntity> DivisionGroups => Set<DivisionGroupEntity>();
    public DbSet<DivisionGroupStandingsEntity> Standings => Set<DivisionGroupStandingsEntity>();
    public DbSet<DivisionGroupStandingsEntryEntity> StandingsEntities => Set<DivisionGroupStandingsEntryEntity>();
    public DbSet<LeagueTeamEntity> LeagueTeams => Set<LeagueTeamEntity>();
    public DbSet<DivisionGroupTeamEntity> DivisionGroupTeams => Set<DivisionGroupTeamEntity>();
    public DbSet<RosterEntity> Rosters => Set<RosterEntity>();
    public DbSet<MatchEntity> Matches => Set<MatchEntity>();
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("Host=127.0.0.1;Port=5432;Database=competitions;Username=brackzr;Password=secret");
    }
}