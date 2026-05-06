using ProjectCopa.Interfaces;

namespace ProjectCopa;

public class Group : IGroup
{
    public string Name { get; }

    public Group(string name = "")
    {
        Name = name;
    }
}
