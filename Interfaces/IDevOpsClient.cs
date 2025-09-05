using gen_ai_api_agents.Plugins;
using System.Text.Json;

namespace Plugin.Interfaces;

/// <summary>
/// Interface for DevOps client operations
/// Defines the contract for both real and mock implementations
/// </summary>
public interface IDevOpsClient
{
    /// <summary>
    /// Get work items assigned to a specific person
    /// </summary>
    /// <param name="assigneeName">Name of the assignee</param>
    /// <returns>Work item query result</returns>
    Task<WiqlResult?> GetWorkItemsByAssigneeAsync(string assigneeName);
    
    /// <summary>
    /// Get bugs assigned to a specific person
    /// </summary>
    /// <param name="assigneeName">Name of the assignee</param>
    /// <returns>Bug query result</returns>
    Task<WiqlResult?> GetBugsByAssigneeAsync(string assigneeName);
    
    /// <summary>
    /// Get work items created recently
    /// </summary>
    /// <param name="days">Number of days to look back</param>
    /// <returns>Recent work items query result</returns>
    Task<WiqlResult?> GetRecentWorkItemsAsync(int days);
    
    /// <summary>
    /// Get details for a specific work item by ID
    /// </summary>
    /// <param name="workItemId">ID of the work item</param>
    /// <returns>Work item details as JsonElement</returns>
    Task<JsonElement> GetWorkItemDetailsAsync(int workItemId);
}

/// <summary>
/// Data class for WIQL query results
/// </summary>
public class WiqlResult
{
    public WorkItemReference[]? workItems { get; set; }
}

/// <summary>
/// Data class for work item references
/// </summary>
public class WorkItemReference
{
    public int id { get; set; }
    public string? url { get; set; }
}