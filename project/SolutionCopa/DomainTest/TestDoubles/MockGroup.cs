using ProjectCopa.Interfaces;

namespace DomainTest.TestDoubles;

public class MockGroup : IGroup
{
    public string Name { get; }

    public MockGroup(string name = "A")
    {
        Name = name;
    }
}
