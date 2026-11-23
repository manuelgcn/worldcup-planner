using WorldCupPlanner.Domain;

namespace WorldCupPlanner.Domain.Tests;
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

    [TestMethod]
    public void StadiumNameCannotExceedEightyCharacters()
    {
        string longName = new string('N', 81);

        Assert.ThrowsException<ArgumentException>(() =>
            new Stadium(longName, "Ciudad", "Descripcion", 30000));
    }

    [TestMethod]
    public void StadiumCityCannotBeEmpty()
    {
        Assert.ThrowsException<ArgumentException>(() =>
            new Stadium("Estadio", "", "Descripcion", 30000));
    }

    [TestMethod]
    public void StadiumCityCannotExceedSixtyCharacters()
    {
        string longCity = new string('C', 61);

        Assert.ThrowsException<ArgumentException>(() =>
            new Stadium("Estadio", longCity, "Descripcion", 30000));
    }

    [TestMethod]
    public void StadiumDescriptionCannotExceedFourHundredCharacters()
    {
        string longDescription = new string('D', 401);

        Assert.ThrowsException<ArgumentException>(() =>
            new Stadium("Estadio", "Ciudad", longDescription, 30000));
    }

    [TestMethod]
    public void StadiumUpdateRejectsDescriptionLongerThanFourHundredCharacters()
    {
        Stadium stadium = new Stadium("Estadio", "Ciudad", "Descripcion", 30000);
        string longDescription = new string('D', 401);

        Assert.ThrowsException<ArgumentException>(() =>
            stadium.Update("Ciudad", longDescription, 30000));
    }

    [DataTestMethod]
    [DataRow("Estadio\nCentenario")]
    [DataRow("Estadio\u0000")]
    public void StadiumNameCannotHaveControlCharacters(string name)
    {
        Assert.ThrowsException<ArgumentException>(() =>
            new Stadium(name, "Ciudad", "Descripcion", 30000));
    }

    [DataTestMethod]
    [DataRow("---")]
    [DataRow("​")]
    public void StadiumNameMustHaveALetterOrDigit(string name)
    {
        // Solo signos se normaliza a "": la unicidad por nombre normalizado los trataria como el mismo estadio.
        Assert.ThrowsException<ArgumentException>(() =>
            new Stadium(name, "Ciudad", "Descripcion", 30000));
    }

    [TestMethod]
    public void StadiumCityCannotHaveControlCharacters()
    {
        Assert.ThrowsException<ArgumentException>(() =>
            new Stadium("Estadio", "Monte\tvideo", "Descripcion", 30000));
    }

    [TestMethod]
    public void StadiumUpdateRejectsCityWithControlCharacters()
    {
        Stadium stadium = new Stadium("Estadio", "Ciudad", "Descripcion", 30000);

        Assert.ThrowsException<ArgumentException>(() =>
            stadium.Update("Ciu\ndad", "Descripcion", 30000));
    }

    [TestMethod]
    public void StadiumAcceptsSignsInsideARealNameAndCity()
    {
        Stadium stadium = new Stadium("AT&T Stadium", "St. John's", null, 80000);

        Assert.AreEqual("AT&T Stadium", stadium.Name);
        Assert.AreEqual("St. John's", stadium.City);
    }

    [TestMethod]
    public void StadiumAcceptsDescriptionOfFourHundredCharacters()
    {
        string description = new string('D', 400);

        Stadium stadium = new Stadium("Estadio", "Ciudad", description, 30000);

        Assert.AreEqual(description, stadium.Description);
    }


}
