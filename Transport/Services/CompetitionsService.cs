using competitions.Application.UseCases;
using competitions.Domain.Competitions.Tournaments.Models;
using Grpc.Core;
using Revyne.Services.Competitions.V1;
using Tournament = Revyne.Services.Competitions.V1.Tournament;

namespace competitions.Transport.Services;

public class CompetitionsService(
    CreateTournamentUseCase createTournamentUseCase
) : Revyne.Services.Competitions.V1.CompetitionsService.CompetitionsServiceBase
{
    public override async Task<Tournament> CreateTournament(CreateTournamentRequest request, ServerCallContext context)
    {
        var tournamentConfig = new TournamentConfig
        {
            Name = request.Name,
            Description = request.Description,
            Format = request.Format.ToDomain(),
        };
        
        var res = await createTournamentUseCase.Execute(
            tournamentConfig, 
            request.UserId, 
            request.RealmId, 
            request.OrganiserId, 
            request.TenantId
        );

        if (res.IsFailure)
        {
            throw res.Error!.Value.ToGrpcError();
        }
        
        return res.Value!.ToGrpc();
    }
}