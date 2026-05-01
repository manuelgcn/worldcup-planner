using ProjectCopa.Interfaces;

namespace ServicesTest.TestDoubles.Fakes_Stubs;

public sealed class StubJourney : IJourney
{
    public int Number { get; }

    public StubJourney(int number)
    {
        Number = number;
    }
}
