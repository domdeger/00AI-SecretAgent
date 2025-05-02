using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Spectre.Console;

public static class Interrogation
{
    public static async Task StartInterrogation(
        Kernel kernel,
        LlmSettings llmSettings,
        string recipientName
    )
    {
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
            recipientName
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
            "[bold red]We could catch {0} and bring her/him to a hidden outpost. Mr. 00AI, let's start the interrogation.[/]",
            recipientName
        );

        var interrogationGroupChat = new AgentGroupChat(agent00AI, villianAgent);
        interrogationGroupChat.ExecutionSettings = new()
        {
            SelectionStrategy = new SequentialSelectionStrategy(),
            TerminationStrategy = new KernelFunctionTerminationStrategy(
                KernelFunctionFactory.CreateFromPrompt(
                    @"If the villian provided the secret, write 'yes'. Otherwise write 'no'.
                     
                    {{$_history_}}"
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
    }
}
