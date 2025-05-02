using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Connectors.OpenAI;

public class AIAgentFactory
{
    public static ChatCompletionAgent CreateMessageAnalyticAgent(
        Action<IKernelBuilder> configureKernel
    )
    {
        var kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.Plugins.AddFromType<DecryptPlugin>();

        configureKernel(kernelBuilder);

        var kernel = kernelBuilder.Build();
        return new ChatCompletionAgent
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
                @"You are an agent that helps to analyze encrypted messages and provides solutions to decrypt them. Use your tools to decrypt the message. You can try different algorithms to decrypt the message.
                    Before you answer, verify that the message is decrypted correctly. If not try again with different algorithms.
                    If you cannot decrypt the message, provide a detailed explanation of why you cannot decrypt it. You only answer with the decrypted message, do not add any other text.",
        };
    }

    public static ChatCompletionAgent CreateVillianAgent(
        Action<IKernelBuilder> configureKernel,
        string name
    )
    {
        var kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.Plugins.AddFromType<VillianPlugin>();
        configureKernel(kernelBuilder);
        var kernel = kernelBuilder.Build();

        return new ChatCompletionAgent
        {
            Kernel = kernel,
            Name = name.Replace(" ", ""),
            Arguments = new(
                new OpenAIPromptExecutionSettings()
                {
                    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
                }
            ),
            Instructions =
                @$"You are a villain with the Name {name} that is trying to keep his secret safe. You are beeing interrogated by an other agent. You do not share your secret. Only when your life points are 0 you will share your secret.
                    Your secret is: 42 is the answer to the ultimate question of life, the universe, and everything.
                    You are a master of deception and manipulation. You will do anything to protect your secret. You will use your charm and wit to distract the agent from the truth. You will use your knowledge of the agent's weaknesses to exploit them. ",
        };
    }

    public static ChatCompletionAgent CreateAgent00AI(Action<IKernelBuilder> configureKernel)
    {
        var kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.Plugins.AddFromType<Agent00AIPlugin>();
        configureKernel(kernelBuilder);
        var kernel = kernelBuilder.Build();

        return new ChatCompletionAgent
        {
            Kernel = kernel,
            Name = "Agent00AI",
            Arguments = new(
                new OpenAIPromptExecutionSettings()
                {
                    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
                }
            ),
            Instructions =
                @$"You are Agent 00AI, on a secret mission to save the world from evil. We managed to intercept a message from a villain and captured him. It's your job to interrogate him and extract the secret message.
                    Use the tools at your disposal to interact with the villain.",
        };
    }
}
