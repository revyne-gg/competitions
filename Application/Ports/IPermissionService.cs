using leagues.Domain.Models;
using leagues.Shared;

namespace leagues.Application.Ports;

public interface IPermissionService
{
    Task<Result<Unit, PermissionError>> AddTeamToLeague(string teamId, string leagueId);
    Task<Result<Unit, PermissionError>> RemoveTeamFromLeague(string teamId, string leagueId);
    Task<Result<Unit, PermissionError>> AddUserToRoster(string userId, string teamId, string leagueId);
    Task<Result<Unit, PermissionError>> RemoveUserFromRoster(string userId, string teamId, string leagueId);
    Task<Result<RealmMemberRole, PermissionError>> GetRoleForUserInRealm(string userId, string realmId);
    Task<Result<OrganiserMemberRole, PermissionError>> GetRoleForUserInOrganiser(string userId, string organiserId);
}