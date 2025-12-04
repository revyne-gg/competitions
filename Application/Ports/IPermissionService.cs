using leagues.Domain.Models;
using leagues.Shared;

namespace leagues.Application.Ports;

public interface IPermissionService
{
    Task<Result<Unit, PermissionError>> AddTeamToRealm(string teamId, string realmId);
    Task<Result<Unit, PermissionError>> RemoveTeamFromRealm(string teamId, string realmId);
    Task<Result<Unit, PermissionError>> RemoveRealm(string realmId);
    Task<Result<RealmMemberRole, PermissionError>> GetRoleForUserInRealm(string userId, string realmId);
    Task<Result<OrganiserMemberRole, PermissionError>> GetRoleForUserInOrganiser(string userId, string organiserId);
    Task<Result<Unit, PermissionError>> SetUserOrganiserOwner(string organiserId, string ownerUserId);
    Task<Result<Unit, PermissionError>> UnsetUserOrganiserOwner(string organiserId, string ownerUserId);
    Task<Result<Unit, PermissionError>> AddUserToOrganiser(string organiserId, string userId, string role);
    Task<Result<Unit, PermissionError>> RemoveUserFromOrganiser(string organiserId, string userId);
    Task<Result<Unit, PermissionError>> RemoveOrganiser(string organiserId);
}