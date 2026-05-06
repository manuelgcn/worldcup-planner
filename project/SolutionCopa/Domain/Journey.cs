using ProjectCopa.Interfaces;

namespace ProjectCopa;

public class Journey : IJourney
{
    public int Number { get; }

    public Journey(int number = 0)
    {
        Number = number;
    }
}
