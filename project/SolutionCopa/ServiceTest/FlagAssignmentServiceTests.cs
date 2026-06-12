using ProjectCopa;
using Services;
using Services.DTOs;
using ServiceTest.TestDoubles.Mocks;

namespace ServiceTest;

[TestClass]
public sealed class FlagAssignmentServiceTests
{
    private MockTeamRepository _teamRepository = null!;
    private MockUnitOfWork _unitOfWork = null!;
    private MockAuditLogRepository _auditLogRepository = null!;
    private FlagAssignmentService _service = null!;
    private string _flagsDirectory = null!;

    [TestInitialize]
    public void Setup()
    {
        _flagsDirectory = Path.Combine(Path.GetTempPath(), "FlagAssignmentTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_flagsDirectory);

        _teamRepository = new MockTeamRepository();
        _unitOfWork = new MockUnitOfWork();
        _auditLogRepository = new MockAuditLogRepository();
        _service = new FlagAssignmentService(
            _teamRepository,
            _unitOfWork,
            _flagsDirectory,
            AuditLogService.CreateInstance(_auditLogRepository));
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_flagsDirectory))
        {
            Directory.Delete(_flagsDirectory, recursive: true);
        }
    }

    private void CreateFlagFile(string isoCode)
    {
        byte[] fakePngBytes = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        File.WriteAllBytes(Path.Combine(_flagsDirectory, isoCode + ".png"), fakePngBytes);
    }

    [TestMethod]
    public void AssignFlags_AssignsFlagToMappedTeam()
    {
        CreateFlagFile("UY");
        Team team = new Team(1, "Uruguay", 1680, false, Confederation.CONMEBOL);
        _teamRepository.Add(team);

        FlagAssignmentResult result = _service.AssignFlags();

        Assert.AreEqual(1, result.AssignedCount);
        Assert.IsNotNull(team.FlagBase64);
        Assert.AreEqual(1, _unitOfWork.CommitCalls);
    }

    [TestMethod]
    public void AssignFlags_MatchesTeamNamesIgnoringAccentsAndCase()
    {
        CreateFlagFile("MX");
        CreateFlagFile("JP");
        Team mexico = new Team(1, "México", 1660, false, Confederation.CONCACAF);
        Team japan = new Team(2, "JAPÓN", 1648, false, Confederation.AFC);
        _teamRepository.Add(mexico);
        _teamRepository.Add(japan);

        FlagAssignmentResult result = _service.AssignFlags();

        Assert.AreEqual(2, result.AssignedCount);
        Assert.IsNotNull(mexico.FlagBase64);
        Assert.IsNotNull(japan.FlagBase64);
    }

    [TestMethod]
    public void AssignFlags_ReportsTeamsWithoutMapping()
    {
        Team unknown = new Team(1, "Atlantis", 1500, false, Confederation.UEFA);
        _teamRepository.Add(unknown);

        FlagAssignmentResult result = _service.AssignFlags();

        Assert.AreEqual(0, result.AssignedCount);
        CollectionAssert.Contains(result.UnmappedTeamNames.ToList(), "Atlantis");
        Assert.IsNull(unknown.FlagBase64);
    }

    [TestMethod]
    public void AssignFlags_ReportsMappedTeamWhenFlagFileIsMissing()
    {
        Team team = new Team(1, "Uruguay", 1680, false, Confederation.CONMEBOL);
        _teamRepository.Add(team);

        FlagAssignmentResult result = _service.AssignFlags();

        Assert.AreEqual(0, result.AssignedCount);
        CollectionAssert.Contains(result.UnmappedTeamNames.ToList(), "Uruguay");
    }

    [TestMethod]
    public void AssignFlags_SkipsFictitiousTeams()
    {
        Team fictitious = new Team(1, "UEFA_01", 1400, true, Confederation.UEFA);
        _teamRepository.Add(fictitious);

        FlagAssignmentResult result = _service.AssignFlags();

        Assert.AreEqual(0, result.AssignedCount);
        Assert.AreEqual(0, result.UnmappedTeamNames.Count);
        Assert.IsNull(fictitious.FlagBase64);
    }

    [TestMethod]
    public void AssignFlags_DoesNotOverwriteExistingFlagByDefault()
    {
        CreateFlagFile("UY");
        string existingFlag = Convert.ToBase64String(new byte[] { 1, 2, 3 });
        Team team = new Team(1, "Uruguay", 1680, false, Confederation.CONMEBOL, existingFlag);
        _teamRepository.Add(team);

        FlagAssignmentResult result = _service.AssignFlags();

        Assert.AreEqual(0, result.AssignedCount);
        Assert.AreEqual(1, result.SkippedExistingCount);
        Assert.AreEqual(existingFlag, team.FlagBase64);
    }

    [TestMethod]
    public void AssignFlags_OverwritesExistingFlagWhenRequested()
    {
        CreateFlagFile("UY");
        string existingFlag = Convert.ToBase64String(new byte[] { 1, 2, 3 });
        Team team = new Team(1, "Uruguay", 1680, false, Confederation.CONMEBOL, existingFlag);
        _teamRepository.Add(team);

        FlagAssignmentResult result = _service.AssignFlags(overwriteExisting: true);

        Assert.AreEqual(1, result.AssignedCount);
        Assert.AreEqual(0, result.SkippedExistingCount);
        Assert.AreNotEqual(existingFlag, team.FlagBase64);
    }

    [TestMethod]
    public void AssignFlags_PersistsUpdatedTeams()
    {
        CreateFlagFile("UY");
        Team team = new Team(1, "Uruguay", 1680, false, Confederation.CONMEBOL);
        _teamRepository.Add(team);

        _service.AssignFlags();

        Assert.AreEqual(1, _teamRepository.UpdateCalls);
    }

    [TestMethod]
    public void AssignFlags_LogsAuditEntryWithActor()
    {
        CreateFlagFile("UY");
        Team team = new Team(1, "Uruguay", 1680, false, Confederation.CONMEBOL);
        _teamRepository.Add(team);

        _service.AssignFlags(overwriteExisting: false, performedByEmail: "admin@copa.com");

        List<AuditLog> logs = _auditLogRepository.GetAll();
        Assert.AreEqual(1, logs.Count);
        Assert.AreEqual(AuditActions.AssignTeamFlags, logs[0].Action);
        Assert.AreEqual("admin@copa.com", logs[0].PerformedByEmail);
    }

    [TestMethod]
    public void AssignFlags_WithoutTeamsDoesNothing()
    {
        FlagAssignmentResult result = _service.AssignFlags();

        Assert.AreEqual(0, result.AssignedCount);
        Assert.AreEqual(0, result.SkippedExistingCount);
        Assert.AreEqual(0, result.UnmappedTeamNames.Count);
    }

    [TestMethod]
    public void AssignFlags_CoversAllFortyEightRealTeamNames()
    {
        string[] realTeamNames =
        {
            "México", "Sudáfrica", "Corea del Sur", "República Checa", "Canadá",
            "Bosnia y Herzegovina", "Catar", "Suiza", "Brasil", "Marruecos",
            "Haití", "Escocia", "Estados Unidos", "Paraguay", "Australia",
            "Turquía", "Alemania", "Curazao", "Costa de Marfil", "Ecuador",
            "Países Bajos", "Japón", "Suecia", "Túnez", "Bélgica",
            "Egipto", "Irán", "Nueva Zelanda", "España", "Cabo Verde",
            "Arabia Saudita", "Uruguay", "Francia", "Senegal", "Irak",
            "Noruega", "Argentina", "Argelia", "Austria", "Jordania",
            "Portugal", "RD Congo", "Uzbekistán", "Colombia", "Inglaterra",
            "Croacia", "Ghana", "Panamá"
        };

        foreach (string isoCode in FlagAssignmentService.KnownIsoCodes)
        {
            CreateFlagFile(isoCode);
        }

        int id = 1;
        foreach (string name in realTeamNames)
        {
            _teamRepository.Add(new Team(id++, name, 1500, false, Confederation.UEFA));
        }

        FlagAssignmentResult result = _service.AssignFlags();

        Assert.AreEqual(realTeamNames.Length, result.AssignedCount);
        Assert.AreEqual(0, result.UnmappedTeamNames.Count);
    }
}
