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

}