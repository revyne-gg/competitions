using leagues.Application.DTO;
using leagues.Application.Ports;
using leagues.Shared;

namespace leagues.Application.UseCases;

public sealed class CreateLeagueUseCase(ILeagueRepository repo)
{
    public async Task<Result<LeagueDTO, AppError>> Execute(string name, string realmId, string userId, string tenantId)
    {
        
    }
}