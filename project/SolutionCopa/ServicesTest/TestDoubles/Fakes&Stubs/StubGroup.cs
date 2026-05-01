using ProjectCopa.Interfaces;

namespace ServicesTest.TestDoubles.Fakes_Stubs;

public sealed class StubGroup : IGroup
{
    public string Name { get; }

    public StubGroup(string name)
    {
        Name = name;
    }
}
