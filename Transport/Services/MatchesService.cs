using competitions.Application.UseCases;
using competitions.Transport;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Revyne.Services.Competitions.V1;

namespace competitions.Services;

public class MatchesService(
    ReportMatchScoreUseCase reportMatchScoreUseCase,
    CreateMatchesForTournamentUseCase createMatchesForTournamentUseCase
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

    public override async Task<MatchList> CreateMatchesForTournament(CreateMatchesForTournamentRequest request, ServerCallContext context)
    {
        var tenantId = context.RequestHeaders.GetValue("x-tenant-id");
        if (string.IsNullOrEmpty(tenantId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Missing tenant ID."));
        }

        var result = await createMatchesForTournamentUseCase.Execute(
            request.TournamentId,
            request.TeamIds.ToList(),
            tenantId
        );

        if (result.IsFailure)
        {
            throw result.Error!.Value.ToGrpcError();
        }

        var response = new MatchList();
        foreach (var match in result.Value!)
        {
            response.Items.Add(new Match
            {
                Id = match.Id,
                CompetitionId = match.CompetitionId,
                HomeTeamId = match.HomeTeamId ?? string.Empty,
                AwayTeamId = match.AwayTeamId ?? string.Empty,
                ScoreHome = match.ScoreHome ?? 0,
                ScoreAway = match.ScoreAway ?? 0,
                WinnerTeamId = match.WinnerTeamId ?? string.Empty,
                LoserTeamId = match.LoserTeamId ?? string.Empty,
                Round = match.Round ?? 0,
                CreatedAt = Timestamp.FromDateTime(DateTime.SpecifyKind(match.CreatedAt, DateTimeKind.Utc)),
            });
        }

        return response;
    }
}
