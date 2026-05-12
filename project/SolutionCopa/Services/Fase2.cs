using System.Linq;
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
            TournamentSettings? settings = _settingsRepository.GetAll().FirstOrDefault();
            if (settings == null) throw new InvalidOperationException("Tournament settings are required to simulate matches.");
            int baseSeed = settings.SimulationSeed;

            foreach (int matchId in matchIds)
            {
                Match? match = _match_repository_GetById(matchId);
                if (match == null) continue;

                int seed = baseSeed + matchId;
                _simulationEngine.Simulate(match, seed);

                _matchRepository.Update(match);
                _auditLog?.Log(AuditActions.SimulateMatch, $"Simulated elimination match {matchId} with seed {seed}.", settings.Id, "Match", match.MatchId);
            }

            _unitOfWork.Commit();
        }

        private Match? _match_repository_GetById(int matchId)
        {
            try
            {
                return _matchRepository.GetById(matchId);
            }
            catch
            {
                return null;
            }
        }

        public void OrchestrateAndSimulateEliminationPhase(IEnumerable<(Team Team, string Group)> qualifiedWithGroups, IReadOnlyCollection<Stadium> stadiums, TournamentSettings settings, IEliminationMatchRepository eliminationRepo, int? crossingsSeedOverride = null)
        {
            if (qualifiedWithGroups == null) throw new ArgumentNullException(nameof(qualifiedWithGroups));
            List<(Team Team, string Group)> entries = qualifiedWithGroups.ToList();
            if (entries.Count != 32) throw new ArgumentException("Requires 32 qualified teams with group info");

            EliminationOrchestrator orchestrator = new EliminationOrchestrator(eliminationRepo, _auditLog ?? throw new InvalidOperationException("AuditLogService required"));
            orchestrator.OrchestrateEliminationPhase(entries, stadiums, settings, crossingsSeedOverride);

            List<Match> allMatches = eliminationRepo.GetAll().ToList();
            foreach (Match m in allMatches)
            {
                try
                {
                    int seed = settings.SimulationSeed + m.MatchId;
                    _simulationEngine.Simulate(m, seed);
                    eliminationRepo.Update(m);
                    _auditLog?.Log(AuditActions.SimulateMatch, $"Simulated elimination match {m.MatchId} with seed {seed}.", settings.Id, "Match", m.MatchId);
                }
                catch (Exception ex)
                {
                    _auditLog?.Log(AuditActions.SimulateMatch, $"Failed to simulate match {m.MatchId}: {ex.Message}", settings.Id, "Match", m.MatchId);
                }
            }

            _unitOfWork.Commit();
        }
    }
}
