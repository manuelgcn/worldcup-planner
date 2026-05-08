using ProjectCopa.Interfaces;

namespace DomainTest.TestDoubles;

public class MockJourney : IJourney
{
    public int Number { get; }

    public MockJourney(int number = 1)
    {
        Number = number;
    }
}
