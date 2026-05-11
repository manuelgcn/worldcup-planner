using ProjectCopa;
using DataAccess.Interfaces;

namespace DataAccess.Repositories
{
    public sealed class AuditLogRepository : IAuditLogRepository
    {
        private readonly List<AuditLog> _logs = new List<AuditLog>();

        public void Add(AuditLog log)
        {
            _logs.Add(log);
        }

        public List<AuditLog> GetAll()
        {
            return _logs.ToList();
        }
    }
}
