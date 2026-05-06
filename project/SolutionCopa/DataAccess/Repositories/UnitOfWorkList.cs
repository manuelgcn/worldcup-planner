using DataAccess.Interfaces;

namespace DataAccess.Repositories;

public class UnitOfWorkList : IUnitOfWork
{
    public void Commit()
    {
        // No-op: in-memory list commits changes immediately.
    }
}
