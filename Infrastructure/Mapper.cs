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
        };
    }

    internal static LeagueConfig ToDomain(this LeagueConfigEntity entity)
    {
        return new LeagueConfig();
    }

    extension (CompetitionEntity entity)
    {
        internal Tournament ToTournamentDomain(TournamentConfigEntity? config = null)
        {
            return new Tournament
            {
                Id = entity.Id,
                Name = entity.Name,
                Discriminator = entity.Discriminator,
                CreatedAt = entity.CreatedAt,
                TenantId = entity.TenantId,
                Format = config?.Format ?? default,
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
            };

            if (config is not null)
            {
                league.Description = config.Description;
                league.OrganiserId = config.OrganiserId;
                league.RealmId = config.RealmId;
                league.State = config.State;
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
                MatchDate = entity.MatchDate,
                CreatedAt = entity.CreatedAt,
                TenantId = entity.TenantId,
                Competition = competition,
                Meta = new Domain.Competitions.Matches.Models.MatchMeta(),
            };
        }
    }
}
