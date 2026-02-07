namespace competitions.Application.DTO;

public record LeagueDTO(
    string Id,
    string Name,
    string Slug,
    string Description,
    DateTime CreatedAt
);