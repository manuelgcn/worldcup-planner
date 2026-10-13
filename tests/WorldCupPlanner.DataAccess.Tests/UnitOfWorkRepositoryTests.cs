using WorldCupPlanner.DataAccess.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WorldCupPlanner.DataAccess.Tests;

[TestClass]
public class UnitOfWorkRepositoryTests
{
    private UnitOfWorkRepository _unitOfWork = null!;

    [TestInitialize]
    public void Setup()
    {
        _unitOfWork = new UnitOfWorkRepository();
    }

    [TestMethod]
    public void CommitDoesNotThrow()
    {
        _unitOfWork.Commit();
    }
}
