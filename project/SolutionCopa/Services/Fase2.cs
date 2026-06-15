using DataAccess.Interfaces;
using Services.Interfaces;

namespace Services;

public sealed class Fase2 : KnockoutFixtureGenerator
{
    public Fase2(
        ISimulationEngineRegistry simulationEngineRegistry,
        IMatchRepository matchRepository,
        IEliminationMatchRepository eliminationRepository,
        IGroupRepository groupRepository,
        IStadiumRepository stadiumRepository,
        ITournamentSettingsRepository settingsRepository,
        IUnitOfWork unitOfWork,
        IAuditLogService auditLog,
        INotificationService notificationService,
        IRankingService rankingService)
        : base(
            simulationEngineRegistry,
            matchRepository,
            eliminationRepository,
            groupRepository,
            stadiumRepository,
            settingsRepository,
            unitOfWork,
            auditLog,
            notificationService,
            rankingService)
    {
    }
}
