using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using leagues.Application;
using leagues.Application.Ports;
using leagues.Domain.Models;
using leagues.Shared;

namespace leagues.Infrastructure.Services;

[Serializable]
class RelationTupleResponse
{
    [JsonPropertyName("relation_tuples")]
    public List<RelationTuple> RelationTuples { get; set; } = new();

    [JsonPropertyName("next_page_token")]
    public string NextPageToken { get; set; } = string.Empty;
}

[Serializable]
record RelationTuple(
    [property: JsonPropertyName("namespace")]
    string Namespace, 
    [property: JsonPropertyName("object")]
    string Object, 
    [property: JsonPropertyName("relation")]
    string Relation, 
    [property: JsonPropertyName("subject_id")] string SubjectId
);

public class KetoPermissionService : IPermissionService
{
    private readonly HttpClient _client;
    private readonly string _ketoUrl;
    private readonly string _ketoAdminUrl;

    public KetoPermissionService(string ketoUrl, string ketoAdminUrl)
    {
        _ketoUrl = ketoUrl;
        _ketoAdminUrl = ketoAdminUrl;
        _client = new HttpClient();
        _client.DefaultRequestHeaders.Accept.Clear();
        _client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json")
        );
    }

    public Task<Result<Unit, PermissionError>> AddTeamToLeague(string teamId, string leagueId)
    {
        throw new NotImplementedException();
    }

    public Task<Result<Unit, PermissionError>> RemoveTeamFromLeague(string teamId, string leagueId)
    {
        throw new NotImplementedException();
    }

    public Task<Result<Unit, PermissionError>> AddUserToRoster(string userId, string teamId, string leagueId)
    {
        throw new NotImplementedException();
    }

    public Task<Result<Unit, PermissionError>> RemoveUserFromRoster(string userId, string teamId, string leagueId)
    {
        throw new NotImplementedException();
    }

    public Task<Result<RealmMemberRole, PermissionError>> GetRoleForUserInRealm(string userId, string realmId)
    {
        throw new NotImplementedException();
    }

    public Task<Result<OrganiserMemberRole, PermissionError>> GetRoleForUserInOrganiser(string userId, string organiserId)
    {
        throw new NotImplementedException();
    }
}