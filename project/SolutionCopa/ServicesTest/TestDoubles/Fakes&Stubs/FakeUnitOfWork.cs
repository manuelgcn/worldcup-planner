using DataAccess.Interfaces;

namespace ServicesTest.TestDoubles.Fakes_Stubs;

public sealed class FakeUnitOfWork : IUnitOfWork
{
    public int CommitCalls { get; private set; }

    public void Commit()
    {
        CommitCalls++;
    }
}
