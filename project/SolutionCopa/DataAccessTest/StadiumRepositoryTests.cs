using DataAccess.Repositories;
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
        _repository = new StadiumRepository();
    }

    [TestMethod]
    public void AddStoresStadiumAndGetByIdReturnsIt()
    {
        Stadium stadium = CreateStadium(1, "Centenario", "Montevideo", 60000);

        _repository.Add(stadium);

        Stadium? storedStadium = _repository.GetById(1);
        Assert.AreSame(stadium, storedStadium);
    }

    [TestMethod]
    public void UpdateReplacesExistingStadiumWithSameId()
    {
        Stadium originalStadium = CreateStadium(2, "Madrigal", "Vila-real", 23000);
        Stadium updatedStadium = CreateStadium(2, "La Ceramica", "Vila-real", 24000);
        _repository.Add(originalStadium);

        _repository.Update(updatedStadium);

        Stadium? storedStadium = _repository.GetById(2);
        Assert.IsNotNull(storedStadium);
        Assert.AreSame(updatedStadium, storedStadium);
    }

    [TestMethod]
    public void UpdateAddsStadiumWhenIdDoesNotExist()
    {
        Stadium stadium = CreateStadium(3, "Azteca", "Ciudad de Mexico", 87000);

        _repository.Update(stadium);

        Assert.AreSame(stadium, _repository.GetById(3));
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

        Assert.AreSame(stadium, storedStadium);
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
    public void GetAllReturnsAllStoredStadiums()
    {
        Stadium firstStadium = CreateStadium(7, "Stadium 1", "City 1", 30000);
        Stadium secondStadium = CreateStadium(8, "Stadium 2", "City 2", 32000);
        _repository.Add(firstStadium);
        _repository.Add(secondStadium);

        IReadOnlyCollection<Stadium> stadiums = _repository.GetAll();

        Assert.AreEqual(2, stadiums.Count);
        Assert.IsTrue(stadiums.Contains(firstStadium));
        Assert.IsTrue(stadiums.Contains(secondStadium));
    }

    private static Stadium CreateStadium(int id, string name, string city, int capacity)
    {
        Stadium stadium = new Stadium(name, city, null, capacity);
        stadium.Id = id;
        return stadium;
    }
}
