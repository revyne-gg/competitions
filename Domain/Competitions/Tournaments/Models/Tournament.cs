using competitions.Domain.Competitions.Shared.Models;
using competitions.Domain.Models;

namespace competitions.Domain.Competitions.Tournaments.Models;

public class Tournament : Competition
{
    public override CompetitionType Type => CompetitionType.Tournament;
    public TournamentFormat Format { get; set; }
    public SeedingType SeedingType { get; set; } = SeedingType.Standard;
    public bool BracketReset { get; set; }
    public RegistrationType RegistrationType { get; set; } = RegistrationType.Open;
    public string? RegistrationPassword { get; set; }
    public List<Stage> Stages { get; set; } = new();
}