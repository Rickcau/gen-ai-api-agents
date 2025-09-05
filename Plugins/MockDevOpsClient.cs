using Plugin.Interfaces;
using System.Text.Json;

namespace gen_ai_api_agents.Plugins;

/// <summary>
/// Mock implementation of DevOps client returning predefined data
/// </summary>
public class MockDevOpsClient : Plugin.Interfaces.IDevOpsClient
{
    private readonly Dictionary<string, Plugin.Interfaces.WiqlResult> _mockAssigneeWorkItems = new();
    private readonly Dictionary<string, Plugin.Interfaces.WiqlResult> _mockAssigneeBugs = new();
    private readonly Dictionary<int, Plugin.Interfaces.WiqlResult> _mockRecentWorkItems = new();
    private readonly Dictionary<int, string> _mockWorkItemDetails = new();

    public MockDevOpsClient()
    {
        InitializeMockData();
    }

    private void InitializeMockData()
    {
        // Mock data for work items by assignee
        _mockAssigneeWorkItems["John Doe"] = new Plugin.Interfaces.WiqlResult
        {
            workItems = new[]
            {
                new Plugin.Interfaces.WorkItemReference { id = 1001, url = "https://example.com/1001" },
                new Plugin.Interfaces.WorkItemReference { id = 1002, url = "https://example.com/1002" },
                new Plugin.Interfaces.WorkItemReference { id = 1003, url = "https://example.com/1003" },
            }
        };

        _mockAssigneeWorkItems["Jane Smith"] = new Plugin.Interfaces.WiqlResult
        {
            workItems = new[]
            {
                new Plugin.Interfaces.WorkItemReference { id = 2001, url = "https://example.com/2001" },
                new Plugin.Interfaces.WorkItemReference { id = 2002, url = "https://example.com/2002" },
                new Plugin.Interfaces.WorkItemReference { id = 2003, url = "https://example.com/2003" },
                new Plugin.Interfaces.WorkItemReference { id = 2004, url = "https://example.com/2004" },
                new Plugin.Interfaces.WorkItemReference { id = 2005, url = "https://example.com/2005" },
            }
        };

        // Mock data for bugs by assignee
        _mockAssigneeBugs["John Doe"] = new Plugin.Interfaces.WiqlResult
        {
            workItems = new[]
            {
                new Plugin.Interfaces.WorkItemReference { id = 1001, url = "https://example.com/1001" },
            }
        };

        _mockAssigneeBugs["Jane Smith"] = new Plugin.Interfaces.WiqlResult
        {
            workItems = new[]
            {
                new Plugin.Interfaces.WorkItemReference { id = 2001, url = "https://example.com/2001" },
                new Plugin.Interfaces.WorkItemReference { id = 2003, url = "https://example.com/2003" },
            }
        };

        // Mock data for recent work items
        _mockRecentWorkItems[7] = new Plugin.Interfaces.WiqlResult
        {
            workItems = new[]
            {
                new Plugin.Interfaces.WorkItemReference { id = 3001, url = "https://example.com/3001" },
                new Plugin.Interfaces.WorkItemReference { id = 3002, url = "https://example.com/3002" },
                new Plugin.Interfaces.WorkItemReference { id = 3003, url = "https://example.com/3003" },
                new Plugin.Interfaces.WorkItemReference { id = 3004, url = "https://example.com/3004" },
                new Plugin.Interfaces.WorkItemReference { id = 3005, url = "https://example.com/3005" },
            }
        };

        _mockRecentWorkItems[30] = new Plugin.Interfaces.WiqlResult
        {
            workItems = new[]
            {
                new Plugin.Interfaces.WorkItemReference { id = 3001, url = "https://example.com/3001" },
                new Plugin.Interfaces.WorkItemReference { id = 3002, url = "https://example.com/3002" },
                new Plugin.Interfaces.WorkItemReference { id = 3003, url = "https://example.com/3003" },
                new Plugin.Interfaces.WorkItemReference { id = 3004, url = "https://example.com/3004" },
                new Plugin.Interfaces.WorkItemReference { id = 3005, url = "https://example.com/3005" },
                new Plugin.Interfaces.WorkItemReference { id = 3006, url = "https://example.com/3006" },
                new Plugin.Interfaces.WorkItemReference { id = 3007, url = "https://example.com/3007" },
                new Plugin.Interfaces.WorkItemReference { id = 3008, url = "https://example.com/3008" },
            }
        };

        // Mock data for work item details
        _mockWorkItemDetails[1001] = @"{
            ""id"": 1001,
            ""fields"": {
                ""System.Title"": ""Fix critical login bug"",
                ""System.WorkItemType"": ""Bug"",
                ""System.State"": ""Active"",
                ""System.AssignedTo"": {
                    ""displayName"": ""John Doe"",
                    ""uniqueName"": ""john.doe@example.com""
                }
            }
        }";

