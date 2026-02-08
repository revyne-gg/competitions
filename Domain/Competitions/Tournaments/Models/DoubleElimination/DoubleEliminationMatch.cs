namespace competitions.Domain.Competitions.Tournaments.Models.DoubleElimination;

public class DoubleEliminationMatch : BracketMatch
{
    public DoubleEliminationMatch? WinnersMatch { get; set; }
    public BracketSlot? WinnersMatchSlot { get; set; }
    
    public DoubleEliminationMatch? LosersMatch { get; set; }
    public BracketSlot? LosersMatchSlot { get; set; }
    
    public bool IsGrandFinal { get; set; }
    public bool IsBracketReset { get; set; }
}