using DataAccess.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataAccessTest;

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
