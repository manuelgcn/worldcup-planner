using DataAccess.Interfaces;
using Services.Interfaces;

namespace Services;

public sealed class Fase2 : KnockoutFixtureGenerator
{
    public Fase2(
        ISimulationEngineRegistry simulationEngineRegistry,
        IMatchRepository matchRepository,
        ITournamentSettingsRepository settingsRepository,
        IUnitOfWork unitOfWork,
        IAuditLogService? auditLog = null)
        : base(simulationEngineRegistry, matchRepository, settingsRepository, unitOfWork, auditLog)
    {
    }
}