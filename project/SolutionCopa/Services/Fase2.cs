using System.Collections.Generic;
using DataAccess.Interfaces;
using ProjectCopa;
using Services.Interfaces;

namespace Services
{
    public class Fase2
    {
        private readonly IMatchSimulationEngine _simulationEngine;
        private readonly IMatchRepository _matchRepository;
        private readonly ITournamentSettingsRepository _settingsRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditLogService? _auditLog;

        public Fase2(
            IMatchSimulationEngine simulationEngine,
            IMatchRepository matchRepository,
            ITournamentSettingsRepository settingsRepository,
            IUnitOfWork unitOfWork,
            IAuditLogService? auditLog = null)
        {
            _simulationEngine = simulationEngine;
            _matchRepository = matchRepository;
            _settingsRepository = settingsRepository;
            _unitOfWork = unitOfWork;
            _auditLog = auditLog;
        }
        
        public void SimulateEliminationMatches(IEnumerable<int> matchIds)
        {
            TournamentSettings settings = _settingsRepository.GetById(1) ?? new TournamentSettings();
            int baseSeed = settings.SimulationSeed;

            foreach (int matchId in matchIds)
            {
                Match match = _matchRepository.GetById(matchId);
                if (match == null) continue;

                int seed = baseSeed + matchId;
                _simulationEngine.Simulate(match, seed);

                _matchRepository.Update(match);
                _auditLog?.Log(AuditActions.SimulateMatch, $"Simulated elimination match {matchId} with seed {seed}.", match.MatchId);
            }

            _unitOfWork.Commit();
        }
    }
}
