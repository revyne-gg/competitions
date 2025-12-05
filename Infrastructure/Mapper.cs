using leagues.Domain.Models;
using leagues.Infrastructure.Entities;

namespace leagues.Infrastructure;

internal static class Mapper
{
    internal static League ToDomain(this LeagueEntity entity)
    {
        var league = (League)Activator.CreateInstance(
            typeof(League),
            nonPublic: true
        )!;
        
        league.Id = entity.Id;
        league.Name = entity.Name;
        league.Description = entity.Description;
        league.CreatedAt = entity.CreatedAt;
        league.OrganiserId = entity.OrganiserId;
        league.OrganiserSlug = entity.OrganiserSlug;
        league.RealmId = entity.RealmId;
        league.RealmSlug = entity.RealmSlug;
        league.TenantId = entity.TenantId;
        league.DeletedAt = entity.DeletedAt;
        league.DeletedBy = entity.DeletedBy;
        
        return league;
    }
}