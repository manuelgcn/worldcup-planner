using WorldCupPlanner.DataAccess.Interfaces;
using Moq;
using WorldCupPlanner.Domain;
using WorldCupPlanner.Services;
using WorldCupPlanner.Services.DTOs;
using WorldCupPlanner.Services.Exceptions;
using WorldCupPlanner.Services.Interfaces;
using WorldCupPlanner.Services.Tests.TestDoubles.Mocks;
// ReSharper disable UseCollectionExpression

namespace WorldCupPlanner.Services.Tests;

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
        _teamRepository = new MockTeamRepository();
        _unitOfWork = new MockUnitOfWork();
        _teamParserMock = new Mock<ITeamParser>();
        _service = CreateService(_teamRepository, _unitOfWork, _teamParserMock.Object);
    }

    [TestMethod]
    public void ImportTeamsCreatesTeamsAndCommits()
    {
        // Arrange
        MemoryStream stream = CreateImportStream();
        List<TeamImportDto> rows = BuildValidRows();
        SetupParserRows(stream, rows);

        // Act
        _service.ImportTeams(stream);

        // Assert
        Assert.AreEqual(2, _teamRepository.GetAll().Count);
        Assert.AreEqual(1, _unitOfWork.CommitCalls);
    }

    [TestMethod]
    public void ImportTeamsThrowsWhenStreamIsNull()
    {
        Assert.ThrowsException<ArgumentNullException>(() => _service.ImportTeams(null!));
    }

    [TestMethod]
    public void ImportTeamsThrowsWhenTeamNameIsEmpty()
    {
        // Arrange
        MemoryStream stream = CreateImportStream();
        List<TeamImportDto> rows = BuildEmptyNameRows();
        SetupParserRows(stream, rows);

        // Assert
        Assert.ThrowsException<ImportServiceException>(() => _service.ImportTeams(stream));
        Assert.AreEqual(0, _unitOfWork.CommitCalls);
    }

    [TestMethod]
    public void ImportTeamsThrowsWhenTeamNameIsTooLong()
    {
        // Arrange
        MemoryStream stream = CreateImportStream();
        List<TeamImportDto> rows = BuildTooLongNameRows();
        SetupParserRows(stream, rows);

        // Assert
        Assert.ThrowsException<ImportServiceException>(() => _service.ImportTeams(stream));
        Assert.AreEqual(0, _teamRepository.GetAll().Count);
    }

    [TestMethod]
    public void ImportTeamsThrowsWhenConfederationIsInvalid()
    {
        // Arrange
        MemoryStream stream = CreateImportStream();
        List<TeamImportDto> rows = BuildInvalidConfederationRows();
        SetupParserRows(stream, rows);

        // Assert
        Assert.ThrowsException<ImportServiceException>(() => _service.ImportTeams(stream));
        Assert.AreEqual(0, _unitOfWork.CommitCalls);
    }

    [TestMethod]
    public void ImportTeamsThrowsWhenRankingIsNotNumeric()
    {
        // Arrange
        MemoryStream stream = CreateImportStream();
        List<TeamImportDto> rows = BuildNonNumericRankingRows();
        SetupParserRows(stream, rows);

        // Assert
        Assert.ThrowsException<ImportServiceException>(() => _service.ImportTeams(stream));
        Assert.AreEqual(0, _teamRepository.GetAll().Count);
    }

    [TestMethod]
    public void ImportTeamsThrowsWhenRankingIsOutOfRange()
    {
        // Arrange
        MemoryStream stream = CreateImportStream();
        List<TeamImportDto> rows = BuildOutOfRangeRankingRows();
        SetupParserRows(stream, rows);

        // Assert
        Assert.ThrowsException<ImportServiceException>(() => _service.ImportTeams(stream));
        Assert.AreEqual(0, _unitOfWork.CommitCalls);
    }

    [TestMethod]
    public void ImportTeamsThrowsWhenDuplicateNameAppearsInSameFile()
    {
        // Arrange
        MemoryStream stream = CreateImportStream();
        List<TeamImportDto> rows = BuildDuplicateRows();
        SetupParserRows(stream, rows);

        // Assert: import atomico (H4): si una fila falla, no se persiste NINGUNA, ni siquiera
        // la primera valida que la precede.
        Assert.ThrowsException<ImportServiceException>(() => _service.ImportTeams(stream));
        Assert.AreEqual(0, _teamRepository.GetAll().Count);
        Assert.AreEqual(0, _unitOfWork.CommitCalls);
    }

    [TestMethod]
    public void ImportTeams_WhenLaterRowIsInvalid_PersistsNothing()
    {
        // H4: una fila valida seguida de una invalida no debe dejar escrituras parciales.
        MemoryStream stream = CreateImportStream();
        List<TeamImportDto> rows = new List<TeamImportDto>
        {
            new TeamImportDto { RowNumber = 2, Name = "Argentina", Confederation = "CONMEBOL", Ranking = "500" },
            new TeamImportDto { RowNumber = 3, Name = "Canada", Confederation = "INVALID", Ranking = "800" }
        };
        SetupParserRows(stream, rows);

        Assert.ThrowsException<ImportServiceException>(() => _service.ImportTeams(stream));
        Assert.AreEqual(0, _teamRepository.GetAll().Count);
        Assert.AreEqual(0, _teamRepository.AddCalls);
        Assert.AreEqual(0, _unitOfWork.CommitCalls);
    }

    [TestMethod]
    public void ImportTeamsThrowsWhenRepositoryAlreadyHasSameTeamName()
    {
        // Arrange
        Team existingTeam = new Team(10, "Brazil", 900, false, Confederation.CONMEBOL);
        _teamRepository.Add(existingTeam);

        MemoryStream stream = CreateImportStream();
        List<TeamImportDto> rows = BuildRepositoryDuplicateRows();
        SetupParserRows(stream, rows);

        // Assert
        Assert.ThrowsException<ImportServiceException>(() => _service.ImportTeams(stream));
        Assert.AreEqual(1, _teamRepository.GetAll().Count);
    }

    [TestMethod]
    public void ImportTeamsUsesNextIdWhenRepositoryAlreadyHasTeams()
    {
        // Arrange
        Team existingTeam = new Team(10, "Brazil", 900, false, Confederation.CONMEBOL);
        _teamRepository.Add(existingTeam);

        MemoryStream stream = CreateImportStream();
        List<TeamImportDto> rows = BuildSingleValidRow();
        SetupParserRows(stream, rows);

        // Act
        _service.ImportTeams(stream);
        Team? importedTeam = _teamRepository.GetByName("Mexico");

        // Assert
        Assert.IsNotNull(importedTeam);
        Assert.AreEqual(11, importedTeam.Id);
    }

    [TestMethod]
    public void ImportTeamsReturnsResultWithImportedTeamsAndCount()
    {
        MemoryStream stream = CreateImportStream();
        List<TeamImportDto> rows = BuildValidRows();
        SetupParserRows(stream, rows);

        ImportResult result = _service.ImportTeams(stream);

        Assert.AreEqual(2, result.ImportedCount);
        CollectionAssert.AreEquivalent(
            new List<string> { "Argentina", "Japan" },
            result.ImportedTeams.Select(t => t.Name).ToList());
    }

    [TestMethod]
    public void ImportTeamsWhenTotalWouldExceed48ImportsOnlyUntil48AndReturnsWarning()
    {
        // Arrange
        for (int i = 1; i <= 47; i++)
        {
            _teamRepository.Add(new Team(i, $"Team{i}", 1000 + i, false, Confederation.UEFA));
        }

        MemoryStream stream = CreateImportStream();
        List<TeamImportDto> rows = BuildValidRows();
        SetupParserRows(stream, rows);

        // Act
        ImportResult result = _service.ImportTeams(stream);

        // Assert
        Assert.AreEqual(48, _teamRepository.GetAll().Count);
        Assert.AreEqual(1, result.ImportedCount);
        Assert.AreEqual("Argentina", result.ImportedTeams.First().Name);
        Assert.AreEqual("No se agregaron todos los equipos, max:48", result.WarningMessage);
    }

    [TestMethod]
    public void ImportTeamsPropagatesUnitOfWorkFailure()
    {
        // Arrange
        MockUnitOfWork failingUnitOfWork = CreateFailingUnitOfWork();
        ImportService service = CreateService(_teamRepository, failingUnitOfWork, _teamParserMock.Object);

        MemoryStream stream = CreateImportStream();
        List<TeamImportDto> rows = BuildSingleValidRow();
        SetupParserRows(stream, rows);

        // Assert
        Assert.ThrowsException<InvalidOperationException>(() => service.ImportTeams(stream));
        Assert.AreEqual(1, failingUnitOfWork.CommitCalls);
    }

    private static ImportService CreateService(ITeamRepository teamRepository, IUnitOfWork unitOfWork, ITeamParser teamParser)
    {
        return ImportService.CreateInstance(teamRepository, unitOfWork, teamParser);
    }

    private static MemoryStream CreateImportStream()
    {
        return new MemoryStream();
    }

    private void SetupParserRows(MemoryStream stream, List<TeamImportDto> rows)
    {
        _teamParserMock.Setup(parser => parser.Parse(stream)).Returns(rows);
    }

    private static List<TeamImportDto> BuildValidRows()
    {
        List<TeamImportDto> rows = new List<TeamImportDto>
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

    private static List<TeamImportDto> BuildEmptyNameRows()
    {
        List<TeamImportDto> rows = new List<TeamImportDto>
        {
            new TeamImportDto
            {
                RowNumber = 2,
                Name = string.Empty,
                Confederation = "CONCACAF",
                Ranking = "800"
            }
        };

        return rows;
    }

    private static List<TeamImportDto> BuildTooLongNameRows()
    {
        string longName = new string('A', 61);
        List<TeamImportDto> rows = new List<TeamImportDto>
        {
            new TeamImportDto
            {
                RowNumber = 2,
                Name = longName,
                Confederation = "CONCACAF",
                Ranking = "800"
            }
        };

        return rows;
    }

    private static List<TeamImportDto> BuildInvalidConfederationRows()
    {
        List<TeamImportDto> rows = new List<TeamImportDto>
        {
            new TeamImportDto
            {
                RowNumber = 2,
                Name = "Canada",
                Confederation = "INVALID",
                Ranking = "800"
            }
        };

        return rows;
    }

    private static List<TeamImportDto> BuildNonNumericRankingRows()
    {
        List<TeamImportDto> rows = new List<TeamImportDto>
        {
            new TeamImportDto
            {
                RowNumber = 2,
                Name = "Canada",
                Confederation = "CONCACAF",
                Ranking = "ABC"
            }
        };

        return rows;
    }

    private static List<TeamImportDto> BuildOutOfRangeRankingRows()
    {
        List<TeamImportDto> rows = new List<TeamImportDto>
        {
            new TeamImportDto
            {
                RowNumber = 2,
                Name = "Canada",
                Confederation = "CONCACAF",
                Ranking = "299"
            }
        };

        return rows;
    }

    private static List<TeamImportDto> BuildDuplicateRows()
    {
        List<TeamImportDto> rows = new List<TeamImportDto>
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

    private static List<TeamImportDto> BuildRepositoryDuplicateRows()
    {
        List<TeamImportDto> rows = new List<TeamImportDto>
        {
            new TeamImportDto
            {
                RowNumber = 2,
                Name = "Brazil",
                Confederation = "CONMEBOL",
                Ranking = "800"
            }
        };

        return rows;
    }

    private static List<TeamImportDto> BuildSingleValidRow()
    {
        List<TeamImportDto> rows = new List<TeamImportDto>
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
        MockUnitOfWork failingUnitOfWork = new MockUnitOfWork();
        failingUnitOfWork.EnableCommitFailure(new InvalidOperationException("Database unavailable"));
        return failingUnitOfWork;
    }
}
