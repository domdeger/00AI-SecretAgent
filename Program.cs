using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Spectre.Console;

// Build configuration
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true)
    .Build();

// Bind configuration to strongly typed class
var llmSettings =
    configuration.GetSection("LLM").Get<LlmSettings>()
    ?? throw new Exception("LLM settings not found in configuration.");

// Create Kernel builder
var kernelBuilder = Kernel
    .CreateBuilder()
    .AddAzureOpenAIChatCompletion(
        deploymentName: llmSettings.Model,
        modelId: llmSettings.Model,
        apiKey: llmSettings.Token,
        endpoint: llmSettings.Endpoint
    );

var kernel = kernelBuilder.Build();

AnsiConsole.MarkupLine("[italic][green]Welcome to the 00AI Mission Control Center.[/][/]");

ChatCompletionAgent analyticAgent = new()
{
    Kernel = kernel,
    Name = "CryptoAnalyticAgent",
    Instructions =
        "You are an agent that helps to analyze encrypted messages and provides solutions to decrypt them.",
};

// Create a new chat session
ChatHistory chatMessageContents =
[
    new(AuthorRole.User, "Hello, I need help decrypting a message."),
];

ChatHistoryAgentThread chatHistoryAgentThread = new(chatMessageContents);

var response = await analyticAgent.InvokeAsync(chatHistoryAgentThread).ToArrayAsync();
