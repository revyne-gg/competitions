using competitions.Domain.Competitions.Matches.Models;
using competitions.Domain.Competitions.Shared.Models;
using competitions.Domain.Competitions.Tournaments.Models;
using competitions.Domain.Models;
using competitions.Shared;

namespace competitions.Domain.Competitions.Tournaments;

public class TournamentEngine(
    IEnumerable<ITournamentEngine> engines
) : ICompetitionEngine
{
    private ITournamentEngine? _activeEngine;

    public CompetitionType Type => CompetitionType.Tournament;

    public async Task<Result<Competition, CompetitionError>> Setup(CompetitionConfig config)
    {
        if (config is not TournamentConfig tournamentConfig)
        {
            return CompetitionError.InvalidArguments;
        }

        _activeEngine = engines.FirstOrDefault(x => x.Format == tournamentConfig.Format);

        if (_activeEngine is null)
        {
            return CompetitionError.InternalError;
        }

        return await _activeEngine.Setup(config);
    }

    public async Task<Result<Unit, CompetitionError>> OnMatchFinished(Match match)
    {
        if (_activeEngine is null)
        {
            if (match.Competition is Tournament tournament)
            {
                _activeEngine = engines.FirstOrDefault(x => x.Format == tournament.Format);
            }
        }

        if (_activeEngine is null)
        {
            return CompetitionError.InternalError;
        }

        return await _activeEngine.OnMatchFinished(match);
    }

    public async Task<Result<Unit, CompetitionError>> Tick()
    {
        if (_activeEngine is null)
        {
            return CompetitionError.InternalError;
        }

        return await _activeEngine.Tick();
    }
}
