using DataAccess.Interfaces;

namespace DataAccess.Repositories;

public class UnitOfWorkRepository : IUnitOfWork
{
    public void Commit()
    {
        // No-op: in-memory list commits changes immediately.
    }
}
