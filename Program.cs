using gen_ai_api_agents.Middleware;
using gen_ai_api_agents.Prompts;
using gen_ai_api_agents.Services;
using Microsoft.OpenApi.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true); // Add this line

var configuration = builder.Configuration;
var mytest = configuration["AzureOpenAI:DeploymentName"] ?? throw new ArgumentException("The AzureOpenAI:DeploymentName is not configured or is empty.");
var endpoint = configuration["AzureOpenAI:Endpoint"] ?? throw new ArgumentException("The AzureOpenAI:Endpoint is not configured or is empty.");
var apiKey1 = configuration["AzureOpenAI:ApiKey"] ?? throw new ArgumentException("The AzureOpenAI:ApiKey is not configured or is empty.");


var apiDeploymentName = configuration["AzureOpenAI:DeploymentName"] ?? throw new ArgumentException("The AzureOpenAI:DeploymentName is not configured or is empty.");
var apiEndpoint = configuration["AzureOpenAI:Endpoint"] ?? throw new ArgumentException("The AzureOpenAI:Endpoint is not configured or is empty.");
var apiKey = configuration["AzureOpenAI:ApiKey"] ?? throw new ArgumentException("The AzureOpenAI:ApiKey is not configured or is empty.");

// Add services to the container.
builder.Services.AddApplicationInsightsTelemetry();
builder.Logging.AddConsole();

builder.Services.AddTransient<Kernel>(s =>
{
    var builder = Kernel.CreateBuilder();
    builder.AddAzureOpenAIChatCompletion(
        serviceId: "azure-openai",
        deploymentName: apiDeploymentName,
        endpoint: apiEndpoint,
        apiKey: apiKey);

    return builder.Build();
});

builder.Services.AddSingleton<IChatCompletionService>(sp =>
                     sp.GetRequiredService<Kernel>().GetRequiredService<IChatCompletionService>());

builder.Services.AddSingleton<IChatHistoryManager>(sp =>
{
    string systemmsg = CorePrompts.GetSystemPromptTest();
    return new ChatHistoryManager(systemmsg);
});


builder.Services.AddHostedService<ChatHistoryCleanupService>();

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Your API", Version = "v1" });

    // Configure API key for Swagger
    c.AddSecurityDefinition("api-key", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter your API key",
        Name = "api-key",
        Type = SecuritySchemeType.ApiKey
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "api-key"
                    }
                },
                new string[] {}
            }
        });
});

var app = builder.Build();
app.UseMiddleware<ApiKeyMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();