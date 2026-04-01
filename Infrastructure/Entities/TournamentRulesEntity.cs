using System.ComponentModel.DataAnnotations;

namespace competitions.Infrastructure.Entities;

public class TournamentRulesEntity
{
    [Key]
    public string TournamentId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string TenantId { get; set; }
    public DateTime UpdatedAt { get; set; }
}
