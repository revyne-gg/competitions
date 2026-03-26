namespace competitions.Domain.Competitions.Shared.Models;

public static class SupportedGames
{
    private static readonly HashSet<string> Keys = new(StringComparer.OrdinalIgnoreCase)
    {
        "cs2",
        "valorant",
        "lol",
        "dota2",
        "rocket_league",
        "overwatch2",
        "r6_siege",
        "apex",
        "fortnite",
        "cod",
        "fc26",
        "sf6",
        "tekken8",
        "ssbu",
    };

    public static bool IsSupported(string? game) => !string.IsNullOrEmpty(game) && Keys.Contains(game);
}
