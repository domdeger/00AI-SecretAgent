using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace _00AI.Services
{
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
                    @"You are an agent that helps to analyze encrypted messages and provides solutions to decrypt them. If possible use your tools to decrypt the message. If you cannot decrypt the message, 
                    provide a detailed explanation of why you cannot decrypt it. You only answer with the decrypted message, do not add any other text.",
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
                Name = name,
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
    }
}
