using DataAccess.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataAccessTest;

[TestClass]
public class UnitOfWorkListTests
{
    private UnitOfWorkList _unitOfWork = null!;

    [TestInitialize]
    public void Setup()
    {
        _unitOfWork = new UnitOfWorkList();
    }

    [TestMethod]
    public void CommitDoesNotThrow()
    {
        _unitOfWork.Commit();
    }
}
