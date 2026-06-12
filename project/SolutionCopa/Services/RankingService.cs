using ProjectCopa;
using Services.Interfaces;

namespace Services;

public class RankingService : IRankingService
{
    private readonly IAuditLogService _auditLogService;

    public RankingService(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    public void ApplyMatchResult(Match match, MatchPhase phase)
    {
    }
}