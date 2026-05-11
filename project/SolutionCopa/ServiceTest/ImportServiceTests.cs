using Moq;
using Services;
using Services.DTOs;
using Services.Exceptions;
using Services.Interfaces;
using DataAccess.Interfaces;
using ServiceTest.TestDoubles.Mocks;

namespace ServiceTest;

[TestClass]
public class ImportServiceTests
{
    private MockTeamRepository _teamRepository = null!;
    private MockUnitOfWork _unitOfWork = null!;
    private Mock<ITeamParser> _teamParserMock = null!;
    private ImportService _service = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _teamRepository = CreateTeamRepository();
        _unitOfWork = CreateUnitOfWork();
        _teamParserMock = CreateTeamParserMock();
        _service = CreateService(_teamRepository, _unitOfWork, _teamParserMock.Object);
    }

    [TestMethod]
    public void ImportTeamsCreatesTeamsAndCommits()
    {
        List<TeamImportDto> rows = BuildValidRows();
        MemoryStream stream = CreateImportStream();
        SetupParserRows(stream, rows);

        ImportResult result = _service.ImportTeams(stream);

        Assert.AreEqual(2, result.ImportedCount);
        Assert.AreEqual(2, _teamRepository.GetAll().Count);
        Assert.AreEqual(1, _unitOfWork.CommitCalls);
        VerifyParserCall(stream);
    }

    [TestMethod]
    public void ImportTeamsThrowsWhenDuplicateNameAppearsInSameFile()
    {
        List<TeamImportDto> rows = BuildDuplicateRows();
        MemoryStream stream = CreateImportStream();
        SetupParserRows(stream, rows);

        Assert.ThrowsException<ImportServiceException>(() => _service.ImportTeams(stream));
        Assert.AreEqual(1, _teamRepository.GetAll().Count);
        Assert.AreEqual(0, _unitOfWork.CommitCalls);
    }

    [TestMethod]
    public void ImportTeamsThrowsWhenRankingIsInvalid()
    {
        List<TeamImportDto> rows = BuildInvalidRankingRows();
        MemoryStream stream = CreateImportStream();
        SetupParserRows(stream, rows);

        Assert.ThrowsException<ImportServiceException>(() => _service.ImportTeams(stream));
        Assert.AreEqual(0, _teamRepository.GetAll().Count);
        Assert.AreEqual(0, _unitOfWork.CommitCalls);
    }

    [TestMethod]
    public void ImportTeamsPropagatesUnitOfWorkFailure()
    {
        MockUnitOfWork failingUnitOfWork = CreateFailingUnitOfWork();
        ImportService service = CreateService(_teamRepository, failingUnitOfWork, _teamParserMock.Object);
        List<TeamImportDto> rows = BuildSingleValidRow();
        MemoryStream stream = CreateImportStream();
        SetupParserRows(stream, rows);

        Assert.ThrowsException<InvalidOperationException>(() => service.ImportTeams(stream));
        Assert.AreEqual(1, _teamRepository.GetAll().Count);
        Assert.AreEqual(1, failingUnitOfWork.CommitCalls);
    }

    private static MockTeamRepository CreateTeamRepository()
    {
        return new MockTeamRepository();
    }

    private static MockUnitOfWork CreateUnitOfWork()
    {
        return new MockUnitOfWork();
    }

    private static Mock<ITeamParser> CreateTeamParserMock()
    {
        return new Mock<ITeamParser>();
    }

    private static ImportService CreateService(ITeamRepository teamRepository, IUnitOfWork unitOfWork, ITeamParser teamParser)
    {
        return ImportService.CreateInstance(teamRepository, unitOfWork, teamParser);
    }

    private static MemoryStream CreateImportStream()
    {
        return new MemoryStream();
    }

    private static List<TeamImportDto> BuildValidRows()
    {
        List<TeamImportDto> rows = new()
        {
            new TeamImportDto
            {
                RowNumber = 2,
                Name = "Argentina",
                Confederation = "CONMEBOL",
                Ranking = "500"
            },
            new TeamImportDto
            {
                RowNumber = 3,
                Name = "Japan",
                Confederation = "AFC",
                Ranking = "600"
            }
        };

        return rows;
    }

    private static List<TeamImportDto> BuildDuplicateRows()
    {
        List<TeamImportDto> rows = new()
        {
            new TeamImportDto
            {
                RowNumber = 2,
                Name = "Ecuador",
                Confederation = "CONMEBOL",
                Ranking = "700"
            },
            new TeamImportDto
            {
                RowNumber = 3,
                Name = "ecuador",
                Confederation = "CONMEBOL",
                Ranking = "701"
            }
        };

        return rows;
    }

    private static List<TeamImportDto> BuildInvalidRankingRows()
    {
        List<TeamImportDto> rows = new()
        {
            new TeamImportDto
            {
                RowNumber = 2,
                Name = "Chile",
                Confederation = "CONMEBOL",
                Ranking = "299"
            }
        };

        return rows;
    }

    private static List<TeamImportDto> BuildSingleValidRow()
    {
        List<TeamImportDto> rows = new()
        {
            new TeamImportDto
            {
                RowNumber = 2,
                Name = "Mexico",
                Confederation = "CONCACAF",
                Ranking = "800"
            }
        };

        return rows;
    }

    private static MockUnitOfWork CreateFailingUnitOfWork()
    {
        MockUnitOfWork failingUnitOfWork = new();
        failingUnitOfWork.EnableCommitFailure(new InvalidOperationException("Database unavailable"));
        return failingUnitOfWork;
    }

    private void SetupParserRows(MemoryStream stream, List<TeamImportDto> rows)
    {
        _teamParserMock.Setup(parser => parser.Parse(stream)).Returns(rows);
    }

    private void VerifyParserCall(MemoryStream stream)
    {
        _teamParserMock.Verify(parser => parser.Parse(stream), Times.Once);
    }
}
