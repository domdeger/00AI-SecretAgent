using Microsoft.SemanticKernel;

public class VillianPlugin
{
    public static int LifePoints { get; private set; } = 100;

    public static int SubtractLifePoints(uint points)
    {
        LifePoints -= (int)points;

        if (LifePoints < 0)
        {
            LifePoints = 0;
        }

        return LifePoints;
    }

    [KernelFunction]
    public static int GetLifePoints()
    {
        return LifePoints;
    }
}
