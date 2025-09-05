# Gen AI API Agents

A .NET 8 Web API demonstration showcasing AI-powered chat agents using Microsoft Semantic Kernel and Azure OpenAI. This solution implements a multi-agent architecture with intelligent routing and domain-specific conversational AI capabilities.

## 🎯 What This Solution Demonstrates

This project serves as a comprehensive example of modern AI agent architecture patterns, demonstrating:

### 🤖 Multi-Agent Chat System
- **Router Agent**: Intelligently routes user requests to appropriate specialized agents
- **Domain-Specific Agents**: Separate agents for different business domains (missing persons, movie tickets)
- **Agent Orchestration**: Seamless coordination between multiple AI agents within a single conversation

### 🧠 Microsoft Semantic Kernel Integration
- Advanced prompt engineering with system message templates
- Function calling and tool behavior configuration
- Chat completion services with configurable temperature and TopP settings
- Kernel-based dependency injection for AI services

### 💬 Persistent Session Management
- Session-based chat history maintenance across conversations
- Automatic cleanup of expired chat sessions
- Context preservation for follow-up questions and conversations

### 🔐 Enterprise Security Features
- API key authentication middleware
- Configurable endpoint protection
- Request validation and error handling

### 🏗️ Modern .NET Architecture
- Clean separation of concerns with Controllers, Services, Models, and Middleware
- Dependency injection with scoped and singleton services
- Background services for maintenance tasks
- Docker containerization support

## 🏛️ Architecture Overview

```
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│   Client App    │────│   API Gateway    │────│ Chat Controller │
└─────────────────┘    └──────────────────┘    └─────────────────┘
                                                         │
                                                         ▼
                               ┌─────────────────────────────────────┐
                               │          Coordinator Agent          │
                               └─────────────────────────────────────┘
                                         │
                                         ▼
                        ┌─────────────────────────────────┐
                        ▼                                 ▼
                ┌──────────────┐                  ┌──────────────┐
                │    DevOps    │                  │   Service    │
                │    Agent     │                  │    Agent     │
                └──────────────┘                  └──────────────┘
                        │                                 │
                        └────────────────────────────────┘
                                         │
                                         ▼
                       ┌─────────────────────────────────────┐
                       │       Azure OpenAI Service          │
                       └─────────────────────────────────────┘
```

## 🚀 Key Components

### Controllers
- **ChatController**: Main API endpoint handling chat requests with two modes:
  - `/chat` - General chat 


### Services
** Not being used but the classes included for reference**
- **ChatHistoryManager**: Manages persistent chat sessions with automatic cleanup
- **ChatHistoryCleanupService**: Background service for maintaining chat session hygiene

### Models
- **ChatProviderRequest/Response**: API contract models

### Middleware
- **ApiKeyMiddleware**: Secures API endpoints with key-based authentication

### Prompts
- **CorePrompts**: System message templates for different agent personalities and behaviors

## 🛠️ Technology Stack

- **.NET 8.0**: Latest .NET framework with minimal APIs and enhanced performance
- **Microsoft Semantic Kernel**: Advanced AI orchestration framework
- **Azure OpenAI**: Enterprise-grade OpenAI services
- **ASP.NET Core**: Web API framework with built-in dependency injection
- **Application Insights**: Telemetry and monitoring
- **Swagger/OpenAPI**: Interactive API documentation
- **Docker**: Containerization support

## ⚙️ Setup and Configuration

### Prerequisites
- .NET 8.0 SDK
- Azure OpenAI Service access
- Valid Azure OpenAI API key and endpoint

### Configuration
1. Clone the repository:
```bash
git clone https://github.com/Rickcau/gen-ai-api-agents.git
cd gen-ai-api-agents
```

