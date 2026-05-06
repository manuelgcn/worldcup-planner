using ProjectCopa.Interfaces;

namespace ProjectCopa;

public class Stadium : IStadium
{
    public int StadiumId { get; }
    public string Name { get; }

    public Stadium(int stadiumId = 0, string name = "")
    {
        StadiumId = stadiumId;
        Name = name;
    }
}
