using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text;
using System.Text.Json;

namespace gen_ai_api_agents.Plugins;

/// <summary>
/// Interface for ServiceNow client operations
/// Defines the contract for both real and mock implementations
/// </summary>
public interface IServiceNowClient
{
    /// <summary>
    /// Query ServiceNow table with the given query parameters
    /// </summary>
    /// <param name="table">The table name to query</param>
    /// <param name="query">The query parameters</param>
    /// <returns>ServiceNow query result</returns>
    Task<ServiceNowResponse?> QueryServiceNowTableAsync(string table, string query);
}

/// <summary>
/// Real implementation of ServiceNow client using REST API
/// </summary>
public class ServiceNowApiClient : IServiceNowClient
{
    private readonly string _instanceUrl;
    private readonly string _username;
    private readonly string _password;
    private readonly HttpClient _httpClient;

    public ServiceNowApiClient(string instanceUrl, string username, string password)
    {
        _instanceUrl = instanceUrl?.TrimEnd('/') ?? throw new ArgumentNullException(nameof(instanceUrl));
        _username = username ?? throw new ArgumentNullException(nameof(username));
        _password = password ?? throw new ArgumentNullException(nameof(password));
        
        _httpClient = new HttpClient();
        var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_username}:{_password}"));
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
        _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<ServiceNowResponse?> QueryServiceNowTableAsync(string table, string query)
    {
        var url = $"{_instanceUrl}/api/now/table/{table}?{query}&sysparm_limit=100";
        
        var response = await _httpClient.GetAsync(url);
        
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"ServiceNow query failed: {response.StatusCode}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ServiceNowResponse>(responseContent);
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}

/// <summary>
/// Mock implementation of ServiceNow client returning predefined data
/// </summary>
public class MockServiceNowClient : IServiceNowClient
{
    private readonly Dictionary<string, Dictionary<string, string>> _mockIncidentData = new();
    private readonly Dictionary<string, Dictionary<string, string>> _mockChangeRequestData = new();
    
    public MockServiceNowClient()
    {
        InitializeMockData();
    }
    
    private void InitializeMockData()
    {
        // Mock incident data by priority
        _mockIncidentData["priority=1&active=true"] = new Dictionary<string, string>
        {
            ["count"] = "3",
            ["data"] = @"[
                {
                    ""number"": ""INC0001001"",
                    ""short_description"": ""Production database down"",
                    ""priority"": ""1"",
                    ""state"": ""2"",
                    ""category"": ""Database"",
                    ""sys_created_on"": ""2023-06-15 10:30:22"",
                    ""assigned_to"": { ""display_value"": ""John Smith"" }
                },
                {
                    ""number"": ""INC0001002"",
                    ""short_description"": ""Website unavailable"",
                    ""priority"": ""1"",
                    ""state"": ""2"",
                    ""category"": ""Network"",
                    ""sys_created_on"": ""2023-06-15 14:22:10"",
                    ""assigned_to"": { ""display_value"": ""Jane Doe"" }
                },
                {
                    ""number"": ""INC0001003"",
                    ""short_description"": ""Payment processing failing"",
                    ""priority"": ""1"",
                    ""state"": ""1"",
                    ""category"": ""Software"",
                    ""sys_created_on"": ""2023-06-15 16:45:33"",
                    ""assigned_to"": { ""display_value"": ""Bob Johnson"" }
                }
            ]"
        };
        
        _mockIncidentData["priority=2&active=true"] = new Dictionary<string, string>
        {
            ["count"] = "5",
            ["data"] = @"[
                {
                    ""number"": ""INC0002001"",
                    ""short_description"": ""Users cannot access email"",
                    ""priority"": ""2"",
                    ""state"": ""2"",
                    ""category"": ""Email"",
                    ""sys_created_on"": ""2023-06-14 08:12:45"",
                    ""assigned_to"": { ""display_value"": ""Alice Brown"" }
                },
                {
                    ""number"": ""INC0002002"",
                    ""short_description"": ""CRM system slow performance"",
                    ""priority"": ""2"",
                    ""state"": ""2"",
                    ""category"": ""Software"",
                    ""sys_created_on"": ""2023-06-14 09:32:18"",
                    ""assigned_to"": { ""display_value"": ""John Smith"" }
                }
            ]"
        };
        
