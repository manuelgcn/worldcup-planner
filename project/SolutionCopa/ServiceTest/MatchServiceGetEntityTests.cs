using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectCopa;
using Services.Exceptions;
using ServiceTest.TestDoubles;

namespace ServiceTest;

[TestClass]
public sealed class MatchServiceGetEntityTests
{
    private MatchServiceTestContext _context = null!;

    [TestInitialize]
    public void Setup()
    {
        _context = new MatchServiceTestContext();
    }

    [TestMethod]
    public void GetEntity_ReturnsStoredMatchWithItsGraph()
    {
        _context.Service.CreateMatch(MatchServiceTestContext.NewGroupMatchRequest(1));

        Match match = _context.Service.GetEntity(1);

        Assert.AreEqual(1, match.MatchId);
        Assert.AreEqual(10, match.HomeTeam!.Id);
        Assert.AreEqual(20, match.AwayTeam!.Id);
        Assert.AreEqual("Main Stadium", match.Stadium!.Name);
    }

    [TestMethod]
    [ExpectedException(typeof(MatchServicesException))]
    public void GetEntity_WhenMatchDoesNotExist_ThrowsServiceException()
    {
        _context.Service.GetEntity(999);
    }
}
