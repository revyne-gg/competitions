using competitions.Domain.Models;

namespace competitions.Infrastructure.Entities;

public class DivisionEntity
{
    public string Id { get; set; }
    public string CompetitionId { get; set; }
    public CompetitionEntity Competition { get; set; }
    public string Name { get; set; }
    public string Slug { get; set; }
    public int Order { get; set; }
    public int BestOf { get; set; }
    public int MaxTeamsPerGroup { get; set; }
    public string TenantId { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<DivisionGroupEntity> Groups { get; set; } = new();
}