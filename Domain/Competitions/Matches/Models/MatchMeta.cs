namespace competitions.Domain.Competitions.Matches.Models;

public class MatchMeta
{
    public DateTime? ProposedDate { get; set; }
    public string? ProposerTeamId { get; set; }
    public string? ProposedByUserId { get; set; }
}