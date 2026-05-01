using DataAccess.Interfaces;

namespace ServicesTest.TestDoubles;

public sealed class FakeUnitOfWork : IUnitOfWork
{
    public int CommitCalls { get; private set; }

    public void Commit()
    {
        CommitCalls++;
    }
}
