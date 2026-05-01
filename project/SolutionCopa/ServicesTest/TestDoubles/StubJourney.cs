using ProjectCopa.Interfaces;

namespace ServicesTest.TestDoubles;

public sealed class StubJourney : IJourney
{
    public int Number { get; }

    public StubJourney(int number)
    {
        Number = number;
    }
}