        _mockIncidentData["priority=3&active=true"] = new Dictionary<string, string>
        {
            ["count"] = "8",
            ["data"] = @"[
                {
                    ""number"": ""INC0003001"",
                    ""short_description"": ""Printer not working"",
                    ""priority"": ""3"",
                    ""state"": ""2"",
                    ""category"": ""Hardware"",
                    ""sys_created_on"": ""2023-06-13 11:22:33"",
                    ""assigned_to"": { ""display_value"": ""Support Team"" }
                }
            ]"
        };
        
        // Mock incident data by assigned person
        _mockIncidentData["assigned_to.name=John Smith&active=true"] = new Dictionary<string, string>
        {
            ["count"] = "2",
            ["data"] = @"[
                {
                    ""number"": ""INC0001001"",
                    ""short_description"": ""Production database down"",
                    ""priority"": ""1"",
                    ""state"": ""2"",
                    ""category"": ""Database"",
                    ""sys_created_on"": ""2023-06-15 10:30:22"",
                    ""assigned_to"": { ""display_value"": ""John Smith"" }
                },
                {
                    ""number"": ""INC0002002"",
                    ""short_description"": ""CRM system slow performance"",
                    ""priority"": ""2"",
                    ""state"": ""2"",
                    ""category"": ""Software"",
                    ""sys_created_on"": ""2023-06-14 09:32:18"",
                    ""assigned_to"": { ""display_value"": ""John Smith"" }
                }
            ]"
        };
        
        _mockIncidentData["assigned_to.name=Jane Doe&active=true"] = new Dictionary<string, string>
        {
            ["count"] = "1",
            ["data"] = @"[
                {
                    ""number"": ""INC0001002"",
                    ""short_description"": ""Website unavailable"",
                    ""priority"": ""1"",
                    ""state"": ""2"",
                    ""category"": ""Network"",
                    ""sys_created_on"": ""2023-06-15 14:22:10"",
                    ""assigned_to"": { ""display_value"": ""Jane Doe"" }
                }
            ]"
        };
        
        // Mock recent incidents
        _mockIncidentData["sys_created_on>=2023-06-10&active=true"] = new Dictionary<string, string>
        {
            ["count"] = "12",
            ["data"] = @"[
                {
                    ""number"": ""INC0001001"",
                    ""short_description"": ""Production database down"",
                    ""priority"": ""1"",
                    ""state"": ""2"",
                    ""category"": ""Database"",
                    ""sys_created_on"": ""2023-06-15 10:30:22"",
                    ""assigned_to"": { ""display_value"": ""John Smith"" }
                },
                {
                    ""number"": ""INC0001002"",
                    ""short_description"": ""Website unavailable"",
                    ""priority"": ""1"",
                    ""state"": ""2"",
                    ""category"": ""Network"",
                    ""sys_created_on"": ""2023-06-15 14:22:10"",
                    ""assigned_to"": { ""display_value"": ""Jane Doe"" }
                },
                {
                    ""number"": ""INC0001003"",
                    ""short_description"": ""Payment processing failing"",
                    ""priority"": ""1"",
                    ""state"": ""1"",
                    ""category"": ""Software"",
                    ""sys_created_on"": ""2023-06-15 16:45:33"",
                    ""assigned_to"": { ""display_value"": ""Bob Johnson"" }
                },
                {
                    ""number"": ""INC0002001"",
                    ""short_description"": ""Users cannot access email"",
                    ""priority"": ""2"",
                    ""state"": ""2"",
                    ""category"": ""Email"",
                    ""sys_created_on"": ""2023-06-14 08:12:45"",
                    ""assigned_to"": { ""display_value"": ""Alice Brown"" }
                },
                {
                    ""number"": ""INC0002002"",
                    ""short_description"": ""CRM system slow performance"",
                    ""priority"": ""2"",
                    ""state"": ""2"",
                    ""category"": ""Software"",
                    ""sys_created_on"": ""2023-06-14 09:32:18"",
                    ""assigned_to"": { ""display_value"": ""John Smith"" }
                }
            ]"
        };
        
        // Mock specific incident
        _mockIncidentData["number=INC0001001"] = new Dictionary<string, string>
        {
            ["count"] = "1",
            ["data"] = @"[
                {
                    ""number"": ""INC0001001"",
                    ""short_description"": ""Production database down"",
                    ""priority"": ""1"",
                    ""state"": ""2"",
                    ""category"": ""Database"",
                    ""sys_created_on"": ""2023-06-15 10:30:22"",
                    ""assigned_to"": { ""display_value"": ""John Smith"" }
                }
            ]"
        };
        
        // Mock change requests by state
        _mockChangeRequestData["state.display_value=New"] = new Dictionary<string, string>
        {
            ["count"] = "3",
            ["data"] = @"[
                {
                    ""number"": ""CHG0001001"",
                    ""short_description"": ""Deploy new application version"",
                    ""state"": { ""display_value"": ""New"" }
                },
                {
                    ""number"": ""CHG0001002"",
                    ""short_description"": ""Update firewall rules"",
                    ""state"": { ""display_value"": ""New"" }
                },
                {
                    ""number"": ""CHG0001003"",
                    ""short_description"": ""Install new server"",
                    ""state"": { ""display_value"": ""New"" }
                }
            ]"
        };
        
        _mockChangeRequestData["state.display_value=Assess"] = new Dictionary<string, string>
        {
            ["count"] = "2",
            ["data"] = @"[
                {
                    ""number"": ""CHG0002001"",
                    ""short_description"": ""Network infrastructure upgrade"",
                    ""state"": { ""display_value"": ""Assess"" }
                },
                {
                    ""number"": ""CHG0002002"",
                    ""short_description"": ""Database migration"",
                    ""state"": { ""display_value"": ""Assess"" }
                }
            ]"
        };
        
        _mockChangeRequestData["state.display_value=Authorize"] = new Dictionary<string, string>
        {
            ["count"] = "1",
            ["data"] = @"[
                {
                    ""number"": ""CHG0003001"",
                    ""short_description"": ""Deploy security patches"",
                    ""state"": { ""display_value"": ""Authorize"" }
                }
            ]"
        };
    }
    
    public Task<ServiceNowResponse?> QueryServiceNowTableAsync(string table, string query)
    {
        Dictionary<string, Dictionary<string, string>> mockData;
        ServiceNowResponse? response = null;
        
        // Select the mock data source based on table
        if (table == "incident")
        {
            mockData = _mockIncidentData;
        }
        else if (table == "change_request")
        {
            mockData = _mockChangeRequestData;
        }
        else
        {
            // Return empty response for unsupported tables
            return Task.FromResult<ServiceNowResponse?>(new ServiceNowResponse 
            { 
                result = Array.Empty<JsonElement>() 
            });
        }
        
        // Try to find exact match for the query
        if (mockData.TryGetValue(query, out var mockResult))
        {
            var jsonData = mockResult["data"];
            response = new ServiceNowResponse
            {
                result = JsonSerializer.Deserialize<JsonElement[]>(jsonData)
            };
        }
        // For recent incidents with a dynamic date, try to find a partial match
        else if (query.Contains("sys_created_on>=") && table == "incident")
        {
            var fallbackKey = _mockIncidentData.Keys.FirstOrDefault(k => k.StartsWith("sys_created_on"));
            if (fallbackKey != null && _mockIncidentData.TryGetValue(fallbackKey, out var fallbackData))
            {
                var jsonData = fallbackData["data"];
                response = new ServiceNowResponse
                {
                    result = JsonSerializer.Deserialize<JsonElement[]>(jsonData)
                };
            }
        }
        // If we couldn't find a match, return empty results
        else
        {
            response = new ServiceNowResponse
            {
                result = Array.Empty<JsonElement>()
            };
        }
        
        return Task.FromResult(response);
    }
}

