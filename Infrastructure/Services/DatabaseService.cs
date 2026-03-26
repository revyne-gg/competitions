using competitions.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace competitions.Infrastructure.Services;

public sealed class DatabaseService : DbContext
{
    public DatabaseService(DbContextOptions<DatabaseService> options) : base(options) { }

    public DbSet<CompetitionEntity> Competitions => Set<CompetitionEntity>();
    public DbSet<TournamentConfigEntity> TournamentConfigs => Set<TournamentConfigEntity>();
    public DbSet<TournamentStageEntity> TournamentStages => Set<TournamentStageEntity>();
    public DbSet<LeagueConfigEntity> LeagueConfigs => Set<LeagueConfigEntity>();
    public DbSet<DivisionEntity> Divisions => Set<DivisionEntity>();
    public DbSet<DivisionGroupEntity> DivisionGroups => Set<DivisionGroupEntity>();
    public DbSet<DivisionGroupStandingsEntity> Standings => Set<DivisionGroupStandingsEntity>();
    public DbSet<DivisionGroupStandingsEntryEntity> StandingsEntities => Set<DivisionGroupStandingsEntryEntity>();
    public DbSet<LeagueTeamEntity> LeagueTeams => Set<LeagueTeamEntity>();
    public DbSet<TournamentTeamEntity> TournamentTeams => Set<TournamentTeamEntity>();
    public DbSet<DivisionGroupTeamEntity> DivisionGroupTeams => Set<DivisionGroupTeamEntity>();
    public DbSet<RosterEntity> Rosters => Set<RosterEntity>();
    public DbSet<MatchEntity> Matches => Set<MatchEntity>();
}