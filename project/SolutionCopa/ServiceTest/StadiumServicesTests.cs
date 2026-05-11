using ProjectCopa;
using Services;
using ServiceTest.TestDoubles.Mocks;
using DataAccess.Interfaces;
using DataAccess.Repositories;
using Services.Exceptions;

namespace ServiceTest;

[TestClass]
public class StadiumServicesTests
{
    
    private StadiumServices service = null!;

    [TestInitialize]
    public void Setup()
    {
        service = new StadiumServices(new StadiumRepository(), new UnitOfWorkRepository());
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
    public void CanAddStadium()
    {
        Stadium stadium = new Stadium(
            "Centenario",
            "Montevideo",
            null,
            60000
        );

        service.AddStadium(stadium);

        Assert.AreEqual(1, service.GetAllStadiums().Count);
    }

    [TestMethod]
    public void AddStadium_LogsAuditEntry()
    {
        MockAuditLogRepository repository = new MockAuditLogRepository();
        StadiumServices auditedService = new StadiumServices(
            new StadiumRepository(),
            new UnitOfWorkRepository(),
            AuditLogService.CreateInstance(repository, 1));
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
        StadiumServices auditedService = new StadiumServices(
            new StadiumRepository(),
            new UnitOfWorkRepository(),
            AuditLogService.CreateInstance(repository, 1));
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
        StadiumServices auditedService = new StadiumServices(
            new StadiumRepository(),
            new UnitOfWorkRepository(),
            AuditLogService.CreateInstance(repository, 1));
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

        service.AddStadium(s1);
        Assert.ThrowsException<StadiumServicesException>(() => service.AddStadium(s2));
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

        service.AddStadium(stadium);

        Stadium result = service.GetByName("Centenario");

        Assert.AreEqual(stadium, result);
    }

    
    
    [TestMethod]
    public void GetStadiumByNameThrowsIfNotFound()
    {
        Assert.ThrowsException<StadiumServicesException>(() => service.GetByName("NoExiste"));
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

        service.AddStadium(stadium);
        int stadiumId = service.GetByName("Centenario").Id;
        service.RemoveById(stadiumId);

        Assert.AreEqual(0, service.GetAllStadiums().Count);
    }

   
    [TestMethod]
    public void CannotRemoveNonExistingStadium()
    {
        Assert.ThrowsException<StadiumServicesException>(() => service.RemoveById(-1));
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

        service.AddStadium(stadium);

        int stadiumId = service.GetByName("Centenario").Id;
        service.UpdateById(
            stadiumId,
            "Montevideo Nuevo",
            "Estadio remodelado",
            65000
        );

        Stadium updated = service.GetByName("Centenario");

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
            service.UpdateById(-1, "Ciudad", null, 30000));
    }

    
    
}
