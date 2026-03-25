namespace competitions.Domain.Competitions.Tournaments.Models;

public class Stage
{
    public string Name { get; set; }
    public TournamentFormat Format { get; set; }
    public int Order { get; set; }
    public int Advancing { get; set; }

    // Format-specific configs — only one is populated based on Format
    public SingleEliminationStageConfig? SingleEliminationConfig { get; set; }
    public DoubleEliminationStageConfig? DoubleEliminationConfig { get; set; }
    public SwissStageConfig? SwissConfig { get; set; }
    public RoundRobinStageConfig? RoundRobinConfig { get; set; }
}

public class StageRound
{
    public string Name { get; set; }
    public int BestOf { get; set; }
}

public class SingleEliminationStageConfig
{
    public List<StageRound> Rounds { get; set; } = new();
}

public class DoubleEliminationStageConfig
{
    public bool BracketReset { get; set; }
    public List<StageRound> Rounds { get; set; } = new();
}

public class SwissStageConfig
{
    public string PointsFormula { get; set; }
}

public class RoundRobinStageConfig
{
    public int MaxGroupSize { get; set; }
    public int PointsPerWin { get; set; }
    public int PointsPerDraw { get; set; }
    public int PointsPerLoss { get; set; }
}
