using Services;
using DataAccess.Interfaces;
using ProjectCopa;
using Services.DTOs;

namespace ServiceTest
{
    [TestClass]
    public class AuditLogServiceCoverageTests
    {
        [TestMethod]
        public void Log_GetAll_GetByTournamentSettings_And_ExportAsText()
        {
            FakeAuditLogRepository repo = new FakeAuditLogRepository();
            AuditLogService service = AuditLogService.CreateInstance(repo, defaultTournamentSettingsId: 99);

            // Log with explicit tournament id
            service.Log("Action1", "Details1", 1, "Match", 10);
            // Log with default tournament id overload
            service.Log("Action2", "Details2", "Team", 5);

            List<AuditLogDto> all = service.GetAll();
            Assert.AreEqual(2, all.Count);
            Assert.IsTrue(all.Any(a => a.Action == "Action1"));
            Assert.IsTrue(all.Any(a => a.Action == "Action2"));

            List<AuditLogDto> filtered = service.GetByTournamentSettings(1);
            Assert.AreEqual(1, filtered.Count);
            Assert.AreEqual("Action1", filtered[0].Action);

            string exportAll = service.ExportAsText(null);
            Assert.IsTrue(exportAll.Contains("Action1") && exportAll.Contains("Action2"));

            string exportFiltered = service.ExportAsText(1);
            Assert.IsTrue(exportFiltered.Contains("Action1") && !exportFiltered.Contains("Action2"));
        }

        private class FakeAuditLogRepository : IAuditLogRepository
        {
            private readonly List<AuditLog> _logs = new();
            public void Add(AuditLog log)
            {
                // emulate persistent id assignment
                log.SetId(_logs.Count + 1);
                _logs.Add(log);
            }

            public List<AuditLog> GetAll() => _logs.ToList();
        }
    }
}
