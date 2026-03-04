namespace competitions.Infrastructure.Entities;

public class DivisionGroupTeamEntity
{
    public long Id { get; set; }
    public string GroupId { get; set; }
    public DivisionGroupEntity Group { get; set; }
    public string TeamId { get; set; }
}
