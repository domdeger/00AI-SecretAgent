using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
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
    new(AuthorRole.User, "First make a plan how to analyze and decrpyt the message."),
];

ChatHistoryAgentThread chatHistoryAgentThread = new(chatMessageContents);

await analyticAgent.InvokeAsync(chatHistoryAgentThread).ToArrayAsync();
chatHistoryAgentThread.ChatHistory.AddUserMessage("Now decrypt the message.");
var response = await analyticAgent.InvokeAsync(chatHistoryAgentThread).ToArrayAsync();

var decyrptedMessage = response.First().Message.ToString();

var villianAgent = AIAgentFactory.CreateVillianAgent(
    kernel =>
    {
        kernel.AddAzureOpenAIChatCompletion(
            deploymentName: llmSettings.Model,
            modelId: llmSettings.Model,
            apiKey: llmSettings.Token,
            endpoint: llmSettings.Endpoint
        );
    },
    messageContents.Recipient
);

var agent00AI = AIAgentFactory.CreateAgent00AI(kernel =>
{
    kernel.AddAzureOpenAIChatCompletion(
        deploymentName: llmSettings.Model,
        modelId: llmSettings.Model,
        apiKey: llmSettings.Token,
        endpoint: llmSettings.Endpoint
    );
});

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

var interrogationGroupChat = new AgentGroupChat(agent00AI, villianAgent);
interrogationGroupChat.ExecutionSettings = new()
{
    SelectionStrategy = new SequentialSelectionStrategy(),
    TerminationStrategy = new KernelFunctionTerminationStrategy(
        KernelFunctionFactory.CreateFromPrompt(
            @"If the villian provided the secret, write 'yes'. Otherwise write 'no'.
             \n{{$_history_}}"
        ),
        kernel
    )
    {
        ResultParser = result =>
            result.ToString().Equals("yes", StringComparison.CurrentCultureIgnoreCase),
    },
};

await foreach (var messagePart in interrogationGroupChat.InvokeAsync())
{
    AnsiConsole.MarkupLine(
        "[bold red]{0}:[/] [bold blue]{1}[/]",
        messagePart.AuthorName,
        messagePart.Content
    );
}
