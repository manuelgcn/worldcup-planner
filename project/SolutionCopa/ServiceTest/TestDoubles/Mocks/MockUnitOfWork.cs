using DataAccess.Interfaces;

namespace ServiceTest.TestDoubles.Mocks;

public sealed class MockUnitOfWork : IUnitOfWork
{
    public int CommitCalls { get; private set; }

    public void Commit()
    {
        CommitCalls++;
    }
}
