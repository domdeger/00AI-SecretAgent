using System.Text.Json;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

public static class MessageDataExtraction
{
    public static async Task<Message> ExtractMessageContents(
        Kernel kernel,
        string encryptedMessage,
        JsonSerializerOptions jsonSerializerOptions
    )
    {
        var messageChatResponse = await kernel.InvokePromptAsync(
            $"Extract the contents of the following message. Do not decrypt the payload. Just add extract the data into a json structure, do not make anything up:\n {encryptedMessage}",
            new(new OpenAIPromptExecutionSettings() { ResponseFormat = typeof(Message) }) { }
        );

        var messageContents =
            JsonSerializer.Deserialize<Message>(
                messageChatResponse.ToString(),
                jsonSerializerOptions
            ) ?? throw new Exception("Failed to deserialize message contents.");

        return messageContents;
    }
}
