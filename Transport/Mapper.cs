using competitions.Application;
using competitions.Domain.Models;
using competitions.Domain.Competitions.Leagues.Models;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Revyne.Services.Competitions.V1;
using GrpcLeague = Revyne.Services.Competitions.V1.League;
using GrpcDivision = Revyne.Services.Competitions.V1.Division;
using GrpcDivisionGroup = Revyne.Services.Competitions.V1.DivisionGroup;
using GrpcLeagueStatus = Revyne.Services.Competitions.V1.LeagueStatus;
using GrpcLeagueLegs = Revyne.Services.Competitions.V1.LeagueLegs;
using GrpcRegistrationStatus = Revyne.Services.Competitions.V1.RegistrationStatus;
using GrpcSeedingType = Revyne.Services.Competitions.V1.SeedingType;
using GrpcStage = Revyne.Services.Competitions.V1.TournamentStage;
using GrpcStageRound = Revyne.Services.Competitions.V1.StageRound;
using DomainLeague = competitions.Domain.Models.League;
using DomainLeagueStatus = competitions.Domain.Models.LeagueStatus;
using DomainLeagueLegs = competitions.Domain.Competitions.Leagues.Models.LeagueLegs;
using DomainSeedingType = competitions.Domain.Competitions.Tournaments.Models.SeedingType;
using DomainStage = competitions.Domain.Competitions.Tournaments.Models.Stage;
using DomainStageRound = competitions.Domain.Competitions.Tournaments.Models.StageRound;
using DomainDivision = competitions.Domain.Models.Division;
using DomainDivisionGroup = competitions.Domain.Models.DivisionGroup;
using DomainRegistrationStatus = competitions.Domain.Models.RegistrationStatus;

namespace competitions.Transport;

// Domain → gRPC mappings
internal static class Mapper
{
    extension(AppError error)
    {
        public RpcException ToGrpcError()
        {
            return error switch
            {
                AppError.NotFound => new RpcException(new Status(StatusCode.NotFound, "Not found.")),
                AppError.Forbidden => new RpcException(new Status(StatusCode.PermissionDenied, "Permission denied.")),
                AppError.BadRequest => new RpcException(new Status(StatusCode.InvalidArgument, "Bad request")),
                _ => new RpcException(new Status(StatusCode.Internal, "Internal error."))
            };
        }
    }

    extension(TournamentFormat format)
    {
        internal Domain.Competitions.Tournaments.Models.TournamentFormat ToDomain()
        {
            switch (format)
            {
                case TournamentFormat.SingleElimination:
                    return Domain.Competitions.Tournaments.Models.TournamentFormat.SingleElimination;
                case TournamentFormat.DoubleElimination:
                    return Domain.Competitions.Tournaments.Models.TournamentFormat.DoubleElimination;
                case TournamentFormat.RoundRobin:
                    return Domain.Competitions.Tournaments.Models.TournamentFormat.RoundRobin;
                case TournamentFormat.Swiss:
                    return Domain.Competitions.Tournaments.Models.TournamentFormat.Swiss;
            }

            return default;
        }
    }

    extension(Domain.Competitions.Tournaments.Models.TournamentFormat format)
    {
        internal TournamentFormat ToGrpc()
        {
            switch (format)
            {
                case Domain.Competitions.Tournaments.Models.TournamentFormat.SingleElimination:
                    return TournamentFormat.SingleElimination;
                case Domain.Competitions.Tournaments.Models.TournamentFormat.DoubleElimination:
                    return TournamentFormat.DoubleElimination;
                case Domain.Competitions.Tournaments.Models.TournamentFormat.RoundRobin:
                    return TournamentFormat.RoundRobin;
                case Domain.Competitions.Tournaments.Models.TournamentFormat.Swiss:
                    return TournamentFormat.Swiss;
            }

            return default;
        }
    }

    extension(Domain.Competitions.Tournaments.Models.Tournament tournament)
    {
        internal Tournament ToGrpc()
        {
            var grpc = new Tournament
            {
                Id = tournament.Id,
                Discriminator = tournament.Discriminator,
                Name = tournament.Name,
                Description = tournament.Description ?? string.Empty,
                Format = tournament.Format.ToGrpc(),
                CreatedAt = tournament.CreatedAt.ToTimestamp(),
                Game = tournament.Game ?? string.Empty,
                BestOf = tournament.BestOf,
                SeedingType = tournament.SeedingType.ToGrpc(),
                BracketReset = tournament.BracketReset,
                MaxParticipants = tournament.MaxParticipants,
                OrganiserId = tournament.OrganiserId ?? string.Empty,
                RealmId = tournament.RealmId ?? string.Empty,
            };
            if (tournament.MapPool is not null)
                grpc.MapPool.AddRange(tournament.MapPool);
            grpc.Stages.AddRange(tournament.Stages.Select(StageToGrpc));
            return grpc;
        }
    }

