using competitions.Application.DTO;
using competitions.Domain.Models;

namespace competitions.Application;

internal static class Mapper
{
    internal static LeagueDTO ToDto(this League league)
    {
        return new LeagueDTO(league.Id, league.Name, league.Slug, league.Description, league.CreatedAt);
    }
}