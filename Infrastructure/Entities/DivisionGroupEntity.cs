namespace competitions.Infrastructure.Entities;

public class DivisionGroupEntity
{
    public string Id { get; set; }
    public string DivisionId { get; set; }
    public DivisionEntity Division { get; set; }
    public string Name { get; set; }
    public string Slug { get; set; }
    public int Order { get; set; }
    public string TenantId { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<DivisionGroupTeamEntity> Teams { get; set; } = new();
}