    internal static GrpcStage StageToGrpc(DomainStage stage)
    {
        var grpc = new GrpcStage
        {
            Name = stage.Name,
            Format = stage.Format.ToGrpc(),
            Order = stage.Order,
            Advancing = stage.Advancing,
        };

        if (stage.SingleEliminationConfig is not null)
        {
            grpc.SingleEliminationConfig = new Revyne.Services.Competitions.V1.SingleEliminationStageConfig();
            grpc.SingleEliminationConfig.Rounds.AddRange(
                stage.SingleEliminationConfig.Rounds.Select(r => new GrpcStageRound { Name = r.Name, BestOf = r.BestOf }));
        }
        if (stage.DoubleEliminationConfig is not null)
        {
            grpc.DoubleEliminationConfig = new Revyne.Services.Competitions.V1.DoubleEliminationStageConfig
            {
                BracketReset = stage.DoubleEliminationConfig.BracketReset,
            };
            grpc.DoubleEliminationConfig.Rounds.AddRange(
                stage.DoubleEliminationConfig.Rounds.Select(r => new GrpcStageRound { Name = r.Name, BestOf = r.BestOf }));
        }
        if (stage.SwissConfig is not null)
        {
            grpc.SwissConfig = new Revyne.Services.Competitions.V1.SwissStageConfig
            {
                PointsFormula = stage.SwissConfig.PointsFormula ?? string.Empty,
            };
        }
        if (stage.RoundRobinConfig is not null)
        {
            grpc.RoundRobinConfig = new Revyne.Services.Competitions.V1.RoundRobinStageConfig
            {
                MaxGroupSize = stage.RoundRobinConfig.MaxGroupSize,
                PointsPerWin = stage.RoundRobinConfig.PointsPerWin,
                PointsPerDraw = stage.RoundRobinConfig.PointsPerDraw,
                PointsPerLoss = stage.RoundRobinConfig.PointsPerLoss,
            };
        }

        return grpc;
    }

    // ── League mappings ────────────────────────────────────────────────────────

    extension(DomainLeague league)
    {
        internal GrpcLeague ToGrpc()
        {
            var grpc = new GrpcLeague
            {
                Id = league.Id,
                Name = league.Name,
                Discriminator = league.Discriminator,
                Description = league.Description ?? string.Empty,
                OrganiserId = league.OrganiserId ?? string.Empty,
                RealmId = league.RealmId ?? string.Empty,
                State = league.State.ToGrpc(),
                Legs = league.Legs.ToGrpc(),
                CreatedAt = league.CreatedAt.ToTimestamp(),
                Game = league.Game ?? string.Empty,
                BestOf = league.BestOf,
            };
            if (league.MapPool is not null)
                grpc.MapPool.AddRange(league.MapPool);
            if (league.RegistrationPeriodStart.HasValue)
                grpc.RegistrationPeriodStart = league.RegistrationPeriodStart.Value.ToTimestamp();
            if (league.RegistrationPeriodEnd.HasValue)
                grpc.RegistrationPeriodEnd = league.RegistrationPeriodEnd.Value.ToTimestamp();
            if (league.LeaguePeriodStart.HasValue)
                grpc.LeaguePeriodStart = league.LeaguePeriodStart.Value.ToTimestamp();
            if (league.LeaguePeriodEnd.HasValue)
                grpc.LeaguePeriodEnd = league.LeaguePeriodEnd.Value.ToTimestamp();
            return grpc;
        }
    }

    extension(DomainLeagueStatus status)
    {
        internal GrpcLeagueStatus ToGrpc() => status switch
        {
            DomainLeagueStatus.Public   => GrpcLeagueStatus.Public,
            DomainLeagueStatus.Live     => GrpcLeagueStatus.Live,
            DomainLeagueStatus.Finished => GrpcLeagueStatus.Finished,
            _                           => GrpcLeagueStatus.Hidden,
        };
    }

    extension(DomainLeagueLegs legs)
    {
        internal GrpcLeagueLegs ToGrpc() => legs switch
        {
            DomainLeagueLegs.TwoLegs => GrpcLeagueLegs.TwoLegs,
            _                        => GrpcLeagueLegs.OneLeg,
        };
    }

    extension(DomainSeedingType seedingType)
    {
        internal GrpcSeedingType ToGrpc() => seedingType switch
        {
            DomainSeedingType.Random => GrpcSeedingType.Random,
            DomainSeedingType.Manual => GrpcSeedingType.Manual,
            _                       => GrpcSeedingType.Standard,
        };
    }

    // ── Division mappings ──────────────────────────────────────────────────────

    extension(DomainDivision division)
    {
        internal GrpcDivision ToGrpc()
        {
            return new GrpcDivision
            {
                Id = division.Id,
                LeagueId = division.LeagueId,
                Name = division.Name,
                Slug = division.Slug,
                Order = division.Order,
                BestOf = division.BestOf,
                MaxTeamsPerGroup = division.MaxTeamsPerGroup,
                CreatedAt = division.CreatedAt.ToTimestamp(),
            };
        }
    }

    // ── DivisionGroup mappings ─────────────────────────────────────────────────

