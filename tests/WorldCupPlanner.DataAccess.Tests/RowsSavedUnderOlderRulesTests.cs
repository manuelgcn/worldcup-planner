using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WorldCupPlanner.DataAccess.Repositories;
using WorldCupPlanner.DataAccess.Tests.TestDoubles;
using WorldCupPlanner.Domain;

namespace WorldCupPlanner.DataAccess.Tests;

// Las reglas de nombres se endurecieron con datos ya guardados. Una fila anterior tiene que seguir leyendose:
// si EF la armara con el constructor que valida, esa sola fila tumbaria cada pantalla que lista equipos o
// estadios y ni siquiera se podria borrar. Las filas se insertan con SQL porque el dominio ya no las deja crear.
[TestClass]
public sealed class RowsSavedUnderOlderRulesTests
{
    private SqliteDbContextFactory _factory = null!;

    [TestInitialize]
    public void Setup()
    {
        _factory = new SqliteDbContextFactory();
    }

    [TestCleanup]
    public void Cleanup() => _factory.Dispose();

    [TestMethod]
    public void ATeamWhoseNameHasOnlySignsCanStillBeReadAndDeleted()
    {
        using (AppDbContext context = _factory.CreateDbContext())
        {
            context.Database.ExecuteSqlRaw(
                "INSERT INTO Teams (Id, Name, FifaRanking, IsFictitious, Confederation) VALUES (7, '?!', 1500, 1, 'UEFA')");
        }
        TeamRepository repository = new TeamRepository(_factory);

        Team stored = repository.GetAll().Single();
        repository.Delete(stored);

        Assert.AreEqual(7, stored.Id);
        Assert.AreEqual("?!", stored.Name);
        Assert.AreEqual(1500, stored.FifaRanking);
        Assert.IsTrue(stored.IsFictitious);
        Assert.AreEqual(Confederation.UEFA, stored.Confederation);
        Assert.AreEqual(0, repository.GetAll().Count);
    }

    [TestMethod]
    public void AStadiumWhoseNameAndCityHaveATabCanStillBeRead()
    {
        using (AppDbContext context = _factory.CreateDbContext())
        {
            context.Database.ExecuteSqlRaw(
                "INSERT INTO Stadiums (Id, Name, City, Capacity) VALUES (3, {0}, {1}, 40000)",
                "Estadio\tCentenario",
                "Monte\tvideo");
        }

        Stadium stored = new StadiumRepository(_factory).GetAll().Single();

        Assert.AreEqual(3, stored.Id);
        Assert.AreEqual("Estadio\tCentenario", stored.Name);
        Assert.AreEqual("Monte\tvideo", stored.City);
        Assert.AreEqual(40000, stored.Capacity);
    }
}
