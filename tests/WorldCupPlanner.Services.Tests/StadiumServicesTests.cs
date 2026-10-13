using WorldCupPlanner.Domain;
using WorldCupPlanner.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WorldCupPlanner.Services;
using WorldCupPlanner.Services.Tests.TestDoubles.Mocks;
using WorldCupPlanner.Services.Exceptions;
using WorldCupPlanner.Services.Interfaces;

namespace WorldCupPlanner.Services.Tests;

[TestClass]
public class StadiumServicesTests
{
    private MockStadiumRepository _repository = null!;
    private MockUnitOfWork _unitOfWork = null!;
    private StadiumServices _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _repository = new MockStadiumRepository();
        _unitOfWork = new MockUnitOfWork();
        _service = new StadiumServices(_repository, _unitOfWork, new MockAuditLogService());
    }

    
    [TestMethod]
    public void AddStadiumCallsRepositoryAndCommits()
    {
        MockStadiumRepository repository = new MockStadiumRepository();
        MockUnitOfWork unitOfWork = new MockUnitOfWork();
        StadiumServices localService = new StadiumServices(repository, unitOfWork, new MockAuditLogService());
        Stadium stadium = new Stadium("Centenario", "Montevideo", null, 60000);

        localService.AddStadium(stadium);

        Assert.AreEqual(1, repository.AddCalls);
        Assert.AreEqual(1, unitOfWork.CommitCalls);
    }

    [TestMethod]
    public void CannotAddDuplicateStadiumNameIgnoringCaseAndAccents()
    {
        _service.AddStadium(new Stadium("Estadio Azteca", "Ciudad de Mexico", null, 87000) { Id = 1 });

        Assert.ThrowsException<StadiumServicesException>(
            () => _service.AddStadium(new Stadium("ESTADIO AZTECA", "Otra Ciudad", null, 70000) { Id = 2 }));
    }

    [TestMethod]
    public void DI_RegistersStadiumServicesAsSameInstance()
    {
        ServiceCollection services = new ServiceCollection();
        services.AddSingleton<IStadiumRepository, MockStadiumRepository>();
        services.AddSingleton<IUnitOfWork, MockUnitOfWork>();
        services.AddSingleton<IAuditLogService, MockAuditLogService>();
        services.AddSingleton<StadiumServices>();
        services.AddSingleton<IStadiumService>(sp => sp.GetRequiredService<StadiumServices>());

        ServiceProvider sp = services.BuildServiceProvider();

        Assert.AreSame(
            sp.GetRequiredService<StadiumServices>(),
            sp.GetRequiredService<IStadiumService>());
    }

    [TestMethod]
    public void CanAddStadium()
    {
        Stadium stadium = new Stadium(
            "Centenario",
            "Montevideo",
            null,
            60000
        );

        _service.AddStadium(stadium);

        Assert.AreEqual(1, _service.GetAllStadiums().Count);
    }

    [TestMethod]
    public void AddStadium_LogsAuditEntry()
    {
        MockAuditLogRepository repository = new MockAuditLogRepository();
        StadiumServices auditedService = CreateAuditedService(repository);
        Stadium stadium = new Stadium("Centenario", "Montevideo", null, 60000);

        auditedService.AddStadium(stadium);

        List<AuditLog> logs = repository.GetAll();
        Assert.AreEqual(1, logs.Count);
        Assert.AreEqual("CREATE_STADIUM", logs[0].Action);
    }

    [TestMethod]
    public void UpdateStadium_LogsAuditEntry()
    {
        MockAuditLogRepository repository = new MockAuditLogRepository();
        StadiumServices auditedService = CreateAuditedService(repository);
        Stadium stadium = new Stadium("Centenario", "Montevideo", null, 60000);

        auditedService.AddStadium(stadium);
        int stadiumId = auditedService.GetByName("Centenario").Id;
        auditedService.UpdateById(stadiumId, "Montevideo Nuevo", "Estadio remodelado", 65000);

        Assert.IsTrue(repository.GetAll().Any(log => log.Action == "UPDATE_STADIUM"));
    }

    [TestMethod]
    public void RemoveStadium_LogsAuditEntry()
    {
        MockAuditLogRepository repository = new MockAuditLogRepository();
        StadiumServices auditedService = CreateAuditedService(repository);
        Stadium stadium = new Stadium("Centenario", "Montevideo", null, 60000);

        auditedService.AddStadium(stadium);
        int stadiumId = auditedService.GetByName("Centenario").Id;
        auditedService.RemoveById(stadiumId);

        Assert.IsTrue(repository.GetAll().Any(log => log.Action == "DELETE_STADIUM"));
    }

    
    [TestMethod]
    public void CannotAddTwoStadiumsWithSameName()
    {
        Stadium s1 = new Stadium("Centenario", "Montevideo", null, 60000);
        Stadium s2 = new Stadium("Centenario", "Otra", null, 50000);

        _service.AddStadium(s1);
        Assert.ThrowsException<StadiumServicesException>(() => _service.AddStadium(s2));
    }

    
    [TestMethod]
    public void AddStadium_WhenIdNotProvided_AssignsSequentialIds()
    {
        // La UI crea estadios sin Id (default 0). Como la clave es ValueGeneratedNever,
        // dos estadios con Id 0 violarian la PK; AddStadium debe asignar Id = max+1.
        _service.AddStadium(new Stadium("Centenario", "Montevideo", null, 60000));
        _service.AddStadium(new Stadium("Maracana", "Rio", null, 78000));

        List<Stadium> all = _service.GetAllStadiums().ToList();
        Assert.AreEqual(2, all.Count);
        List<int> ids = all.Select(stadium => stadium.Id).ToList();
        Assert.AreEqual(2, ids.Distinct().Count());
        Assert.IsFalse(ids.Contains(0));
    }

    [TestMethod]
    public void AddStadium_AssignsIdAfterTheMaximumExistingId()
    {
        _service.AddStadium(new Stadium("Wembley", "London", null, 90000) { Id = 5 });

        _service.AddStadium(new Stadium("Centenario", "Montevideo", null, 60000));

        Stadium added = _service.GetByName("Centenario");
        Assert.AreEqual(6, added.Id);
    }

    [TestMethod]
    public void CanGetStadiumByName()
    {
        Stadium stadium = new Stadium(
            "Centenario",
            "Montevideo",
            null,
            60000
        );

        _service.AddStadium(stadium);

        Stadium result = _service.GetByName("Centenario");

        Assert.AreEqual(stadium, result);
    }

    
    
    [TestMethod]
    public void GetStadiumByNameThrowsIfNotFound()
    {
        Assert.ThrowsException<StadiumServicesException>(() => _service.GetByName("NoExiste"));
    }

    
    [TestMethod]
    public void CanRemoveStadium()
    {
        Stadium stadium = new Stadium(
            "Centenario",
            "Montevideo",
            null,
            60000
        );

        _service.AddStadium(stadium);
        int stadiumId = _service.GetByName("Centenario").Id;
        _service.RemoveById(stadiumId);

        Assert.AreEqual(0, _service.GetAllStadiums().Count);
    }

   
    [TestMethod]
    public void CannotRemoveNonExistingStadium()
    {
        Assert.ThrowsException<StadiumServicesException>(() => _service.RemoveById(-1));
    }

    [TestMethod]
    public void StadiumServices_RemoveByIdDeletesViaRepository()
    {
        MockStadiumRepository repository = new MockStadiumRepository();
        MockUnitOfWork unitOfWork = new MockUnitOfWork();
        StadiumServices localService = new StadiumServices(repository, unitOfWork, new MockAuditLogService());
        Stadium stadium = new Stadium("Centenario", "Montevideo", null, 60000);

        localService.AddStadium(stadium);
        int stadiumId = localService.GetByName("Centenario").Id;
        localService.RemoveById(stadiumId);

        Assert.AreEqual(1, repository.DeleteCalls);
        Assert.AreEqual(2, unitOfWork.CommitCalls);
    }

     
    [TestMethod]
    public void CanUpdateStadium()
    {
        Stadium stadium = new Stadium(
            "Centenario",
            "Montevideo",
            null,
            60000
        );

        _service.AddStadium(stadium);

        int stadiumId = _service.GetByName("Centenario").Id;
        _service.UpdateById(
            stadiumId,
            "Montevideo Nuevo",
            "Estadio remodelado",
            65000
        );

        Stadium updated = _service.GetByName("Centenario");

        Assert.AreEqual("Montevideo Nuevo", updated.City);
        Assert.AreEqual(65000, updated.Capacity);
    }

    [TestMethod]
    public void StadiumServices_UpdateByIdPersistsViaRepository()
    {
        MockStadiumRepository repository = new MockStadiumRepository();
        MockUnitOfWork unitOfWork = new MockUnitOfWork();
        StadiumServices localService = new StadiumServices(repository, unitOfWork, new MockAuditLogService());
        Stadium stadium = new Stadium("Centenario", "Montevideo", null, 60000);

        localService.AddStadium(stadium);
        int stadiumId = localService.GetByName("Centenario").Id;
        localService.UpdateById(stadiumId, "NuevaCiudad", null, 30000);

        Assert.AreEqual(1, repository.UpdateCalls);
        Assert.AreEqual(2, unitOfWork.CommitCalls);
    }

    [TestMethod]
    public void CannotUpdateNonExistingStadium()
    {
        Assert.ThrowsException<StadiumServicesException>(() =>
            _service.UpdateById(-1, "Ciudad", null, 30000));
    }

    [TestMethod]
    public void RemoveById_WhenStadiumReferencedByMatch_ThrowsStadiumServicesException()
    {
        // En SQL real el FK Restrict hace fallar el SaveChanges con DbUpdateException; el servicio
        // debe traducirla a StadiumServicesException (InMemory no aplica FKs, por eso usamos un stub).
        Stadium referenced = new Stadium("Centenario", "Montevideo", null, 60000) { Id = 7 };
        StadiumServices service = new StadiumServices(
            new DeleteFailingStadiumRepository(referenced),
            new MockUnitOfWork(),
            new MockAuditLogService());

        Assert.ThrowsException<StadiumServicesException>(() => service.RemoveById(7));
    }

    [TestMethod]
    public void AddStadium_RecordsPerformingActor()
    {
        MockAuditLogRepository auditRepo = new MockAuditLogRepository();
        StadiumServices auditedService = CreateAuditedService(auditRepo);

        auditedService.AddStadium(new Stadium("Centenario", "Montevideo", null, 60000), performedByEmail: "admin@copa.com");

        AuditLog log = auditRepo.GetAll().Single(l => l.Action == "CREATE_STADIUM");
        Assert.AreEqual("admin@copa.com", log.PerformedByEmail);
    }

    [TestMethod]
    public void UpdateById_RecordsPerformingActor()
    {
        MockAuditLogRepository auditRepo = new MockAuditLogRepository();
        StadiumServices auditedService = CreateAuditedService(auditRepo);
        auditedService.AddStadium(new Stadium("Centenario", "Montevideo", null, 60000));
        int stadiumId = auditedService.GetByName("Centenario").Id;

        auditedService.UpdateById(stadiumId, "Montevideo Nuevo", null, 65000, performedByEmail: "admin@copa.com");

        AuditLog log = auditRepo.GetAll().Single(l => l.Action == "UPDATE_STADIUM");
        Assert.AreEqual("admin@copa.com", log.PerformedByEmail);
    }

    [TestMethod]
    public void RemoveById_RecordsPerformingActor()
    {
        MockAuditLogRepository auditRepo = new MockAuditLogRepository();
        StadiumServices auditedService = CreateAuditedService(auditRepo);
        auditedService.AddStadium(new Stadium("Centenario", "Montevideo", null, 60000));
        int stadiumId = auditedService.GetByName("Centenario").Id;

        auditedService.RemoveById(stadiumId, performedByEmail: "admin@copa.com");

        AuditLog log = auditRepo.GetAll().Single(l => l.Action == "DELETE_STADIUM");
        Assert.AreEqual("admin@copa.com", log.PerformedByEmail);
    }

    private static StadiumServices CreateAuditedService(MockAuditLogRepository repository)
    {
        return new StadiumServices(
            new MockStadiumRepository(),
            new MockUnitOfWork(),
            AuditLogService.CreateInstance(repository));
    }

    // Repositorio que simula el rechazo del FK (estadio referenciado por un partido) al borrar.
    private sealed class DeleteFailingStadiumRepository(Stadium stadium) : IStadiumRepository
    {
        public void Add(Stadium toAdd) { }
        public void Update(Stadium toUpdate) { }
        public void Delete(Stadium toDelete) => throw new DbUpdateException("FK violation: stadium referenced by a match.");
        public Stadium? GetById(int stadiumId) => stadiumId == stadium.Id ? stadium : null;
        public Stadium? GetByName(string name) => stadium.Name == name ? stadium : null;
        public bool ExistsByName(string name) => stadium.Name == name;
        public IReadOnlyCollection<Stadium> GetAll() => new[] { stadium };
    }

    [TestMethod]
    public void SeedDefaultStadiums_FromEmpty_AddsAtLeastFourStadiums()
    {
        _service.SeedDefaultStadiums(performedByEmail: null);

        Assert.IsTrue(_service.GetAllStadiums().Count >= 4);
    }

    [TestMethod]
    public void SeedDefaultStadiums_FromEmpty_StadiumsHaveValidCapacity()
    {
        _service.SeedDefaultStadiums(performedByEmail: null);

        Assert.IsTrue(_service.GetAllStadiums().All(stadium => stadium.Capacity >= 20000));
    }

    [TestMethod]
    public void SeedDefaultStadiums_WhenFourOrMoreExist_DoesNothing()
    {
        _service.AddStadium(new Stadium("Centenario", "Montevideo", null, 60000) { Id = 1 });
        _service.AddStadium(new Stadium("Maracana", "Rio", null, 78000) { Id = 2 });
        _service.AddStadium(new Stadium("Azteca", "Mexico", null, 80000) { Id = 3 });
        _service.AddStadium(new Stadium("Wembley", "London", null, 90000) { Id = 4 });
        int countBefore = _service.GetAllStadiums().Count;

        _service.SeedDefaultStadiums(performedByEmail: null);

        Assert.AreEqual(countBefore, _service.GetAllStadiums().Count);
    }

    [TestMethod]
    public void SeedDefaultStadiums_WhenSomeExist_FillsUpToFour()
    {
        _service.AddStadium(new Stadium("Centenario", "Montevideo", null, 60000) { Id = 1 });
        _service.AddStadium(new Stadium("Maracana", "Rio", null, 78000) { Id = 2 });

        _service.SeedDefaultStadiums(performedByEmail: null);

        Assert.IsTrue(_service.GetAllStadiums().Count >= 4);
    }

    [TestMethod]
    public void SeedDefaultStadiums_AuditsCreateStadiumPerGeneratedStadium()
    {
        MockAuditLogRepository auditRepo = new MockAuditLogRepository();
        StadiumServices auditedService = new StadiumServices(
            new MockStadiumRepository(),
            new MockUnitOfWork(),
            AuditLogService.CreateInstance(auditRepo));

        auditedService.SeedDefaultStadiums(performedByEmail: "admin@copa.com");

        int createLogs = auditRepo.GetAll().Count(log => log.Action == AuditActions.CreateStadium);
        Assert.IsTrue(createLogs >= 4);
    }
}
