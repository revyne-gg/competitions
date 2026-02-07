using Brackzr.Leagues.V1;
using Grpc.Core;

namespace competitions.Services;

public class LeaguesService(ILogger<LeaguesService> logger) : Brackzr.Leagues.V1.LeaguesService.LeaguesServiceBase
{
    public Task<League> GetLeagueByIdRequest(GetLeagueByIdRequest request, ServerCallContext context)
    {
        return base.GetTenantById(request, context);
    }
}