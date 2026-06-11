using System.Reflection;
using System.Runtime.Loader;

namespace competitions.Infrastructure.Plugins;

/// <summary>
/// Isolated load context for a single engine plugin DLL. It resolves the
/// plugin's own private dependencies from alongside the DLL, but defers any
/// assembly the host has already loaded — most importantly <c>Revyne.Engine.Api</c> —
/// to the default context. That deferral is what makes a plugin's
/// <c>ICompetitionEngine</c> the *same* type the host knows, so discovery and
/// dispatch work across the boundary.
/// </summary>
internal sealed class PluginLoadContext : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver _resolver;

    public PluginLoadContext(string pluginPath) : base(isCollectible: false)
    {
        _resolver = new AssemblyDependencyResolver(pluginPath);
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        // Share anything already loaded by the host (the engine contract, the
        // framework) so types unify by identity rather than being duplicated.
        if (Default.Assemblies.Any(a => a.GetName().Name == assemblyName.Name))
            return null;

        var path = _resolver.ResolveAssemblyToPath(assemblyName);
        return path is null ? null : LoadFromAssemblyPath(path);
    }

    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        var path = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        return path is null ? IntPtr.Zero : LoadUnmanagedDllFromPath(path);
    }
}