        _mockWorkItemDetails[2001] = @"{
            ""id"": 2001,
            ""fields"": {
                ""System.Title"": ""Implement new authentication flow"",
                ""System.WorkItemType"": ""Bug"",
                ""System.State"": ""Active"",
                ""System.AssignedTo"": {
                    ""displayName"": ""Jane Smith"",
                    ""uniqueName"": ""jane.smith@example.com""
                }
            }
        }";

        _mockWorkItemDetails[3001] = @"{
            ""id"": 3001,
            ""fields"": {
                ""System.Title"": ""Add user profile feature"",
                ""System.WorkItemType"": ""User Story"",
                ""System.State"": ""New"",
                ""System.AssignedTo"": {
                    ""displayName"": ""John Doe"",
                    ""uniqueName"": ""john.doe@example.com""
                }
            }
        }";
    }

    public Task<Plugin.Interfaces.WiqlResult?> GetWorkItemsByAssigneeAsync(string assigneeName)
    {
        if (_mockAssigneeWorkItems.TryGetValue(assigneeName, out var result))
        {
            return Task.FromResult<Plugin.Interfaces.WiqlResult?>(result);
        }

        return Task.FromResult<Plugin.Interfaces.WiqlResult?>(new Plugin.Interfaces.WiqlResult { workItems = Array.Empty<Plugin.Interfaces.WorkItemReference>() });
    }

    public Task<Plugin.Interfaces.WiqlResult?> GetBugsByAssigneeAsync(string assigneeName)
    {
        if (_mockAssigneeBugs.TryGetValue(assigneeName, out var result))
        {
            return Task.FromResult<Plugin.Interfaces.WiqlResult?>(result);
        }

        return Task.FromResult<Plugin.Interfaces.WiqlResult?>(new Plugin.Interfaces.WiqlResult { workItems = Array.Empty<Plugin.Interfaces.WorkItemReference>() });
    }

    public Task<Plugin.Interfaces.WiqlResult?> GetRecentWorkItemsAsync(int days)
    {
        // Try to get exact day match, or default to 7 days if not found
        if (!_mockRecentWorkItems.TryGetValue(days, out var result))
        {
            result = _mockRecentWorkItems.TryGetValue(7, out var defaultResult)
                ? defaultResult
                : new Plugin.Interfaces.WiqlResult { workItems = Array.Empty<Plugin.Interfaces.WorkItemReference>() };
        }

        return Task.FromResult<Plugin.Interfaces.WiqlResult?>(result);
    }

    public Task<JsonElement> GetWorkItemDetailsAsync(int workItemId)
    {
        if (_mockWorkItemDetails.TryGetValue(workItemId, out var json))
        {
            return Task.FromResult(JsonSerializer.Deserialize<JsonElement>(json));
        }

        // If ID not found, return a generic work item
        var genericWorkItem = @"{
            ""id"": " + workItemId + @",
            ""fields"": {
                ""System.Title"": ""Sample work item"",
                ""System.WorkItemType"": ""Task"",
                ""System.State"": ""New"",
                ""System.AssignedTo"": {
                    ""displayName"": ""Unassigned"",
                    ""uniqueName"": """"
                }
            }
        }";

        return Task.FromResult(JsonSerializer.Deserialize<JsonElement>(genericWorkItem));
    }
}