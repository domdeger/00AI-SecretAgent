using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
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

var (decryptedMessage, messageContents) = await DecryptSecretMessage.DecryptAsync(
    kernel,
    analyticAgent,
    message,
    jsonSerializerSettings
);

await Interrogation.StartInterrogation(kernel, llmSettings, messageContents.Recipient);
