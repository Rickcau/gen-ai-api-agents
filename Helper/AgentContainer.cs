using Helper.ApprovalTermStrategy;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using gen_ai_api_agents.Plugins;
using gen_ai_api_agents.Models;
using System.Configuration;
using System.Linq;

namespace Helper.AgentContainer
{
    /// <summary>
    /// Manages a group chat between an Assistant and a Specialist agent for handling IT operations.
    /// The Assistant agent evaluates requests and routes them to the Specialist, which handles runbook operations.
    /// </summary>
    /// <example>
    /// Usage:
    /// <code>
    /// var container = new AgentContainer(chatCompletionService, kernel);
    /// var groupChat = await container.CreateAgentGroupChatAsync();
    /// var chatHistory = await container.ExecuteGroupChatAsync(groupChat, "List all VMs");
    /// </code>
    /// </example>

    public class AgentContainer
    {
        private readonly Kernel _kernel;
        //private readonly IAzureDbService _azureDbService;
        //private readonly AISearchPlugin _aiSearchPlugin;
        //private readonly CompanyLookupPlugin _companyLookupPlugin;
        private readonly IConfiguration  _configuration;
        // Remove section
        //private readonly RunbookPlugin _runbookPlugin;
        //private readonly WeatherPlugin _weatherPlugin;
        //private readonly GitHubWorkflowPlugin _gitHubWorkflowPlugin;
        // end remove section
        //private readonly CompanyPlugin _companyPlugin;
        //private readonly DBQueryPlugin _dbQueryPlugin;
        private const string CoordinatorAgentName = "Coordinator";
        private const string DevOpsAgentName = "DevOps";
        private const string ServiceNowAgentName = "ServiceNow";

        /// <summary>
        /// Initializes a new instance of the AgentContainer class.
        /// </summary>        /// <param name="chatCompletionService">The chat completion service for agent communication.</param>
        /// <param name="kernel">The semantic kernel instance for agent operations.</param>
        /// <param name="configuration">The AI search plugin for the specialist agent.</param>
       
        public AgentContainer(
            Kernel kernel,
            IConfiguration configuration)
        {
            _kernel = kernel;
            _configuration = configuration;
        }

        private ChatCompletionAgent CreateCoordinatorAgent()
        {
            var agent = new ChatCompletionAgent
            {
                Name = CoordinatorAgentName,
                Instructions = """
                You are an intelligent multi-agent coordinator that manages specialized AI agents for IT operations.
                You have access to multiple agents and can route queries appropriately:

                1. DevOps Agent - For Azure DevOps queries (work items, bugs, projects)
                2. ServiceNow Agent - For ServiceNow/ITSM queries (incidents, change requests, tickets)

                When users ask questions:
                - Analyze the question to determine if it relates to Azure DevOps or ServiceNow if not explain you can only help with these topics.
                - Route single-system questions to the appropriate specialist agent
                - Use the DevOps Agent for Azure DevOps related queries
                - Use the ServiceNow Agent for ITSM related queries
                - Provide context about which agents you're consulting
                - Synthesize information from multiple agents when needed

                When routing to the DevOps agent, end your response with: "ROUTE_TO_DEVOPS_AGENT"
                When routing to the ServiceNow agent, end your response with: "ROUTE_TO_SERVICENOW_AGENT"
                If you need to route to both agents, end your response with: "ROUTE_TO_BOTH" 
                If you are unable to determine the correct agent, ask clarifying questions.

                Be conversational and explain your multi-agent approach to users.
                """,
                Kernel = _kernel.Clone(),
                Arguments = new KernelArguments(
                    new OpenAIPromptExecutionSettings
                    {
                        ServiceId = "azure-openai",
                        ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
                    }
                )
            };
            // Add plugins to the specialist agent
            // agent.Kernel.Plugins.Add(KernelPluginFactory.CreateFromObject(_aiSearchPlugin));

            return agent;
        }

