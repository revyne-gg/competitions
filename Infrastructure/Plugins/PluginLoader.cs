using Revyne.Engine.Api;

namespace competitions.Infrastructure.Plugins;

/// <summary>
/// Discovers <see cref="ICompetitionEngine"/> implementations shipped as DLLs in
/// a plugins folder. Called once at startup; the returned types are registered in
/// DI and offered to the dispatcher. Failures to load a single DLL are logged and
/// skipped — a bad plugin never takes the service down.
/// </summary>
public static class PluginLoader
{
    public static IReadOnlyList<Type> DiscoverEngineTypes(string pluginsDirectory, ILogger logger)
    {
        if (!Directory.Exists(pluginsDirectory))
        {
            logger.LogInformation("Plugin directory {Directory} not found; no engine plugins loaded.", pluginsDirectory);
            return [];
        }

        var discovered = new List<Type>();

        foreach (var dll in Directory.GetFiles(pluginsDirectory, "*.dll"))
        {
            try
            {
                var context = new PluginLoadContext(dll);
                var assembly = context.LoadFromAssemblyPath(Path.GetFullPath(dll));

                var engineTypes = assembly.GetTypes()
                    .Where(t => t is { IsClass: true, IsAbstract: false }
                                && typeof(ICompetitionEngine).IsAssignableFrom(t))
                    .ToList();

                if (engineTypes.Count == 0)
                {
                    logger.LogDebug("Plugin {Dll} contains no ICompetitionEngine implementations; skipped.", Path.GetFileName(dll));
                    continue;
                }

                foreach (var type in engineTypes)
                {
                    discovered.Add(type);
                    logger.LogInformation("Loaded engine plugin {Engine} from {Dll}.", type.FullName, Path.GetFileName(dll));
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to load engine plugin {Dll}; skipping.", Path.GetFileName(dll));
            }
        }

        return discovered;
    }
}