2. Configure Azure OpenAI settings in `appsettings.json` or create `appsettings.Local.json`:
```json
{
  "AzureOpenAI": {
    "DeploymentName": "gpt-4o",
    "Endpoint": "Your_Endpoint_Here",
    "ApiKey": "Your_ApiKey_Here"
  },
  "AzureDevOps": {
    "UseMock": true,
    "OrganizationUrl": "https://dev.azure.com/yourorganization",
    "ProjectName": "YourProject",
    "PersonalAccessToken": ""
  },    
  "ServiceNow": {
    "UseMock": true,
    "InstanceUrl": "https://yourinstance.service-now.com",
    "Username": "",
    "Password": ""
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.SemanticKernel": "Warning"
    }
  }
}
```

3. Build and run the application:
```bash
dotnet build
dotnet run
```

4. Access the Swagger UI at `https://localhost:5001/swagger` (or your configured port)


## 📡 API Usage Examples

### Basic Chat Request
```http
POST /chat
Content-Type: application/json
api-key: your-api-key

{
  "sessionId": "optional-session-id",
  "userId": "some-user-id","
  "prompt": "What workitems are assigned to John Doe?"
}
```

### Example Prompts to test
```{
  "sessionId": "session123",
  "userId": "user456",
  "prompt": "I am looking for information about a missing person named John Doe."
}
```

```{
  "sessionId": "session123",
  "userId": "user456",
  "prompt": "Give me a list of the most recent workitems"
}
```

### Response Format
```json
{
  
  "coordinatorResponse": "Routing your request to the DevOps Agent",
  "devOpsResponse": "Here are your results",
  "serviceNowResponse": ""
}
```

## 🎭 Agent Behavior Patterns

### Coordinator Agent
- Focuses exclusively on routing requests to the DevOps or ServiceNow agents
- Asks clarifying questions to gather complete information
- Provides structured responses based on available data

### DevOps Agent
1. Focuses on Azure DevOps requests
2. Uses specific prompts to extract relevant information
3. Leverages DevOpsPlugin for API interactions

### ServiceNow Agent
- Focuses on ServiceNow related requests
- Uses specific prompts to extract relevant information
- Leverages ServiceNowPlugin for API interactions

## 🔧 Development Notes

### Session Management
** ChatHistoryManager and ChatHistoryCleanupService are included for reference but not actively used in this demo. They illustrate how to manage persistent chat sessions.**
- Sessions automatically expire after 1 hour of inactivity
- Background cleanup service runs hourly to maintain performance
- Each session maintains independent conversation history

### Error Handling
- Comprehensive exception handling with appropriate HTTP status codes
- Detailed logging for troubleshooting and monitoring
- Graceful degradation when AI services are unavailable

### Extensibility
- Easy to add new agents by extending the router pattern
- Configurable prompts for customizing agent behavior
- Modular architecture supports additional business domains

## 🚨 Troubleshooting

### Common Issues
1. **API Key Authentication Failures**: Verify `GenAiApiKey` in configuration and include `api-key` header
2. **Azure OpenAI Errors**: Check endpoint URL, API key, and deployment name
3. **Build Warnings**: Nullable reference warnings are non-blocking and can be addressed in future iterations

### Monitoring
- Application Insights integration provides telemetry and performance metrics
- Structured logging available through ILogger interface
- Health checks can be added for production deployments

## 🤝 Contributing

This solution demonstrates enterprise-grade AI agent patterns and welcomes contributions that enhance the architectural examples, add new agent types, or improve the multi-agent orchestration patterns.

## 📄 License

MIT License - see LICENSE.txt for details

---

## 🎓 Learning Outcomes

By exploring this solution, developers will gain hands-on experience with:
- Microsoft Semantic Kernel framework implementation
- Multi-agent conversation design patterns
- Azure OpenAI service integration
- Session management in conversational AI
- Enterprise security patterns for AI APIs
- Background service implementation in .NET
- Docker containerization for AI applications

This project serves as a practical reference for building production-ready AI-powered APIs using modern .NET and Azure technologies.