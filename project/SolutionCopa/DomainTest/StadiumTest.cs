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
    public void StadiumCapacityMustBeAtLeast20000()
    {
        Assert.ThrowsException<ArgumentException>(() => 
            new Stadium("Estadio Chico", "Ciudad", "Muy pequeno", 15000));
    }

    
    [TestMethod]
    public void StadiumNameCannotBeEmpty()
    {
        Assert.ThrowsException<ArgumentException>(() => 
            new Stadium("", "Ciudad", "Descripcion", 30000));
    }

    
}