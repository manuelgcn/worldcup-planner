using ProjectCopa;
using Services;
using ServiceTest.TestDoubles.Mocks;

namespace ServiceTest;

[TestClass]
public class StadiumServicesTests
{
    
    private StadiumServices service;

    [TestInitialize]
    public void Setup()
    {
        service = new StadiumServices();
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
        StadiumServices auditedService = new StadiumServices(Services.AuditLogService.CreateInstance(repository));
        Stadium stadium = new Stadium("Centenario", "Montevideo", null, 60000);

        auditedService.AddStadium(stadium);

        List<ProjectCopa.AuditLog> logs = repository.GetAll();
        Assert.AreEqual(1, logs.Count);
        Assert.AreEqual("CREATE_STADIUM", logs[0].Action);
    }

    
    [TestMethod]
    public void CannotAddTwoStadiumsWithSameName()
    {
        Stadium s1 = new Stadium("Centenario", "Montevideo", null, 60000);
        Stadium s2 = new Stadium("Centenario", "Otra", null, 50000);

        service.AddStadium(s1);
        Assert.ThrowsException<ArgumentException>(() => service.AddStadium(s2));
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
        Assert.ThrowsException<ArgumentException>(() => service.GetByName("NoExiste"));
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
        service.Remove("Centenario");

        Assert.AreEqual(0, service.GetAllStadiums().Count);
    }

   
    [TestMethod]
    public void CannotRemoveNonExistingStadium()
    {
        Assert.ThrowsException<ArgumentException>(() => service.Remove("NoExiste"));
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

        service.Update(
            "Centenario",
            "Montevideo Nuevo",
            "Estadio remodelado",
            65000
        );

        Stadium updated = service.GetByName("Centenario");

        Assert.AreEqual("Montevideo Nuevo", updated.City);
        Assert.AreEqual(65000, updated.Capacity);
    }

    [TestMethod]
    public void CannotUpdateNonExistingStadium()
    {
        Assert.ThrowsException<ArgumentException>(() =>
            service.Update("NoExiste", "Ciudad", null, 30000));
    }

    
    
}
