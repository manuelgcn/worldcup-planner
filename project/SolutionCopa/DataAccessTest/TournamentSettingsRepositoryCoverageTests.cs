using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectCopa;
using DataAccess.Repositories;

namespace DataAccessTest
{
    [TestClass]
    public class TournamentSettingsRepositoryCoverageTests
    {
        [TestMethod]
        public void Add_Update_Delete_And_GetAll_TournamentSettings()
        {
            var repo = new TournamentSettingsRepository();

            var s1 = new TournamentSettings(1, new DateTime(2026,6,1), 3, 3, 11, 22, 33, 44);
            repo.Add(s1);

            var fetched = repo.GetById(1);
            Assert.IsNotNull(fetched);
            Assert.AreEqual(1, fetched!.Id);

            // Update
            var s1Updated = new TournamentSettings(1, new DateTime(2026,6,2), 2, 2, 111, 222, 333, 444);
            repo.Update(s1Updated);
            var fetched2 = repo.GetById(1);
            Assert.IsNotNull(fetched2);
            Assert.AreEqual(2, fetched2!.MaxMatchesPerDay);

            // GetAll
            var all = repo.GetAll();
            Assert.IsTrue(all.Count >= 1);

            // Delete
            repo.Delete(s1Updated);
            var afterDel = repo.GetById(1);
            Assert.IsNull(afterDel);
        }
    }
}
