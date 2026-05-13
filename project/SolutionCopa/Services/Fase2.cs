using DataAccess.Interfaces;
using Services.Interfaces;

namespace Services;

public sealed class Fase2 : KnockoutFixtureGenerator
{
    public Fase2(
        IMatchSimulationEngine simulationEngine,
        IMatchRepository matchRepository,
        ITournamentSettingsRepository settingsRepository,
        IUnitOfWork unitOfWork,
        IAuditLogService? auditLog = null)
        : base(simulationEngine, matchRepository, settingsRepository, unitOfWork, auditLog)
    {
    }
}