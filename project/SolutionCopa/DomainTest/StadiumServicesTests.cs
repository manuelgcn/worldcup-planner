using ProjectCopa;
using Services;

namespace DomainTest;

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
    [ExpectedException(typeof(ArgumentException))]
    public void CannotAddTwoStadiumsWithSameName()
    {
        Stadium s1 = new Stadium("Centenario", "Montevideo", null, 60000);
        Stadium s2 = new Stadium("Centenario", "Otra", null, 50000);

        service.AddStadium(s1);
        service.AddStadium(s2);
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
    [ExpectedException(typeof(ArgumentException))]
    public void GetStadiumByNameThrowsIfNotFound()
    {
        service.GetByName("NoExiste");
    }

    
}