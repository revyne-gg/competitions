using competitions.Domain.Models;

namespace competitions.Infrastructure.Entities;

public class CompetitionEntity
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Discriminator { get; set; }
    public CompetitionType Type { get; set; }
    public DateTime CreatedAt { get; set; }
    public string TenantId { get; set; }
    public string Game { get; set; }
    public int BestOf { get; set; } = 1;
    public List<string>? MapPool { get; set; }
}