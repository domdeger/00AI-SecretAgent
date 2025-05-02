using Microsoft.SemanticKernel;

public class Agent00AIPlugin
{
    [KernelFunction]
    public void Tickle()
    {
        var remainingLife = VillianPlugin.SubtractLifePoints(10);
        Console.WriteLine(
            $"Agent 00AI tickled the villain! Remaining life points: {remainingLife}"
        );
    }

    [KernelFunction]
    public void TellEndlessKnockKnockJoke()
    {
        Console.WriteLine(
            "Knock knock! Who's there? Agent 00AI! Agent 00AI who? Agent 00AI who will never stop telling knock-knock jokes!"
        );
        VillianPlugin.SubtractLifePoints(35);
    }

    [KernelFunction]
    public void TellEndlessChuckNorrisJoke()
    {
        Console.WriteLine("Chuck Norris doesn't do push-ups. He pushes the Earth down!");
        VillianPlugin.SubtractLifePoints(50);
    }
}
