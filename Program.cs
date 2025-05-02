using System.Text.Json;
using _00AI.Messages;
using _00AI.Services;
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

kernelBuilder.Plugins.AddFromType<DecryptPlugin>();
var kernel = kernelBuilder.Build();

var jsonSerializerSettings = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
AnsiConsole.MarkupLine("[italic][green]Welcome to the 00AI Mission Control Center.[/][/]");

// Create the analytic agent using the factory
var analyticAgent = AIAgentFactory.CreateMessageAnalyticAgent(kernel => 
{
    kernel.AddAzureOpenAIChatCompletion(
        deploymentName: llmSettings.Model,
        modelId: llmSettings.Model,
        apiKey: llmSettings.Token,
        endpoint: llmSettings.Endpoint
    );
});


var message = SecretMessageGenerator.GenerateSecretMessage();
AnsiConsole.MarkupLine(
    "[bold yellow]Encrypted Message intercepted:[/]\n[bold blue]{0}[/]",
    message
);

// Use the message decryptor to extract message contents
var messageContents = await MessageDataExtraction.ExtractMessageContents(
    kernel,
    message,
    jsonSerializerSettings
);

// Create a new chat session
ChatHistory chatMessageContents =
[
    new(AuthorRole.User, "Hello, I need help decrypting a message."),
    new(AuthorRole.User, $"The message is: {messageContents.Content}"),
];

ChatHistoryAgentThread chatHistoryAgentThread = new(chatMessageContents);

var response = await analyticAgent.InvokeAsync(chatHistoryAgentThread).ToArrayAsync();
var decyrptedMessage = response.First().Message.ToString();

var villianAgent = AIAgentFactory.CreateVillianAgent(kernel => 
{
    kernel.AddAzureOpenAIChatCompletion(
        deploymentName: llmSettings.Model,
        modelId: llmSettings.Model,
        apiKey: llmSettings.Token,
        endpoint: llmSettings.Endpoint
    );
}, messageContents.Recipient);

AnsiConsole.MarkupLine(
    "[bold yellow]Sender:[/] [bold blue]{0}[/]\n[bold yellow]Recipient:[/] [bold blue]{1}[/]",
    messageContents.Sender,
    messageContents.Recipient
);
AnsiConsole.MarkupLine("[bold yellow]Decrypted Message:[/] [bold blue]{0}[/]", decyrptedMessage);

AnsiConsole.MarkupLine(
    "[bold red]We could catch \"{0}\" and bring her/him to a hidden outpost. Mr. 00AI, let's start the interrogation.[/]",
    messageContents.Recipient
);
