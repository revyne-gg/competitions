using Revyne.Engine.Api;

namespace competitions.Application.Engines;

/// <summary>
/// Open baseline implementation of <see cref="ICompetitionEngine"/> shipped with
/// the service so it builds, runs and is testable standalone. Implements the
/// standard, non-proprietary algorithms (bracket seeding, round-robin, winner
/// progression). A deployment may override this with a more capable engine —
/// see Program.cs (registered via TryAdd) and the private engine's HostingStartup.
/// </summary>
public sealed class ReferenceCompetitionEngine : ICompetitionEngine
{
    /// <summary>
    /// Keys the baseline answers to. It also acts as the catch-all fallback (see
    /// the dispatcher), so it claims the default key and an explicit "reference"
    /// alias.
    /// </summary>
    public bool CanHandle(string engineKey) =>
        string.IsNullOrEmpty(engineKey)
        || engineKey == EngineCompetitionConfig.DefaultEngineKey
        || engineKey == "reference";

    public Result<IReadOnlyList<MatchSpec>, CompetitionError> GenerateInitialMatches(
        EngineCompetitionConfig config, IReadOnlyList<string> teamIds)
    {
        if (teamIds.Count < 2)
            return CompetitionError.InvalidArguments;

        return config switch
        {
            TournamentEngineConfig { Format: TournamentFormat.RoundRobin } => RoundRobin(teamIds),
            TournamentEngineConfig => StandardBracket(teamIds),
            LeagueEngineConfig league => LeagueFixtures(league, teamIds),
            _ => CompetitionError.UnsupportedFormat,
        };
    }

    public Result<IReadOnlyList<MatchSpec>, CompetitionError> GenerateNextMatches(
        EngineCompetitionConfig config, IReadOnlyList<FinishedMatch> playedMatches)
    {
        // Only single/double elimination progress round-by-round; everything else
        // generates all fixtures up front.
        if (config is not TournamentEngineConfig
            { Format: TournamentFormat.SingleElimination or TournamentFormat.DoubleElimination })
            return new List<MatchSpec>();

        if (playedMatches.Count == 0)
            return CompetitionError.InvalidArguments;

        var currentRound = playedMatches.Max(m => m.Round);
        var currentRoundMatches = playedMatches.Where(m => m.Round == currentRound).ToList();

        if (currentRoundMatches.Any(m => m.WinnerTeamId is null))
            return CompetitionError.InvalidArguments;

        var winners = currentRoundMatches.Select(m => m.WinnerTeamId!).ToList();

        if (winners.Count <= 1)
            return new List<MatchSpec>(); // complete

        if (winners.Count % 2 != 0)
            return CompetitionError.InvalidArguments;

        var next = new List<MatchSpec>();
        for (var i = 0; i < winners.Count; i += 2)
            next.Add(new MatchSpec { HomeTeamId = winners[i], AwayTeamId = winners[i + 1], Round = currentRound + 1 });

        return next;
    }

    public Result<MatchOutcome, CompetitionError> ResolveOutcome(
        EngineCompetitionConfig config, FinishedMatch match)
    {
        if (match is not { ScoreHome: not null, ScoreAway: not null, HomeTeamId: not null, AwayTeamId: not null })
            return CompetitionError.InvalidArguments;

        var drawsAllowed = config is not TournamentEngineConfig
            { Format: TournamentFormat.SingleElimination or TournamentFormat.DoubleElimination };

        if (match.ScoreHome == match.ScoreAway)
            return drawsAllowed
                ? new MatchOutcome { IsDraw = true }
                : CompetitionError.InvalidArguments;

        return match.ScoreHome > match.ScoreAway
            ? new MatchOutcome { WinnerTeamId = match.HomeTeamId, LoserTeamId = match.AwayTeamId }
            : new MatchOutcome { WinnerTeamId = match.AwayTeamId, LoserTeamId = match.HomeTeamId };
    }

    // 1 v n, 2 v n-1, ... (odd middle team would advance as a bye).
    private static List<MatchSpec> StandardBracket(IReadOnlyList<string> teams)
    {
        var pairs = new List<MatchSpec>();
        var count = teams.Count;
        for (var i = 0; i < count / 2; i++)
            pairs.Add(new MatchSpec { HomeTeamId = teams[i], AwayTeamId = teams[count - 1 - i], Round = 1 });
        return pairs;
    }

    private static List<MatchSpec> RoundRobin(IReadOnlyList<string> teams)
    {
        var pairs = new List<MatchSpec>();
        for (var i = 0; i < teams.Count; i++)
        for (var j = i + 1; j < teams.Count; j++)
            pairs.Add(new MatchSpec { HomeTeamId = teams[i], AwayTeamId = teams[j], Round = 1 });
        return pairs;
    }

    private static List<MatchSpec> LeagueFixtures(LeagueEngineConfig config, IReadOnlyList<string> teams)
    {
        var fixtures = RoundRobin(teams);
        if (config.Legs == LeagueLegs.TwoLegs)
            foreach (var f in fixtures.ToArray())
                fixtures.Add(new MatchSpec { HomeTeamId = f.AwayTeamId, AwayTeamId = f.HomeTeamId, Round = f.Round });
        return fixtures;
    }
}