/// <summary>
/// Semantic Kernel plugin for ServiceNow integration using REST API
/// Provides functions to query incidents, requests, and other ServiceNow data
/// </summary>
public class ServiceNowPlugin
{
    private readonly IServiceNowClient _serviceNowClient;
    private readonly bool _isMockMode;

    /// <summary>
    /// Creates a ServiceNowPlugin with real ServiceNow connection
    /// </summary>
    public ServiceNowPlugin(string instanceUrl, string username, string password)
    {
        _serviceNowClient = new ServiceNowApiClient(instanceUrl, username, password);
        _isMockMode = false;
    }

    /// <summary>
    /// Creates a ServiceNowPlugin with mock data (no real API calls)
    /// </summary>
    /// <param name="useMock">Should always be true for this constructor</param>
    public ServiceNowPlugin(bool useMock = true)
    {
        if (!useMock)
        {
            throw new ArgumentException("This constructor is for mock mode only. Set useMock to true or use the other constructor for real API access.");
        }
        
        _serviceNowClient = new MockServiceNowClient();
        _isMockMode = true;
    }

    [KernelFunction]
    [Description("Get count of incidents with specified priority level")]
    public async Task<string> GetIncidentCount(
        [Description("Priority level (1=Critical, 2=High, 3=Moderate, 4=Low, 5=Planning)")] string priority = "1")
    {
        try
        {
            var query = $"priority={priority}&active=true";
            var result = await _serviceNowClient.QueryServiceNowTableAsync("incident", query);
            
            if (result?.result == null)
            {
                return $"No incidents found with priority {priority}";
            }

            var count = result.result.Length;
            var priorityText = GetPriorityText(priority);
            var mockInfo = _isMockMode ? " [MOCK DATA]" : "";
            
            return $"Found {count} active {priorityText} priority incidents{mockInfo}";
        }
        catch (Exception ex)
        {
            return $"Error querying incidents: {ex.Message}";
        }
    }

