using System.Text;
using System.Text.Json;
using Plugin.Interfaces;

namespace gen_ai_api_agents.Plugins;

/// <summary>
/// Real implementation of DevOps client using Azure DevOps REST API
/// </summary>
public class DevOpsApiClient : Plugin.Interfaces.IDevOpsClient
{
    private readonly string _organizationUrl;
    private readonly string _projectName;
    private readonly string _personalAccessToken;
    private readonly HttpClient _httpClient;

    public DevOpsApiClient(string organizationUrl, string projectName, string personalAccessToken)
    {
        _organizationUrl = organizationUrl?.TrimEnd('/') ?? throw new ArgumentNullException(nameof(organizationUrl));
        _projectName = projectName ?? throw new ArgumentNullException(nameof(projectName));
        _personalAccessToken = personalAccessToken ?? throw new ArgumentNullException(nameof(personalAccessToken));
        
        _httpClient = new HttpClient();
        var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{_personalAccessToken}"));
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
        _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<Plugin.Interfaces.WiqlResult?> GetWorkItemsByAssigneeAsync(string assigneeName)
    {
        var wiqlQuery = $@"
            SELECT [System.Id]
            FROM WorkItems 
            WHERE [System.AssignedTo] CONTAINS '{assigneeName}' 
            AND [System.State] <> 'Closed'
            AND [System.State] <> 'Removed'";

        return await ExecuteWiqlQuery(wiqlQuery);
    }

    public async Task<Plugin.Interfaces.WiqlResult?> GetBugsByAssigneeAsync(string assigneeName)
    {
        var wiqlQuery = $@"
            SELECT [System.Id]
            FROM WorkItems 
            WHERE [System.AssignedTo] CONTAINS '{assigneeName}' 
            AND [System.WorkItemType] = 'Bug'
            AND [System.State] <> 'Closed'
            AND [System.State] <> 'Removed'";

        return await ExecuteWiqlQuery(wiqlQuery);
    }

    public async Task<Plugin.Interfaces.WiqlResult?> GetRecentWorkItemsAsync(int days)
    {
        var sinceDate = DateTime.Now.AddDays(-days).ToString("yyyy-MM-dd");

        var wiqlQuery = $@"
            SELECT [System.Id], [System.Title], [System.WorkItemType], [System.CreatedBy]
            FROM WorkItems 
            WHERE [System.CreatedDate] >= '{sinceDate}'
            ORDER BY [System.CreatedDate] DESC";

        return await ExecuteWiqlQuery(wiqlQuery);
    }

    public async Task<JsonElement> GetWorkItemDetailsAsync(int workItemId)
    {
        var url = $"{_organizationUrl}/{_projectName}/_apis/wit/workitems/{workItemId}?api-version=7.1";
        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Error retrieving work item {workItemId}: {response.StatusCode}");
        }

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<JsonElement>(content);
    }

    private async Task<Plugin.Interfaces.WiqlResult?> ExecuteWiqlQuery(string query)
    {
        var url = $"{_organizationUrl}/{_projectName}/_apis/wit/wiql?api-version=7.1";
        
        var wiqlRequest = new { query = query };
        var json = JsonSerializer.Serialize(wiqlRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(url, content);
        
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"WIQL query failed: {response.StatusCode}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<Plugin.Interfaces.WiqlResult>(responseContent);
    }
}