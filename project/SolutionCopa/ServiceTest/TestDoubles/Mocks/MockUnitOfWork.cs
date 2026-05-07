using DataAccess.Interfaces;

namespace ServiceTest.TestDoubles.Fakes_Stubs;

public sealed class MockUnitOfWork : IUnitOfWork
{
    public int CommitCalls { get; private set; }

    public void Commit()
    {
        CommitCalls++;
    }
}
