using DataAccess.Interfaces;

namespace ServiceTest.TestDoubles.Mocks;

public sealed class MockUnitOfWork : IUnitOfWork
{
    public int CommitCalls { get; private set; }
    private bool ShouldFailOnCommit { get; set; }
    private Exception CommitException { get; set; } = new InvalidOperationException("Database unavailable");

    public void Commit()
    {
        CommitCalls++;

        if (ShouldFailOnCommit)
        {
            throw CommitException;
        }
    }

    public void EnableCommitFailure(Exception commitException)
    {
        ShouldFailOnCommit = true;
        CommitException = commitException;
    }
}
