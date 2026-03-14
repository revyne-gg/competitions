using competitions.Domain.Competitions.Shared.Models;
using competitions.Domain.Models;

namespace competitions.Domain.Competitions.Matches.Models;

public class Match
{
    public string Id { get; set; }
    public MatchMeta Meta { get; set; }
    public DateTime? MatchDate { get; set; }
    public DateTime CreatedAt { get; set; }

    public string? HomeTeamId { get; set; }
    public string? AwayTeamId { get; set; }
    public int? ScoreHome { get; set; }
    public int? ScoreAway { get; set; }
    public string? WinnerTeamId { get; set; }
    public string? LoserTeamId { get; set; }

    public int? Round { get; set; }

    public string TenantId { get; set; }
    public string CompetitionId { get; set; }
    public Competition? Competition { get; set; }
}