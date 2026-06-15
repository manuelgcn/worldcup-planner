using DataAccess.Repositories;
using DataAccessTest.TestDoubles;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectCopa;

namespace DataAccessTest;

[TestClass]
public class StadiumRepositoryTests
{
    private StadiumRepository _repository = null!;

    [TestInitialize]
    public void Setup()
    {
        // Contextos efimeros sobre una BD InMemory aislada por test (mismo patron que TeamRepositoryTests).
        _repository = new StadiumRepository(new InMemoryDbContextFactory());
    }

    [TestMethod]
    public void AddStoresStadiumAndGetByIdReturnsIt()
    {
        Stadium stadium = CreateStadium(1, "Centenario", "Montevideo", 60000);

        _repository.Add(stadium);

        Stadium? storedStadium = _repository.GetById(1);
        Assert.IsNotNull(storedStadium);
        AssertSameValues(stadium, storedStadium!);
    }

    [TestMethod]
    public void UpdateReplacesExistingStadiumWithSameId()
    {
        Stadium originalStadium = CreateStadium(2, "Madrigal", "Vila-real", 23000);
        Stadium updatedStadium = CreateStadium(2, "Madrigal", "Castellon", 24000);
        _repository.Add(originalStadium);

        _repository.Update(updatedStadium);

        Stadium? storedStadium = _repository.GetById(2);
        Assert.IsNotNull(storedStadium);
        Assert.AreEqual("Castellon", storedStadium!.City);
        Assert.AreEqual(24000, storedStadium.Capacity);
        AssertSameValues(updatedStadium, storedStadium);
    }

    [TestMethod]
    public void UpdateAddsStadiumWhenIdDoesNotExist()
    {
        Stadium stadium = CreateStadium(3, "Azteca", "Ciudad de Mexico", 87000);

        _repository.Update(stadium);

        Stadium? storedStadium = _repository.GetById(3);
        Assert.IsNotNull(storedStadium);
        AssertSameValues(stadium, storedStadium!);
    }

    [TestMethod]
    public void DeleteRemovesStoredStadium()
    {
        Stadium stadium = CreateStadium(4, "Maracana", "Rio de Janeiro", 78000);
        _repository.Add(stadium);

        _repository.Delete(stadium);

        Assert.IsNull(_repository.GetById(4));
    }

    [TestMethod]
    public void GetByNameReturnsStoredStadium()
    {
        Stadium stadium = CreateStadium(5, "Wembley", "London", 90000);
        _repository.Add(stadium);

        Stadium? storedStadium = _repository.GetByName("Wembley");

        Assert.IsNotNull(storedStadium);
        AssertSameValues(stadium, storedStadium!);
    }

    [TestMethod]
    public void ExistsByNameReturnsTrueWhenStadiumIsStored()
    {
        Stadium stadium = CreateStadium(6, "Allianz Arena", "Munich", 75000);
        _repository.Add(stadium);

        bool exists = _repository.ExistsByName("Allianz Arena");

        Assert.IsTrue(exists);
    }

    [TestMethod]
    public void ExistsByNameReturnsFalseWhenStadiumIsNotStored()
    {
        bool exists = _repository.ExistsByName("Bombonera");

        Assert.IsFalse(exists);
    }

    [TestMethod]
    public void ExistsByNameIsCaseInsensitive()
    {
        _repository.Add(CreateStadium(20, "Maracana", "Rio de Janeiro", 78000));

        Assert.IsTrue(_repository.ExistsByName("maracana"));
        Assert.IsTrue(_repository.ExistsByName("MARACANA"));
    }

    [TestMethod]
    public void ExistsByNameIgnoresDiacritics()
    {
        _repository.Add(CreateStadium(21, "Estadio Azteca", "Ciudad de Mexico", 87000));

        Assert.IsTrue(_repository.ExistsByName("estadio azteca"));
    }

    [TestMethod]
    public void GetByNameIsCaseInsensitive()
    {
        Stadium stadium = CreateStadium(22, "Wembley", "London", 90000);
        _repository.Add(stadium);

        Stadium? stored = _repository.GetByName("WEMBLEY");

        Assert.IsNotNull(stored);
        Assert.AreEqual("Wembley", stored!.Name);
    }

    [TestMethod]
    public void GetAllReturnsAllStoredStadiums()
    {
        Stadium firstStadium = CreateStadium(7, "Stadium 1", "City 1", 30000);
        Stadium secondStadium = CreateStadium(8, "Stadium 2", "City 2", 32000);
        _repository.Add(firstStadium);
        _repository.Add(secondStadium);

        IReadOnlyCollection<Stadium> stadiums = _repository.GetAll();

        Assert.AreEqual(2, stadiums.Count);
        Assert.IsTrue(stadiums.Any(s => s.Id == 7 && s.Name == "Stadium 1"));
        Assert.IsTrue(stadiums.Any(s => s.Id == 8 && s.Name == "Stadium 2"));
    }

    [TestMethod]
    public void AddPersistsStadiumRetrievableFromNewContext()
    {
        Stadium stadium = CreateStadium(9, "San Siro", "Milan", 80000);
        stadium.Update("Milan", "Giuseppe Meazza", 80018);   // descripcion no nula para cubrir la columna opcional

        _repository.Add(stadium);                            // SaveChanges interno (autosave por operacion)

        Stadium? stored = _repository.GetById(9);            // contexto nuevo: el dato viene de la "BD"
        Assert.IsNotNull(stored);
        Assert.AreEqual("San Siro", stored!.Name);
        Assert.AreEqual("Milan", stored.City);
        Assert.AreEqual("Giuseppe Meazza", stored.Description);
        Assert.AreEqual(80018, stored.Capacity);
    }

    private static Stadium CreateStadium(int id, string name, string city, int capacity)
    {
        return new Stadium(name: name, city: city, description: null, capacity: capacity) { Id = id };
    }

    private static void AssertSameValues(Stadium expected, Stadium actual)
    {
        Assert.AreEqual(expected.Id, actual.Id);
        Assert.AreEqual(expected.Name, actual.Name);
        Assert.AreEqual(expected.City, actual.City);
        Assert.AreEqual(expected.Description, actual.Description);
        Assert.AreEqual(expected.Capacity, actual.Capacity);
    }
}
