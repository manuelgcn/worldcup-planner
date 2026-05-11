using ProjectCopa;
using DataAccess.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Services;
using ServiceTest.TestDoubles.Mocks;
using Services.Exceptions;
using Services.Interfaces;

namespace ServiceTest;

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
        _service = new StadiumServices(_repository, _unitOfWork);
    }

    
    [TestMethod]
    public void AddStadiumCallsRepositoryAndCommits()
    {
        MockStadiumRepository repository = new MockStadiumRepository();
        MockUnitOfWork unitOfWork = new MockUnitOfWork();
        StadiumServices localService = new StadiumServices(repository, unitOfWork);
        Stadium stadium = new Stadium("Centenario", "Montevideo", null, 60000);

        localService.AddStadium(stadium);

        Assert.AreEqual(1, repository.AddCalls);
        Assert.AreEqual(1, unitOfWork.CommitCalls);
    }

    [TestMethod]
    public void DI_RegistersStadiumServicesAsSameInstance()
    {
        ServiceCollection services = new ServiceCollection();
        services.AddSingleton<IStadiumRepository, MockStadiumRepository>();
        services.AddSingleton<IUnitOfWork, MockUnitOfWork>();
        services.AddSingleton<IStadiumService, StadiumServices>();
        services.AddSingleton<StadiumServices>();

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
        StadiumServices localService = new StadiumServices(repository, unitOfWork);
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
        StadiumServices localService = new StadiumServices(repository, unitOfWork);
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

    private static StadiumServices CreateAuditedService(MockAuditLogRepository repository)
    {
        return new StadiumServices(
            new MockStadiumRepository(),
            new MockUnitOfWork(),
            AuditLogService.CreateInstance(repository, 1));
    }
}