        // Modify only the CreateDevOpsAgent method to add support for mock mode
        private ChatCompletionAgent CreateDevOpsAgent()
        {
            DevOpsPlugin devOpsPlugin;

            // Check if we should use mock mode
            var useMockDevOps = _configuration.GetValue<bool>("AzureDevOps:UseMock");
            
            if (useMockDevOps)
            {
                // Use the mock plugin constructor
                devOpsPlugin = new DevOpsPlugin(useMock: true);
                Console.WriteLine("Using DevOps plugin in MOCK MODE");
            }
            else
            {
                // Get DevOps plugin variables from configuration
                var devOpsOrgUrl = _configuration["AzureDevOps:OrganizationUrl"] ?? throw new ArgumentNullException("AzureDevOps:OrganizationUrl");
                var devOpsProject = _configuration["AzureDevOps:ProjectName"] ?? throw new ArgumentNullException("AzureDevOps:ProjectName");
                var devOpsToken = _configuration["AzureDevOps:PersonalAccessToken"] ?? throw new ArgumentNullException("AzureDevOps:PersonalAccessToken");

                // Use the real plugin constructor with API connection
                devOpsPlugin = new DevOpsPlugin(devOpsOrgUrl, devOpsProject, devOpsToken);
                Console.WriteLine("Using DevOps plugin with REAL API connection");
            }

            var agent = new ChatCompletionAgent
            {
                Name = DevOpsAgentName,
                Instructions = """
                    You are a DevOps Agent which specialized in answering questions related to Azure DevOps:
                    1. Determine the intent of the question, is it related to Azure DevOps?
                    2. If the question is about Azure DevOps, proceed with the following steps:
                       - Analyze the question to identify specific Azure DevOps concepts (e.g., work items, bugs, projects).
                       - If the question is not clear, ask clarifying questions to gather more context.
                       - Use the Azure DevOps API to retrieve relevant information and provide a comprehensive answer.
                    3. If the question is not related to Azure DevOps, explain that you can only help with Azure DevOps related questions.
                       - End with ""OPERATION_COMPLETE""
                """,
                Kernel = _kernel.Clone(),
                Arguments = new KernelArguments(
                    new OpenAIPromptExecutionSettings
                    {
                        ServiceId = "azure-openai",
                        ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
                    }
                )
            };
            
            // Add plugin to the DevOpsAgent agent
            agent.Kernel.Plugins.Add(KernelPluginFactory.CreateFromObject(devOpsPlugin));
            return agent;
        }

        private ChatCompletionAgent CreateServiceNowAgent()
        {
            ServiceNowPlugin serviceNowPlugin;

            // Check if we should use mock mode
            var useMockServiceNow = _configuration.GetValue<bool>("ServiceNow:UseMock");
            
            if (useMockServiceNow)
            {
                // Use the mock plugin constructor
                serviceNowPlugin = new ServiceNowPlugin(useMock: true);
                Console.WriteLine("Using ServiceNow plugin in MOCK MODE");
            }
            else
            {
                // Get ServiceNow plugin variables from configuration
                var serviceNowUrl = _configuration["ServiceNow:InstanceUrl"] ?? throw new ArgumentNullException("ServiceNow:InstanceUrl");
                var serviceNowUser = _configuration["ServiceNow:Username"] ?? throw new ArgumentNullException("ServiceNow:Username");
                var serviceNowPass = _configuration["ServiceNow:Password"] ?? throw new ArgumentNullException("ServiceNow:Password");

                // Use the real plugin constructor
                serviceNowPlugin = new ServiceNowPlugin(serviceNowUrl, serviceNowUser, serviceNowPass);
                Console.WriteLine("Using ServiceNow plugin with REAL API connection");
            }

            var agent = new ChatCompletionAgent
            {
                Name = ServiceNowAgentName,
                Instructions = """
                    You are a ServiceNow Agent which specialized in answering questions related to ServiceNow/ITSM:
                    1. Determine the intent of the question, is it related to ServiceNow/ITSM?
                    2. If the question is about ServiceNow/ITSM, proceed with the following steps:
                       - Analyze the question to identify specific ServiceNow concepts (e.g., incidents, change requests, tickets).
                       - If the question is not clear, ask clarifying questions to gather more context.
                       - Use the ServiceNow API to retrieve relevant information and provide a comprehensive answer.
                    3. If the question is not related to ServiceNow/ITSM, explain that you can only help with ServiceNow/ITSM related questions.
                       - End with "OPERATION_COMPLETE"
                """,
                Kernel = _kernel.Clone(),
                Arguments = new KernelArguments(
                    new OpenAIPromptExecutionSettings
                    {
                        ServiceId = "azure-openai",
                        ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
                    }
                )
            };
            
            // Add plugin to the ServiceNow agent
            agent.Kernel.Plugins.Add(KernelPluginFactory.CreateFromObject(serviceNowPlugin));
            return agent;
        }

