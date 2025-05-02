using System.Text.Json;
using _00AI.Messages;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
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
kernelBuilder.Plugins.AddFromType<DecryptPlugin>();
var kernel = kernelBuilder.Build();

var jsonSerializerSettings = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
AnsiConsole.MarkupLine("[italic][green]Welcome to the 00AI Mission Control Center.[/][/]");

ChatCompletionAgent analyticAgent = new()
{
    Kernel = kernel,
    Name = "CryptoAnalyticAgent",
    Arguments = new(
        new OpenAIPromptExecutionSettings()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
        }
    ),
    Instructions =
        "You are an agent that helps to analyze encrypted messages and provides solutions to decrypt them. If possible use your tools to decrypt the message. If you cannot decrypt the message, provide a detailed explanation of why you cannot decrypt it.",
};

var message = SecretMessageGenerator.GenerateSecretMessage();
AnsiConsole.MarkupLine("[bold yellow]Encrypted Message:[/] [bold blue]{0}[/]", message);

var messageChatResponse = await kernel.InvokePromptAsync(
    $"Extract the contents of the following message. It contains some encrypted data. Do not decrypt the payload. Just add extract the data, do not make anything up:\n {message}",
    new(new OpenAIPromptExecutionSettings() { ResponseFormat = typeof(Message) }) { }
);

var messageContents =
    JsonSerializer.Deserialize<Message>(messageChatResponse.ToString(), jsonSerializerSettings)
    ?? throw new Exception("Failed to deserialize message contents.");

// Create a new chat session
ChatHistory chatMessageContents =
[
    new(AuthorRole.User, "Hello, I need help decrypting a message."),
    new(AuthorRole.User, $"The message is: {messageContents.Content}"),
];

ChatHistoryAgentThread chatHistoryAgentThread = new(chatMessageContents);

var response = await analyticAgent.InvokeAsync(chatHistoryAgentThread).ToArrayAsync();
Console.WriteLine(response);
