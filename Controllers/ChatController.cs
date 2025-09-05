using gen_ai_api_agents.Models;
using gen_ai_api_agents.Services;
using Helper.AgentContainer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Collections.Concurrent;
using System.Net.Mime;

namespace gen_ai_api_agents.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly ILogger<ChatController> _logger;
        private readonly Kernel _kernel;
        private readonly IChatCompletionService _chat;
        private readonly IChatHistoryManager _chatHistoryManager;
        private readonly IConfiguration _configuration;

        public ChatController(
            ILogger<ChatController> logger,
            IConfiguration configuration,
            Kernel kernel,
            IChatCompletionService chat,
            IChatHistoryManager chathistorymanager
        )
        {
            _kernel = kernel;
            _chat = chat;
            _chatHistoryManager = chathistorymanager;
            _logger = logger;
            _configuration = configuration;
        }

        [HttpPost]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Post([FromBody] ChatProviderRequest chatRequest)
        {
            var response = new ChatProviderResponse();
            ChatHistory? chatHistory = new ChatHistory() ;

            try
            {
                if (string.IsNullOrEmpty(chatRequest.SessionId))
                {
                    // needed for new chats
                    chatRequest.SessionId = Guid.NewGuid().ToString();
                }

                if (string.IsNullOrEmpty(chatRequest.Prompt))
                {
                    _logger.LogWarning("Chat request is missing prompt.");
                    return new BadRequestResult();
                }

                var sessionId = chatRequest.SessionId;
                // var chatHistory = _chatHistoryManager.GetOrCreateChatHistory(sessionId);

                chatHistory.AddUserMessage(chatRequest.Prompt);

                ChatMessageContent? result = null;

                // Create agent container with all necessary dependencies
                var agentContainer = new AgentContainer(_kernel, _configuration);
                response = await agentContainer.ProcessChatRequestAsync(chatRequest.Prompt, chatHistory);

                // Log individual agent responses if available
                if (response.CoordinatorResponse != null)
                {
                    _logger.LogInformation("Coordinator Agent Response: {response}", response.CoordinatorResponse);
                }
                if (response.DevOpsResponse != null)
                {
                    _logger.LogInformation("DevOps Agent Response: {response}", response.DevOpsResponse);
                }
                if (response.ServiceNowResponse != null)
                {
                    _logger.LogInformation("ServiceNow Agent Response: {response}", response.ServiceNowResponse);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing chat request");
                return StatusCode(500, "Internal server error.");
            }

            return new OkObjectResult(response);
        }
    }
}
