using competitions.Application;
using competitions.Application.Ports;
using competitions.Application.UseCases;
using competitions.Domain.Competitions.Tournaments.Models;
using competitions.Shared;
using Grpc.Core;
using Revyne.Services.Competitions.V1;
using Tournament = Revyne.Services.Competitions.V1.Tournament;

namespace competitions.Transport.Services;

public class CompetitionsService(
    CreateTournamentUseCase createTournamentUseCase,
    ITournamentRepository tournamentRepo
) : Revyne.Services.Competitions.V1.CompetitionsService.CompetitionsServiceBase
{
    public override async Task<Tournament> GetTournamentById(GetTournamentByIdRequest request, ServerCallContext context)
    {
        var res = await tournamentRepo.GetByIdAsync(request.TournamentId, request.TenantId);
        if (res.IsFailure)
            throw (res.Error == RepositoryError.NotFound
                ? new RpcException(new Status(StatusCode.NotFound, "Not found."))
                : new RpcException(new Status(StatusCode.Internal, "Internal error.")));
        return res.Value!.ToGrpc();
    }

    public override async Task<Tournament> CreateTournament(CreateTournamentRequest request, ServerCallContext context)
    {
        var tournamentConfig = new TournamentConfig
        {
            Name = request.Name,
            Description = request.Description,
            Format = request.Format.ToDomain(),
            Game = request.Game,
            BestOf = request.BestOf,
            MapPool = request.MapPool.Count > 0 ? request.MapPool.ToList() : null,
            SeedingType = request.SeedingType.ToDomain(),
            BracketReset = request.BracketReset,
            MaxParticipants = request.MaxParticipants,
            Stages = request.Stages.Select(GrpcInputMapper.StageToDomain).ToList(),
        };
        
        var res = await createTournamentUseCase.Execute(
            tournamentConfig,
            request.OrganiserId,
            request.RealmId,
            request.UserId,
            request.TenantId
        );

        if (res.IsFailure)
        {
            throw res.Error!.Value.ToGrpcError();
        }
        
        return res.Value!.ToGrpc();
    }
}