namespace competitions.Domain.Competitions.Tournaments.Models;

public class BracketMatch
{
    public string Id { get; set; }

    public Matches.Models.Match? Match { get; set; }
    
    public string? HomeTeamId { get; set; }
    public string? AwayTeamId { get; set; }
    
    public string? WinnerTeamId { get; set; }
    public string? LoserTeamId { get; set; }
    
    public int? ScoreHomeTeam { get; set; }
    public int? ScoreAwayTeam { get; set; }
}