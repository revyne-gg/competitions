using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using competitions.Application;
using competitions.Application.Ports;
using competitions.Domain.Competitions.Leagues.Models;
using competitions.Domain.Models;
using competitions.Shared;

namespace competitions.Infrastructure.Services;

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
    private readonly ILogger<KetoPermissionService> _logger;
    private readonly HttpClient _client;
    private readonly string _ketoUrl;
    private readonly string _ketoAdminUrl;

    public KetoPermissionService(ILogger<KetoPermissionService> logger, string ketoUrl, string ketoAdminUrl)
    {
        _logger = logger;
        _ketoUrl = ketoUrl;
        _ketoAdminUrl = ketoAdminUrl;
        _client = new HttpClient();
        _client.DefaultRequestHeaders.Accept.Clear();
        _client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json")
        );
    }

    public async Task<Result<Unit, PermissionError>> AddTeamToLeague(string teamId, string leagueId)
    {
        try
        {
            var tuple = new RelationTuple("League", leagueId, "team", teamId);

            var json = JsonSerializer.Serialize(tuple);

            var request = new HttpRequestMessage(HttpMethod.Put, $"{_ketoAdminUrl}/admin/relation-tuples")
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            var response = await _client.SendAsync(request);

            return !response.IsSuccessStatusCode ? 
                PermissionError.InternalError : 
                Unit.Value;
        }
        catch (Exception e)
        {
            return PermissionError.InternalError;
        }
    }

    public async Task<Result<Unit, PermissionError>> RemoveTeamFromLeague(string teamId, string leagueId)
    {
        try
        {
            var query = $"?namespace=League&object={leagueId}&relation=team&subject_id={teamId}";
            var request = new HttpRequestMessage(HttpMethod.Delete, $"{_ketoAdminUrl}/admin/relation-tuples{query}");
            var response = await _client.SendAsync(request);

            return !response.IsSuccessStatusCode
                ? PermissionError.InternalError
                : Unit.Value;
        }
        catch (Exception)
        {
            return PermissionError.InternalError;
        }
    }

    public async Task<Result<Unit, PermissionError>> AddUserToRoster(string userId, string teamId, string leagueId)
    {
        try
        {
            var tuple = new RelationTuple("LeagueRoster", $"{leagueId}:{teamId}", "member", userId);
            var json = JsonSerializer.Serialize(tuple);

            var request = new HttpRequestMessage(HttpMethod.Put, $"{_ketoAdminUrl}/admin/relation-tuples")
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            var response = await _client.SendAsync(request);

            return !response.IsSuccessStatusCode
                ? PermissionError.InternalError
                : Unit.Value;
        }
        catch (Exception)
        {
            return PermissionError.InternalError;
        }
    }

    public async Task<Result<Unit, PermissionError>> RemoveUserFromRoster(string userId, string teamId, string leagueId)
    {
        try
        {
            var query = $"?namespace=LeagueRoster&object={leagueId}:{teamId}&relation=member&subject_id={userId}";
            var request = new HttpRequestMessage(HttpMethod.Delete, $"{_ketoAdminUrl}/admin/relation-tuples{query}");
            var response = await _client.SendAsync(request);

            return !response.IsSuccessStatusCode
                ? PermissionError.InternalError
                : Unit.Value;
        }
        catch (Exception)
        {
            return PermissionError.InternalError;
        }
    }

    public async Task<Result<RealmMemberRole, PermissionError>> GetRoleForUserInRealm(string userId, string realmId)
    {
        try
        {
            var query = $"?namespace=Realm&object={realmId}&subject_id={userId}";
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_ketoUrl}/relation-tuples{query}");
            var response = await _client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                return PermissionError.InternalError;
            }

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<RelationTupleResponse>(content);

            if (result is null || result.RelationTuples.Count == 0)
            {
                return RealmMemberRole.None;
            }

            var relation = result.RelationTuples[0].Relation;
            return RealmMemberRoleExtensions.FromString(relation);
        }
        catch (Exception)
        {
            return PermissionError.InternalError;
        }
    }

    public async Task<Result<OrganiserMemberRole, PermissionError>> GetRoleForUserInOrganiser(string userId, string organiserId)
    {
        try
        {
            var query = $"?namespace=Organiser&object={organiserId}&subject_id={userId}";
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_ketoUrl}/relation-tuples{query}");
            var response = await _client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                return PermissionError.InternalError;
            }

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<RelationTupleResponse>(content);

            if (result is null || result.RelationTuples.Count == 0)
            {
                return OrganiserMemberRole.None;
            }

            var relation = result.RelationTuples[0].Relation;
            return OrganiserMemberRoleExtensions.FromString(relation);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting role for user in organiser");
            return PermissionError.InternalError;
        }
    }
}