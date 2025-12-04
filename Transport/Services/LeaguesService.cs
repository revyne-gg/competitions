using Grpc.Core;

namespace leagues.Services;

public class LeaguesService(ILogger<LeaguesService> logger) : Brackzr.Leagues.V1.LeaguesService.LeaguesServiceBase
{
    
}