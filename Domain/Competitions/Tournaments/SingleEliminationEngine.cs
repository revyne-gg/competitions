using competitions.Domain.Competitions.Matches.Models;
using competitions.Domain.Competitions.Shared.Models;
using competitions.Domain.Competitions.Tournaments.Models;
using competitions.Domain.Models;
using competitions.Shared;

namespace competitions.Domain.Competitions.Tournaments;

public class SingleEliminationEngine : ITournamentEngine
{
    public TournamentFormat Format => TournamentFormat.SingleElimination;

    public async Task<Result<Competition, CompetitionError>> Setup(CompetitionConfig config)
    {
        throw new NotImplementedException();
    }

    public Task<Result<Unit, CompetitionError>> OnMatchFinished(Match match)
    {
        if (match.ScoreHome is null || match.ScoreAway is null)
        {
            return Task.FromResult<Result<Unit, CompetitionError>>(CompetitionError.InvalidArguments);
        }

        if (match.HomeTeamId is null || match.AwayTeamId is null)
        {
            return Task.FromResult<Result<Unit, CompetitionError>>(CompetitionError.InvalidArguments);
        }

        // Ties are not allowed in single elimination
        if (match.ScoreHome == match.ScoreAway)
        {
            return Task.FromResult<Result<Unit, CompetitionError>>(CompetitionError.InvalidArguments);
        }

        if (match.ScoreHome > match.ScoreAway)
        {
            match.WinnerTeamId = match.HomeTeamId;
            match.LoserTeamId = match.AwayTeamId;
        }
        else
        {
            match.WinnerTeamId = match.AwayTeamId;
            match.LoserTeamId = match.HomeTeamId;
        }

        return Task.FromResult<Result<Unit, CompetitionError>>(Unit.Value);
    }

    public async Task<Result<Unit, CompetitionError>> Tick()
    {
        throw new NotImplementedException();
    }
}
