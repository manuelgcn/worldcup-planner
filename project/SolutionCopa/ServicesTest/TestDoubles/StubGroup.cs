using ProjectCopa.Interfaces;

namespace ServicesTest.TestDoubles;

public sealed class StubGroup : IGroup
{
    public string Name { get; }

    public StubGroup(string name)
    {
        Name = name;
    }
}
