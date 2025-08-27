using System.Net.Mime;
using gen_ai_api_agents.Models;
using gen_ai_api_agents.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Collections.Concurrent;

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

        public ChatController(
            ILogger<ChatController> logger,
            IConfiguration configuration,
            Kernel kernel,
            IChatCompletionService chat,
            IChatHistoryManager chathistorymanager)
        {
            _kernel = kernel;
            _chat = chat;
            _chatHistoryManager = chathistorymanager;
            _logger = logger;
        }

        [HttpPost]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Post([FromBody] ChatProviderRequest chatRequest)
        {
            var response = new ChatProviderResponse();

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
                var chatHistory = _chatHistoryManager.GetOrCreateChatHistory(sessionId);

                chatHistory.AddUserMessage(chatRequest.Prompt);

                ChatMessageContent? result = null;

                result = await _chat.GetChatMessageContentAsync(
                      chatHistory,
                      executionSettings: new OpenAIPromptExecutionSettings { Temperature = 0.8, TopP = 0.0, ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions },
                      kernel: _kernel);

                response.ChatResponse = result.Content;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing chat request");
                return StatusCode(500, "Internal server error.");
            }

            return new OkObjectResult(response);
        }

        [HttpPost("movies")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ChatProviderMovieMultiAgent([FromBody] ChatProviderRequest chatRequest)
        {
            var response = new ChatProviderResponse();
            try
            {
                if (string.IsNullOrEmpty(chatRequest.SessionId))
                {
                    chatRequest.SessionId = Guid.NewGuid().ToString();
                }
                if (string.IsNullOrEmpty(chatRequest.Prompt))
                {
                    _logger.LogWarning("Chat request is missing prompt.");
                    return new BadRequestResult();
                }

                // Static movie data
                var movies = new List<Movie>
                {
                    new Movie { Title = "Inception", Cinema = "Grand Cinema", Time = "7:00 PM", Price = 15 },
                    new Movie { Title = "The Matrix", Cinema = "Cityplex", Time = "9:00 PM", Price = 13 }
                };

                // Define system prompts for each agent
                var routerPrompt = "You are a router agent. Only respond to movie-related requests. If the user asks about movies, respond with 'movie'. If the user asks to buy or refund a ticket, respond with 'purchase'. Only provide details about the static movies.";
                var moviePrompt = "You are a movie agent. Respond with the list of available movies and their details. Only use the static movie data provided.";
                var purchasingPrompt = "You are a purchasing agent. Handle ticket purchases and refunds for the static movies. Respond with confirmation messages for purchases and refunds.";

                // Router agent: decide which agent to use
                var routerHistory = new ChatHistory();
                routerHistory.AddSystemMessage(routerPrompt);
                routerHistory.AddUserMessage(chatRequest.Prompt);
                var routerResult = await _chat.GetChatMessageContentAsync(
                    routerHistory,
                    executionSettings: new OpenAIPromptExecutionSettings { Temperature = 0.2, TopP = 0.0 },
                    kernel: _kernel);
                var routerResponse = routerResult.Content?.ToLowerInvariant() ?? string.Empty;

                string agentResponse = string.Empty;
                if (routerResponse.Contains("movie"))
                {
                    // Movie Agent
                    var movieHistory = new ChatHistory();
                    movieHistory.AddSystemMessage(moviePrompt);
                    movieHistory.AddUserMessage($"Static movie data: {string.Join(", ", movies.Select(m => m.Title))}");
                    movieHistory.AddUserMessage(chatRequest.Prompt);
                    var movieResult = await _chat.GetChatMessageContentAsync(
                        movieHistory,
                        executionSettings: new OpenAIPromptExecutionSettings { Temperature = 0.2, TopP = 0.0 },
                        kernel: _kernel);
                    agentResponse = movieResult.Content;
                }
                else if (routerResponse.Contains("purchase"))
                {
                    // Purchasing Agent
                    var purchasingHistory = new ChatHistory();
                    purchasingHistory.AddSystemMessage(purchasingPrompt);
                    purchasingHistory.AddUserMessage($"Static movie data: {string.Join(", ", movies.Select(m => m.Title))}");
                    purchasingHistory.AddUserMessage(chatRequest.Prompt);
                    var purchasingResult = await _chat.GetChatMessageContentAsync(
                        purchasingHistory,
                        executionSettings: new OpenAIPromptExecutionSettings { Temperature = 0.2, TopP = 0.0 },
                        kernel: _kernel);
                    agentResponse = purchasingResult.Content;
                }
                else
                {
                    agentResponse = "Sorry, I can only help with movie listings, ticket purchases, or refunds for the available movies.";
                }

                response.ChatResponse = agentResponse;
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
