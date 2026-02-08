using competitions.Application;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Revyne.Services.Competitions.V1;

namespace competitions.Transport;

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
            return new Tournament
            {
                Id = tournament.Id,
                Discriminator = tournament.Discriminator,
                Name = tournament.Name,
                Description = tournament.Description,
                Format = tournament.Format.ToGrpc(),
                CreatedAt = tournament.CreatedAt.ToTimestamp()
            };
        }
    } 
}