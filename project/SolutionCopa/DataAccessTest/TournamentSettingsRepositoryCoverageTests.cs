using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectCopa;
using DataAccess.Repositories;
using DataAccessTest.TestDoubles;

namespace DataAccessTest;

[TestClass]
public class TournamentSettingsRepositoryCoverageTests
{
    [TestMethod]
    public void Add_Update_Delete_And_GetAll_TournamentSettings()
    {
        TournamentSettingsRepository repo = new TournamentSettingsRepository(new InMemoryDbContextFactory());

        TournamentSettings s1 = new TournamentSettings(1, new DateTime(2026, 6, 1), 3, 3, 11, 22, 33, 44);
        repo.Add(s1);

        TournamentSettings? fetched = repo.GetById(1);
        Assert.IsNotNull(fetched);
        Assert.AreEqual(1, fetched!.Id);

        // Update
        TournamentSettings s1Updated = new TournamentSettings(1, new DateTime(2026, 6, 2), 2, 2, 111, 222, 333, 444);
        repo.Update(s1Updated);
        TournamentSettings? fetched2 = repo.GetById(1);
        Assert.IsNotNull(fetched2);
        Assert.AreEqual(2, fetched2!.MaxMatchesPerDay);

        // GetAll
        IReadOnlyCollection<TournamentSettings> all = repo.GetAll();
        Assert.IsTrue(all.Count >= 1);

        // Delete
        repo.Delete(s1Updated);
        TournamentSettings? afterDel = repo.GetById(1);
        Assert.IsNull(afterDel);
    }
}
