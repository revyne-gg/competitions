using competitions.Domain.Models;

namespace competitions.Domain.Competitions.Tournaments.Models;

public class TournamentConfig : CompetitionConfig
{
    public TournamentFormat Format { get; set; }
    public SeedingType SeedingType { get; set; } = SeedingType.Standard;
    public bool BracketReset { get; set; }
    public int MaxParticipants { get; set; }
    public List<Stage> Stages { get; set; } = new();
}
