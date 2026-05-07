using ProjectCopa.Interfaces;

namespace DomainTest.TestDoubles;

public class StubGroup : IGroup
{
    public string Name { get; }

    public StubGroup(string name = "A")
    {
        Name = name;
    }
}
