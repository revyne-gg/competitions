using Revyne.Engine.Api;

namespace competitions.Application.Engines;

/// <summary>
/// The ordered set of engine implementation types the service can dispatch to.
/// Populated once at startup (plugins first, the reference engine last as the
/// fallback) and consumed by <see cref="DispatchingCompetitionEngine"/>.
/// </summary>
public sealed class CompetitionEngineCatalog(IReadOnlyList<Type> engineTypes)
{
    public IReadOnlyList<Type> EngineTypes { get; } = engineTypes;
}

/// <summary>
/// The single <see cref="ICompetitionEngine"/> the use cases depend on. It owns
/// no fixture logic; it picks the registered engine whose <see cref="ICompetitionEngine.CanHandle"/>
/// matches the config's <see cref="EngineCompetitionConfig.EngineKey"/> and
/// forwards the call. The first match wins, so plugins (registered ahead of the
/// reference engine) override the baseline for keys they claim.
/// </summary>
public sealed class DispatchingCompetitionEngine(
    IServiceProvider services,
    CompetitionEngineCatalog catalog
) : ICompetitionEngine
{
    // The dispatcher itself answers any key — it delegates to whatever can.
    public bool CanHandle(string engineKey) => TryResolve(engineKey, out _);

    public Result<IReadOnlyList<MatchSpec>, CompetitionError> GenerateInitialMatches(
        EngineCompetitionConfig config, IReadOnlyList<string> teamIds) =>
        TryResolve(config.EngineKey, out var engine)
            ? engine!.GenerateInitialMatches(config, teamIds)
            : CompetitionError.UnsupportedFormat;

    public Result<IReadOnlyList<MatchSpec>, CompetitionError> GenerateNextMatches(
        EngineCompetitionConfig config, IReadOnlyList<FinishedMatch> playedMatches) =>
        TryResolve(config.EngineKey, out var engine)
            ? engine!.GenerateNextMatches(config, playedMatches)
            : CompetitionError.UnsupportedFormat;

    public Result<MatchOutcome, CompetitionError> ResolveOutcome(
        EngineCompetitionConfig config, FinishedMatch match) =>
        TryResolve(config.EngineKey, out var engine)
            ? engine!.ResolveOutcome(config, match)
            : CompetitionError.UnsupportedFormat;

    private bool TryResolve(string engineKey, out ICompetitionEngine? engine)
    {
        foreach (var type in catalog.EngineTypes)
        {
            var candidate = (ICompetitionEngine)services.GetRequiredService(type);
            if (candidate.CanHandle(engineKey))
            {
                engine = candidate;
                return true;
            }
        }

        engine = null;
        return false;
    }
}