    [KernelFunction]
    [Description("Get count of incidents assigned to a specific person")]
    public async Task<string> GetIncidentsAssignedTo(
        [Description("Name or user ID of the assigned person")] string assignedTo)
    {
        try
        {
            var query = $"assigned_to.name={assignedTo}&active=true";
            var result = await _serviceNowClient.QueryServiceNowTableAsync("incident", query);
            
            if (result?.result == null)
            {
                return $"No active incidents found assigned to {assignedTo}";
            }

            var count = result.result.Length;
            var mockInfo = _isMockMode ? " [MOCK DATA]" : "";
            return $"Found {count} active incidents assigned to {assignedTo}{mockInfo}";
        }
        catch (Exception ex)
        {
            return $"Error querying incidents: {ex.Message}";
        }
    }

    [KernelFunction]
    [Description("Get recent incidents created in the last specified days")]
    public async Task<string> GetRecentIncidents(
        [Description("Number of days to look back, default is 7")] int days = 7)
    {
        try
        {
            var sinceDate = DateTime.Now.AddDays(-days).ToString("yyyy-MM-dd");
            var query = $"sys_created_on>={sinceDate}&active=true";
            
            var result = await _serviceNowClient.QueryServiceNowTableAsync("incident", query);
            
            if (result?.result == null || result.result.Length == 0)
            {
                return $"No incidents found in the last {days} days";
            }

            var mockInfo = _isMockMode ? " [MOCK DATA]" : "";
            var summary = $"Recent incidents (last {days} days){mockInfo}:\n";
            foreach (var incident in result.result.Take(10))
            {
                var number = incident.GetProperty("number").GetString();
                var shortDescription = incident.GetProperty("short_description").GetString();
                var priority = incident.GetProperty("priority").GetString();
                var priorityText = GetPriorityText(priority);
                
                summary += $"• {number}: {shortDescription} ({priorityText} priority)\n";
            }

            return summary;
        }
        catch (Exception ex)
        {
            return $"Error querying recent incidents: {ex.Message}";
        }
    }

    [KernelFunction]
    [Description("Get incident details by number")]
    public async Task<string> GetIncidentDetails(
        [Description("Incident number (e.g., INC0000123)")] string incidentNumber)
    {
        try
        {
            var query = $"number={incidentNumber}";
            var result = await _serviceNowClient.QueryServiceNowTableAsync("incident", query);
            
            if (result?.result == null || result.result.Length == 0)
            {
                return $"Incident {incidentNumber} not found";
            }

            var incident = result.result[0];
            var mockInfo = _isMockMode ? " [MOCK DATA]" : "";
            
            var details = $"Incident {incidentNumber}{mockInfo}:\n";
            details += $"• Short Description: {incident.GetProperty("short_description").GetString()}\n";
            details += $"• State: {GetIncidentState(incident.GetProperty("state").GetString())}\n";
            details += $"• Priority: {GetPriorityText(incident.GetProperty("priority").GetString())}\n";
            details += $"• Category: {incident.GetProperty("category").GetString()}\n";
            
            if (incident.TryGetProperty("assigned_to", out var assignedTo) && 
                assignedTo.TryGetProperty("display_value", out var assignedToName))
            {
                details += $"• Assigned To: {assignedToName.GetString()}\n";
            }
            else
            {
                details += "• Assigned To: Unassigned\n";
            }

            details += $"• Created: {incident.GetProperty("sys_created_on").GetString()}\n";

            return details;
        }
        catch (Exception ex)
        {
            return $"Error getting incident details: {ex.Message}";
        }
    }

    [KernelFunction]
    [Description("Get count of change requests with specified state")]
    public async Task<string> GetChangeRequestCount(
        [Description("State of change requests (e.g., 'New', 'Assess', 'Authorize', 'Scheduled', 'Implement')")] string state = "New")
    {
        try
        {
            var query = $"state.display_value={state}";
            var result = await _serviceNowClient.QueryServiceNowTableAsync("change_request", query);
            
            if (result?.result == null)
            {
                return $"No change requests found with state '{state}'";
            }

            var count = result.result.Length;
            var mockInfo = _isMockMode ? " [MOCK DATA]" : "";
            return $"Found {count} change requests in '{state}' state{mockInfo}";
        }
        catch (Exception ex)
        {
            return $"Error querying change requests: {ex.Message}";
        }
    }

    private static string GetPriorityText(string? priority)
    {
        return priority switch
        {
            "1" => "Critical",
            "2" => "High", 
            "3" => "Moderate",
            "4" => "Low",
            "5" => "Planning",
            _ => $"Priority {priority}"
        };
    }

    private static string GetIncidentState(string? state)
    {
        return state switch
        {
            "1" => "New",
            "2" => "In Progress",
            "3" => "On Hold",
            "6" => "Resolved",
            "7" => "Closed",
            "8" => "Canceled",
            _ => $"State {state}"
        };
    }
}

/// <summary>
/// Data class for ServiceNow query results
/// </summary>
public class ServiceNowResponse
{
    public JsonElement[]? result { get; set; }
}