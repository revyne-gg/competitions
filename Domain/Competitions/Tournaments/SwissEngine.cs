using competitions.Domain.Competitions.Matches.Models;
using competitions.Domain.Competitions.Shared.Models;
using competitions.Domain.Competitions.Tournaments.Models;
using competitions.Domain.Models;
using competitions.Shared;

namespace competitions.Domain.Competitions.Tournaments;

public class SwissEngine : ITournamentEngine
{
    public TournamentFormat Format => TournamentFormat.Swiss;

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

        if (match.ScoreHome > match.ScoreAway)
        {
            match.WinnerTeamId = match.HomeTeamId;
            match.LoserTeamId = match.AwayTeamId;
        }
        else if (match.ScoreAway > match.ScoreHome)
        {
            match.WinnerTeamId = match.AwayTeamId;
            match.LoserTeamId = match.HomeTeamId;
        }
        // Ties are valid in Swiss — no winner/loser is set

        return Task.FromResult<Result<Unit, CompetitionError>>(Unit.Value);
    }

    public async Task<Result<Unit, CompetitionError>> Tick()
    {
        throw new NotImplementedException();
    }
}
