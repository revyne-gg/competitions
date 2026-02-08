namespace competitions.Domain.Competitions.Tournaments.Models.SingleElimination;

public class SingleEliminationMatch : BracketMatch
{
    public SingleEliminationMatch? NextMatch { get; set; }
    public BracketSlot? NextMatchSlot { get; set; }
}