using ProjectCopa;

namespace DomainTest;
[TestClass]
public class StadiumTest
{
    [TestMethod]
    public void StadiumCanBeCreatedWithValidData()
    {
        Stadium stadium = new Stadium(
            "Estadio Centenario",
            "Montevideo",
            "Estadio historico",
            60000
        );

        Assert.AreEqual("Estadio Centenario", stadium.Name);
        Assert.AreEqual("Montevideo", stadium.City);
        Assert.AreEqual(60000, stadium.Capacity);
    }

    
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void StadiumCapacityMustBeAtLeast20000()
    {
        new Stadium(
            "Estadio Chico",
            "Ciudad",
            "Muy pequeno",
            15000
        );
    }

    
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void StadiumNameCannotBeEmpty()
    {
        new Stadium(
            "",
            "Ciudad",
            "Descripcion",
            30000
        );
    }

    
}