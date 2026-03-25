using System.Text.Json;
using competitions.Domain.Competitions.Leagues.Models;
using competitions.Domain.Competitions.Shared.Models;
using competitions.Domain.Competitions.Tournaments.Models;
using competitions.Domain.Models;
using competitions.Infrastructure.Entities;

namespace competitions.Infrastructure;

internal static class Mapper
{
    internal static TournamentConfig ToDomain(this TournamentConfigEntity entity)
    {
        return new TournamentConfig
        {
            Format = entity.Format,
            SeedingType = entity.SeedingType,
            BracketReset = entity.BracketReset,
        };
    }

    internal static LeagueConfig ToDomain(this LeagueConfigEntity entity)
    {
        return new LeagueConfig();
    }

    extension (CompetitionEntity entity)
    {
        internal Tournament ToTournamentDomain(TournamentConfigEntity? config = null, List<TournamentStageEntity>? stageEntities = null)
        {
            return new Tournament
            {
                Id = entity.Id,
                Name = entity.Name,
                Discriminator = entity.Discriminator,
                CreatedAt = entity.CreatedAt,
                TenantId = entity.TenantId,
                Game = entity.Game,
                BestOf = entity.BestOf,
                MapPool = entity.MapPool,
                Format = config?.Format ?? default,
                SeedingType = config?.SeedingType ?? default,
                BracketReset = config?.BracketReset ?? false,
                MaxParticipants = config?.MaxParticipants ?? 0,
                OrganiserId = config?.OrganiserId,
                RealmId = config?.RealmId,
                Stages = stageEntities?.OrderBy(s => s.Order).Select(s => s.ToDomain()).ToList() ?? new(),
            };
        }

        internal League ToLeagueDomain(LeagueConfigEntity? config = null)
        {
            var league = new League
            {
                Id = entity.Id,
                Name = entity.Name,
                Discriminator = entity.Discriminator,
                CreatedAt = entity.CreatedAt,
                TenantId = entity.TenantId,
                Game = entity.Game,
                BestOf = entity.BestOf,
                MapPool = entity.MapPool,
            };

            if (config is not null)
            {
                league.Description = config.Description;
                league.OrganiserId = config.OrganiserId;
                league.RealmId = config.RealmId;
                league.State = config.State;
                league.Legs = config.Legs;
                league.IsRegistrationOpen = config.IsRegistrationOpen;
                league.RegistrationPeriodStart = config.RegistrationPeriodStart;
                league.RegistrationPeriodEnd = config.RegistrationPeriodEnd;
                league.LeaguePeriodStart = config.LeaguePeriodStart;
                league.LeaguePeriodEnd = config.LeaguePeriodEnd;
                league.DeletedAt = config.DeletedAt;
                league.DeletedBy = config.DeletedBy;
            }

            return league;
        }
    }

    extension (DivisionEntity entity)
    {
        internal Division ToDomain()
        {
            return new Division
            {
                Id = entity.Id,
                LeagueId = entity.CompetitionId,
                Name = entity.Name,
                Slug = entity.Slug,
                Order = entity.Order,
                BestOf = entity.BestOf,
                MaxTeamsPerGroup = entity.MaxTeamsPerGroup,
                TenantId = entity.TenantId,
                CreatedAt = entity.CreatedAt,
            };
        }
    }

    extension (DivisionGroupEntity entity)
    {
        internal DivisionGroup ToDomain()
        {
            return new DivisionGroup
            {
                Id = entity.Id,
                DivisionId = entity.DivisionId,
                LeagueId = entity.Division?.CompetitionId ?? string.Empty,
                Name = entity.Name,
                Slug = entity.Slug,
                Order = entity.Order,
                TenantId = entity.TenantId,
                CreatedAt = entity.CreatedAt,
                Teams = entity.Teams?.Select(t => new DivisionGroupTeam
                {
                    TeamId = t.TeamId,
                    GroupId = t.GroupId,
                    LeagueId = entity.Division?.CompetitionId ?? string.Empty,
                }).ToList() ?? new(),
            };
        }
    }

    extension (LeagueTeamEntity entity)
    {
        internal CompetitionTeam ToDomain()
        {
            return new CompetitionTeam
            {
                LeagueId = entity.LeagueId,
                TeamId = entity.TeamId,
                TenantId = entity.TenantId,
                CreatedAt = entity.CreatedAt,
                Status = entity.Status,
            };
        }
    }

    extension (TournamentStageEntity entity)
    {
        internal Stage ToDomain()
        {
            var stage = new Stage
            {
                Name = entity.Name,
                Format = entity.Format,
                Order = entity.Order,
                Advancing = entity.Advancing,
            };

            if (!string.IsNullOrEmpty(entity.FormatConfigJson))
            {
                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                switch (entity.Format)
                {
                    case TournamentFormat.SingleElimination:
                        stage.SingleEliminationConfig = JsonSerializer.Deserialize<SingleEliminationStageConfig>(entity.FormatConfigJson, opts);
                        break;
                    case TournamentFormat.DoubleElimination:
                        stage.DoubleEliminationConfig = JsonSerializer.Deserialize<DoubleEliminationStageConfig>(entity.FormatConfigJson, opts);
                        break;
                    case TournamentFormat.Swiss:
                        stage.SwissConfig = JsonSerializer.Deserialize<SwissStageConfig>(entity.FormatConfigJson, opts);
                        break;
                    case TournamentFormat.RoundRobin:
                        stage.RoundRobinConfig = JsonSerializer.Deserialize<RoundRobinStageConfig>(entity.FormatConfigJson, opts);
                        break;
                }
            }

            return stage;
        }
    }

    extension (MatchEntity entity)
    {
        internal Domain.Competitions.Matches.Models.Match ToMatchDomain()
        {
            Competition? competition = null;
            if (entity.Competition is not null)
            {
                competition = entity.Competition.Type switch
                {
                    CompetitionType.Tournament => entity.Competition.ToTournamentDomain(),
                    CompetitionType.League => entity.Competition.ToLeagueDomain(),
                    _ => null
                };
            }

            return new Domain.Competitions.Matches.Models.Match
            {
                Id = entity.Id,
                CompetitionId = entity.CompetitionId,
                HomeTeamId = entity.HomeTeamId,
                AwayTeamId = entity.AwayTeamId,
                ScoreHome = entity.ScoreHome,
                ScoreAway = entity.ScoreAway,
                WinnerTeamId = entity.WinnerTeamId,
                LoserTeamId = entity.LoserTeamId,
                Round = entity.Round,
                MatchDate = entity.MatchDate,
                CreatedAt = entity.CreatedAt,
                TenantId = entity.TenantId,
                Competition = competition,
                Meta = new Domain.Competitions.Matches.Models.MatchMeta(),
            };
        }
    }
}