        public ChatCompletionAgent GetCoordinatorAgent()
        {
            return CreateCoordinatorAgent();
        }

        /// <summary>
        /// Gets the Research agent for handling Company, Persona, or People information requests.
        /// </summary>
        /// <returns>A configured ChatCompletionAgent for the Research role.</returns>
        public ChatCompletionAgent GetDevOpsAgent()
        {
            return CreateDevOpsAgent();
        }

        public ChatCompletionAgent GetServiceNowAgent()
        {
            return CreateServiceNowAgent();
        }


        public async Task<ChatProviderResponse> ProcessChatRequestAsync(string userInput, ChatHistory existingChatHistory)
        {
            Console.WriteLine("\n=== AgentContainer Initialized ===");
            Console.WriteLine("\n=== Starting ProcessChatRequestAsync ===");
            Console.WriteLine($"User Input: {userInput}");

            var response = new ChatProviderResponse();
            var chatHistory = new ChatHistory();

            // Add the last 5 messages from existing history for context
            var recentHistory = existingChatHistory.TakeLast(5).ToList();
            Console.WriteLine($"\nAdding {recentHistory.Count} recent messages to chat history");
            foreach (var historicalMessage in recentHistory)
            {
                Console.WriteLine($"Historical Message - Role: {historicalMessage.Role}, Author: {historicalMessage.AuthorName}, Content: {historicalMessage.Content}");
                chatHistory.Add(new ChatMessageContent(
                    historicalMessage.Role,
                    historicalMessage.Content,
                    historicalMessage.AuthorName
                ));
            }

            // Add the current user input
            chatHistory.AddUserMessage(userInput);

            var coordinatorAgent = GetCoordinatorAgent();
            var devOpsAgent = GetDevOpsAgent();
            var serviceNowAgent = GetServiceNowAgent();

            // First, let the Coordinator process the request
            Console.WriteLine("\n=== Coordinator Processing Request ===");
            var coordinatorResponses = new List<string>();
            bool isRouteToDevOpsAgent = false;
            bool isRouteToServiceNowAgent = false;
            bool isRouteToBothAgents = false;

            try
            {

                await foreach (ChatMessageContent coordinatorResponse in coordinatorAgent.InvokeAsync(chatHistory))
                {
                    var originalContent = coordinatorResponse.Content!.Trim();
                    Console.WriteLine($"Coordinator Raw Response: {originalContent}");
                    isRouteToDevOpsAgent = originalContent.EndsWith("ROUTE_TO_DEVOPS_AGENT");
                    isRouteToServiceNowAgent = originalContent.EndsWith("ROUTE_TO_SERVICENOW_AGENT");

                    var displayContent = originalContent
                        .Replace("ROUTE_TO_DEVOPS_AGENT", "")
                        .Replace("ROUTE_TO_SERVICENOW_AGENT", "")
                        .Trim();

                    if (!string.IsNullOrWhiteSpace(displayContent))
                    {
                        Console.WriteLine($"Coordinator Agent Processed Response: {displayContent}");
                        coordinatorResponses.Add(displayContent);
                        chatHistory.AddAssistantMessage(displayContent);
                    }
                }

                response.CoordinatorResponse = string.Join("\n", coordinatorResponses);
                Console.WriteLine($"\nFinal Coordinator Response: {response.CoordinatorResponse}");
                Console.WriteLine($"Route to DevOps Agent: {isRouteToDevOpsAgent}");
                Console.WriteLine($"Route to ServiceNow Agent: {isRouteToServiceNowAgent}");

                if (isRouteToDevOpsAgent)
                {
                    Console.WriteLine("\n=== DevOps Agent Processing ===");
                    var devOpsAgentResponses = new List<string>();
                    await foreach (ChatMessageContent devOpsResponse in devOpsAgent.InvokeAsync(chatHistory))
                    {
                        var originalContent = devOpsResponse.Content!.Trim();
                        Console.WriteLine($"DevOps Agent Raw Response: {originalContent}");
                        var displayContent = originalContent
                            .Replace("OPERATION_COMPLETE", "")
                            .Trim();

                        if (!string.IsNullOrWhiteSpace(displayContent))
                        {
                            Console.WriteLine($"DevOps Agent Processed Response: {displayContent}");
                            devOpsAgentResponses.Add(displayContent);
                            chatHistory.AddAssistantMessage(displayContent);
                        }
                    }

                    response.DevOpsResponse = string.Join("\n", devOpsAgentResponses);
                    Console.WriteLine($"\nFinal DevOps Response: {response.DevOpsResponse}");
                }

                if (isRouteToServiceNowAgent)
                {
                    Console.WriteLine("\n=== ServiceNow Agent Processing ===");
                    var serviceNowAgentResponses = new List<string>();
                    await foreach (ChatMessageContent serviceNowResponse in serviceNowAgent.InvokeAsync(chatHistory))
                    {
                        var originalContent = serviceNowResponse.Content!.Trim();
                        Console.WriteLine($"ServiceNow Agent Raw Response: {originalContent}");
                        var displayContent = originalContent
                            .Replace("OPERATION_COMPLETE", "")
                            .Trim();

                        if (!string.IsNullOrWhiteSpace(displayContent))
                        {
                            Console.WriteLine($"ServiceNow Agent Processed Response: {displayContent}");
                            serviceNowAgentResponses.Add(displayContent);
                            chatHistory.AddAssistantMessage(displayContent);
                        }
                    }

                    response.ServiceNowResponse = string.Join("\n", serviceNowAgentResponses);
                    Console.WriteLine($"\nFinal ServiceNow Response: {response.ServiceNowResponse}");
                }

                if (isRouteToBothAgents)
                {
                    Console.WriteLine("\n=== Both Agents were requested, but this path is not yet implemented ===");
                    // Future implementation for handling both agents
                }
                response.ChatResponse = response.CoordinatorResponse;
                Console.WriteLine("\n=== ProcessChatRequestAsync Complete ===\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return response;
        }


        // RDC - Added 6/6/2025 : The Streaming logic will need to be reworked at a later date
        // With the shift and additional items added to the backlog this has to be put on hold for now.
        /// <summary>
        /// Processes a chat request through the agent system with streaming responses.
        /// </summary>
        /// <param name="userInput">The user's current input message.</param>
        /// <param name="existingChatHistory">The existing chat history from previous interactions.</param>
        /// <param name="sessionId">Session ID for tracking the conversation.</param>
        /// <returns>An async enumerable of streaming chat message content.</returns>
        public async IAsyncEnumerable<StreamingChatMessageContent> ProcessChatRequestStreamAsync(
            string userInput, 
            ChatHistory existingChatHistory,
            string sessionId)
        {
            Console.WriteLine("\n=== Starting ProcessChatRequestStreamAsync ===");
            Console.WriteLine($"User Input: {userInput}");

            var chatHistory = new ChatHistory();

            // Add the last 5 messages from existing history for context
            var recentHistory = existingChatHistory.TakeLast(5).ToList();
            Console.WriteLine($"\nAdding {recentHistory.Count} recent messages to chat history");
            foreach (var historicalMessage in recentHistory)
            {
                Console.WriteLine($"Historical Message - Role: {historicalMessage.Role}, Author: {historicalMessage.AuthorName}, Content: {historicalMessage.Content}");
                chatHistory.Add(new ChatMessageContent(
                    historicalMessage.Role,
                    historicalMessage.Content,
                    historicalMessage.AuthorName
                ));
            }

            // Add the current user input
            chatHistory.AddUserMessage(userInput);

            var assistantAgent = GetCoordinatorAgent();

            // Stream the Assistant agent response using Semantic Kernel streaming
            Console.WriteLine("\n=== Assistant Streaming Processing ===");
            
            await foreach (StreamingChatMessageContent streamingResponse in assistantAgent.InvokeStreamingAsync(chatHistory))
            {
                Console.WriteLine($"Assistant Streaming Chunk: {streamingResponse.Content}");
                yield return streamingResponse;
            }

            Console.WriteLine("\n=== ProcessChatRequestStreamAsync Complete ===\n");
        }
    }
}