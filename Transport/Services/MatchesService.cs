using competitions.Application.UseCases;
using competitions.Transport;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Revyne.Services.Competitions.V1;

namespace competitions.Services;

public class MatchesService(
    ReportMatchScoreUseCase reportMatchScoreUseCase
) : Revyne.Services.Competitions.V1.MatchesService.MatchesServiceBase
{
    public override async Task<Empty> ReportScoresForMatch(ReportScoresForMatchRequest request, ServerCallContext context)
    {
        var tenantId = context.RequestHeaders.GetValue("x-tenant-id");
        if (string.IsNullOrEmpty(tenantId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Missing tenant ID."));
        }

        var result = await reportMatchScoreUseCase.Execute(
            request.MatchId,
            request.ScoreHome,
            request.ScoreAway,
            tenantId
        );

        if (result.IsFailure)
        {
            throw result.Error!.Value.ToGrpcError();
        }

        return new Empty();
    }
}
