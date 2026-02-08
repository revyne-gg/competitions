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

        internal League ToLeagueDomain()
        {
            var league = new League
            {
                Id = entity.Id,
                Name = entity.Name,
                Discriminator = entity.Discriminator,
                CreatedAt = entity.CreatedAt,
                TenantId = entity.TenantId,
            };

            return league;
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
