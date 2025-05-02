using System.Text.Json;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Spectre.Console;

public static class DecryptSecretMessage
{
    public static async Task<(string decryptedMessage, Message messageContents)> DecryptAsync(
        Kernel kernel,
        Agent analyticAgent,
        string encryptedMessage,
        JsonSerializerOptions jsonSerializerSettings
    )
    {
        AnsiConsole.MarkupLine(
            "[bold yellow]Encrypted Message intercepted:[/]\n[bold blue]{0}[/]",
            encryptedMessage
        );

        // Use the message extractor to extract message contents from raw text.
        var messageContents = await MessageDataExtraction.ExtractMessageContents(
            kernel,
            encryptedMessage,
            jsonSerializerSettings
        );

        // Create a new chat session
        ChatHistory chatMessageContentsHistory =
        [
            new(AuthorRole.User, "Decrypt this message:"),
            new(AuthorRole.User, $"The message is: {messageContents.Content}"),
            new(AuthorRole.User, "First make a plan how to deterimine the used cipher."),
        ];

        ChatHistoryAgentThread chatHistoryAgentThread = new(chatMessageContentsHistory);

        // Invoke the agent to create a plan
        await analyticAgent.InvokeAsync(chatHistoryAgentThread).ToArrayAsync();
        chatHistoryAgentThread.ChatHistory.AddUserMessage("Analyze the cipher.");
        await analyticAgent.InvokeAsync(chatHistoryAgentThread).ToArrayAsync();
        chatHistoryAgentThread.ChatHistory.AddUserMessage("Decrypt the message.");
        // Invoke the agent to execute the plan and decrypt
        var response = await analyticAgent.InvokeAsync(chatHistoryAgentThread).ToArrayAsync();

        var decryptedMessage = response.First().Message.ToString();

        AnsiConsole.MarkupLine(
            "[bold yellow]Sender:[/] [bold blue]{0}[/]\n[bold yellow]Recipient:[/] [bold blue]{1}[/]",
            messageContents.Sender,
            messageContents.Recipient
        );
        AnsiConsole.MarkupLine(
            "[bold yellow]Decrypted Message:[/] [bold blue]{0}[/]",
            decryptedMessage
        );

        return (decryptedMessage, messageContents);
    }
}