    extension(DomainDivisionGroup group)
    {
        internal GrpcDivisionGroup ToGrpc()
        {
            var grpc = new GrpcDivisionGroup
            {
                Id = group.Id,
                DivisionId = group.DivisionId,
                LeagueId = group.LeagueId,
                Name = group.Name,
                Slug = group.Slug,
                Order = group.Order,
                CreatedAt = group.CreatedAt.ToTimestamp(),
            };
            grpc.Teams.AddRange(group.Teams.Select(t => new DivisionGroupTeamSummary { TeamId = t.TeamId }));
            return grpc;
        }
    }

    // ── Registration mappings ──────────────────────────────────────────────────

    extension(CompetitionTeam team)
    {
        internal LeagueRegistration ToGrpc()
        {
            return new LeagueRegistration
            {
                LeagueId = team.LeagueId,
                TeamId = team.TeamId,
                Status = team.Status.ToGrpc(),
                CreatedAt = team.CreatedAt.ToTimestamp(),
            };
        }
    }

    extension(TournamentTeam tournamentTeam)
    {
        internal TournamentRegistration ToGrpc()
        {
            return new TournamentRegistration
            {
                TournamentId = tournamentTeam.TournamentId,
                TeamId = tournamentTeam.TeamId,
                Status = tournamentTeam.Status.ToGrpc(),
                CreatedAt = tournamentTeam.CreatedAt.ToTimestamp(),
            };
        }
    }

    extension(DomainRegistrationStatus status)
    {
        internal GrpcRegistrationStatus ToGrpc() => status switch
        {
            DomainRegistrationStatus.Approved => GrpcRegistrationStatus.Approved,
            DomainRegistrationStatus.Rejected => GrpcRegistrationStatus.Rejected,
            _                                 => GrpcRegistrationStatus.Pending,
        };
    }
}

// gRPC → Domain mappings (in separate static class to avoid extension block collisions)
internal static class GrpcInputMapper
{
    extension(GrpcLeagueStatus status)
    {
        internal DomainLeagueStatus ToDomain() => status switch
        {
            GrpcLeagueStatus.Public   => DomainLeagueStatus.Public,
            GrpcLeagueStatus.Live     => DomainLeagueStatus.Live,
            GrpcLeagueStatus.Finished => DomainLeagueStatus.Finished,
            _                         => DomainLeagueStatus.Hidden,
        };
    }

    extension(GrpcLeagueLegs legs)
    {
        internal DomainLeagueLegs ToDomain() => legs switch
        {
            GrpcLeagueLegs.TwoLegs => DomainLeagueLegs.TwoLegs,
            _                      => DomainLeagueLegs.OneLeg,
        };
    }

    extension(GrpcSeedingType seedingType)
    {
        internal DomainSeedingType ToDomain() => seedingType switch
        {
            GrpcSeedingType.Random => DomainSeedingType.Random,
            GrpcSeedingType.Manual => DomainSeedingType.Manual,
            _                     => DomainSeedingType.Standard,
        };
    }

    internal static DomainStage StageToDomain(GrpcStage grpcStage)
    {
        var stage = new DomainStage
        {
            Name = grpcStage.Name,
            Format = grpcStage.Format.ToDomain(),
            Order = grpcStage.Order,
            Advancing = grpcStage.Advancing,
        };

        if (grpcStage.SingleEliminationConfig is not null)
        {
            stage.SingleEliminationConfig = new Domain.Competitions.Tournaments.Models.SingleEliminationStageConfig
            {
                Rounds = grpcStage.SingleEliminationConfig.Rounds
                    .Select(r => new DomainStageRound { Name = r.Name, BestOf = r.BestOf }).ToList(),
            };
        }
        if (grpcStage.DoubleEliminationConfig is not null)
        {
            stage.DoubleEliminationConfig = new Domain.Competitions.Tournaments.Models.DoubleEliminationStageConfig
            {
                BracketReset = grpcStage.DoubleEliminationConfig.BracketReset,
                Rounds = grpcStage.DoubleEliminationConfig.Rounds
                    .Select(r => new DomainStageRound { Name = r.Name, BestOf = r.BestOf }).ToList(),
            };
        }
        if (grpcStage.SwissConfig is not null)
        {
            stage.SwissConfig = new Domain.Competitions.Tournaments.Models.SwissStageConfig
            {
                PointsFormula = grpcStage.SwissConfig.PointsFormula,
            };
        }
        if (grpcStage.RoundRobinConfig is not null)
        {
            stage.RoundRobinConfig = new Domain.Competitions.Tournaments.Models.RoundRobinStageConfig
            {
                MaxGroupSize = grpcStage.RoundRobinConfig.MaxGroupSize,
                PointsPerWin = grpcStage.RoundRobinConfig.PointsPerWin,
                PointsPerDraw = grpcStage.RoundRobinConfig.PointsPerDraw,
                PointsPerLoss = grpcStage.RoundRobinConfig.PointsPerLoss,
            };
        }

        return stage;
    }
}