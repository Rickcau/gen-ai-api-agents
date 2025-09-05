using Microsoft.SemanticKernel;
using Plugin.Interfaces;
using System.ComponentModel;
using System.Text.Json;

namespace gen_ai_api_agents.Plugins;

/// <summary>
/// Semantic Kernel plugin for Azure DevOps integration using REST API
/// Provides functions to query work items and project information
/// </summary>
public class DevOpsPlugin
{
    private readonly Plugin.Interfaces.IDevOpsClient _devOpsClient;
    private readonly bool _isMockMode;

    /// <summary>
    /// Creates a DevOpsPlugin with real Azure DevOps connection
    /// </summary>
    public DevOpsPlugin(string organizationUrl, string projectName, string personalAccessToken)
    {
        _devOpsClient = new DevOpsApiClient(organizationUrl, projectName, personalAccessToken);
        _isMockMode = false;
    }

    /// <summary>
    /// Creates a DevOpsPlugin with mock data (no real API calls)
    /// </summary>
    /// <param name="useMock">Should always be true for this constructor</param>
    public DevOpsPlugin(bool useMock = true)
    {
        if (!useMock)
        {
            throw new ArgumentException("This constructor is for mock mode only. Set useMock to true or use the other constructor for real API access.");
        }
        
        _devOpsClient = new MockDevOpsClient();
        _isMockMode = true;
    }

    [KernelFunction]
    [Description("Get count of work items assigned to a specific person")]
    public async Task<string> GetWorkItemCount(
        [Description("Name of the person (assignee)")] string assigneeName)
    {
        try
        {
            var result = await _devOpsClient.GetWorkItemsByAssigneeAsync(assigneeName);
            var count = result?.workItems?.Length ?? 0;

            var mockInfo = _isMockMode ? " [MOCK DATA]" : "";
            return $"Found {count} active work items assigned to {assigneeName}{mockInfo}";
        }
        catch (Exception ex)
        {
            return $"Error querying work items: {ex.Message}";
        }
    }

    [KernelFunction]
    [Description("Get count of bugs assigned to a specific person")]
    public async Task<string> GetBugCount(
        [Description("Name of the person (assignee)")] string assigneeName)
    {
        try
        {
            var result = await _devOpsClient.GetBugsByAssigneeAsync(assigneeName);
            var count = result?.workItems?.Length ?? 0;

            var mockInfo = _isMockMode ? " [MOCK DATA]" : "";
            return $"Found {count} active bugs assigned to {assigneeName}{mockInfo}";
        }
        catch (Exception ex)
        {
            return $"Error querying bugs: {ex.Message}";
        }
    }

    [KernelFunction]
    [Description("Get recent work items created in the project")]
    public async Task<string> GetRecentWorkItems(
        [Description("Number of days to look back, default is 7")] int days = 7)
    {
        try
        {
            var result = await _devOpsClient.GetRecentWorkItemsAsync(days);
            
            if (result?.workItems == null || result.workItems.Length == 0)
            {
                return $"No work items found in the last {days} days";
            }

            var mockInfo = _isMockMode ? " [MOCK DATA]" : "";
            var summary = $"Recent work items (last {days} days){mockInfo}:\n";
            foreach (var workItem in result.workItems.Take(10))
            {
                summary += $"• Work Item #{workItem.id}: Created in the last {days} days\n";
            }

            return summary;
        }
        catch (Exception ex)
        {
            return $"Error querying recent work items: {ex.Message}";
        }
    }

    [KernelFunction]
    [Description("Get work item details by ID")]
    public async Task<string> GetWorkItemDetails(
        [Description("Work item ID number")] int workItemId)
    {
        try
        {
            var workItem = await _devOpsClient.GetWorkItemDetailsAsync(workItemId);
            
            var mockInfo = _isMockMode ? " [MOCK DATA]" : "";
            var details = $"Work Item #{workItemId}{mockInfo}:\n";
            
            if (workItem.TryGetProperty("fields", out var fields))
            {
                if (fields.TryGetProperty("System.Title", out var title))
                    details += $"• Title: {title.GetString()}\n";
                
                if (fields.TryGetProperty("System.WorkItemType", out var type))
                    details += $"• Type: {type.GetString()}\n";
                
                if (fields.TryGetProperty("System.State", out var state))
                    details += $"• State: {state.GetString()}\n";
                
                if (fields.TryGetProperty("System.AssignedTo", out var assignedTo))
                {
                    if (assignedTo.TryGetProperty("displayName", out var displayName))
                        details += $"• Assigned To: {displayName.GetString()}\n";
                }
                else
                {
                    details += "• Assigned To: Unassigned\n";
                }
            }

            return details;
        }
        catch (Exception ex)
        {
            return $"Error getting work item details: {ex.Message}";
        }
    }
}