using ProjectCopa.Interfaces;

namespace DomainTest.TestDoubles;

public class StubStadium : IStadium
{
    public int StadiumId { get; }
    public string Name { get; }

    public StubStadium(int stadiumId = 1, string name = "Stadium")
    {
        StadiumId = stadiumId;
        Name = name;
    }
}
