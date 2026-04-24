using ProjectCopa.Interfaces;

namespace DomainTest.TestDoubles;

public class MockStadium : IStadium
{
    public int StadiumId { get; }
    public string Name { get; }

    public MockStadium(int stadiumId = 1, string name = "Mock Stadium")
    {
        StadiumId = stadiumId;
        Name = name;
    }
}

