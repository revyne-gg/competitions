using competitions.Domain.Competitions.Leagues.Models;
using competitions.Domain.Competitions.Matches.Models;
using competitions.Domain.Competitions.Shared.Models;
using competitions.Domain.Competitions.Tournaments.Models;
using DomainLeague = competitions.Domain.Models.League;
using Engine = Revyne.Engine.Api;

namespace competitions.Application.Mapping;

/// <summary>
/// Translates the service's domain models into the engine's transport-free
/// contract types. This is the single boundary between persistence/domain and
/// the (swappable) engine library — the engine never sees a domain entity.
/// </summary>
public static class EngineMapper
{
    public static Engine.TournamentEngineConfig ToEngineConfig(this Tournament tournament) => new()
    {
        Format = tournament.Format.ToEngine(),
        SeedingType = tournament.SeedingType.ToEngine(),
        BracketReset = tournament.BracketReset,
        BestOf = tournament.BestOf,
    };

    public static Engine.LeagueEngineConfig ToEngineConfig(this DomainLeague league) => new()
    {
        Legs = league.Legs.ToEngine(),
        BestOf = league.BestOf,
    };

    public static Engine.EngineCompetitionConfig ToEngineConfig(this Competition competition) => competition switch
    {
        Tournament tournament => tournament.ToEngineConfig(),
        DomainLeague league => league.ToEngineConfig(),
        _ => throw new ArgumentOutOfRangeException(nameof(competition), competition.GetType(), "Unknown competition type"),
    };

    public static AppError ToAppError(this Engine.CompetitionError? error) => error switch
    {
        Engine.CompetitionError.InvalidArguments => AppError.BadRequest,
        Engine.CompetitionError.NotFound => AppError.NotFound,
        Engine.CompetitionError.UnsupportedFormat => AppError.BadRequest,
        _ => AppError.InternalError,
    };

    public static Engine.FinishedMatch ToFinishedMatch(this Match match) => new()
    {
        HomeTeamId = match.HomeTeamId,
        AwayTeamId = match.AwayTeamId,
        ScoreHome = match.ScoreHome,
        ScoreAway = match.ScoreAway,
        WinnerTeamId = match.WinnerTeamId,
        Round = match.Round ?? 1,
    };

    private static Engine.TournamentFormat ToEngine(this TournamentFormat format) => format switch
    {
        TournamentFormat.SingleElimination => Engine.TournamentFormat.SingleElimination,
        TournamentFormat.DoubleElimination => Engine.TournamentFormat.DoubleElimination,
        TournamentFormat.Swiss => Engine.TournamentFormat.Swiss,
        TournamentFormat.RoundRobin => Engine.TournamentFormat.RoundRobin,
        _ => throw new ArgumentOutOfRangeException(nameof(format), format, null),
    };

    private static Engine.SeedingType ToEngine(this SeedingType seeding) => seeding switch
    {
        SeedingType.Random => Engine.SeedingType.Random,
        // Manual seeding is not modelled by the engine yet — treat as standard.
        _ => Engine.SeedingType.Standard,
    };

    private static Engine.LeagueLegs ToEngine(this LeagueLegs legs) => legs switch
    {
        LeagueLegs.TwoLegs => Engine.LeagueLegs.TwoLegs,
        _ => Engine.LeagueLegs.OneLeg,
    };
}
