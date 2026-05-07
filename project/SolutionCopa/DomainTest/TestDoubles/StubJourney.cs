using ProjectCopa.Interfaces;

namespace DomainTest.TestDoubles;

public class StubJourney : IJourney
{
    public int Number { get; }

    public StubJourney(int number = 1)
    {
        Number = number;
    }
}
