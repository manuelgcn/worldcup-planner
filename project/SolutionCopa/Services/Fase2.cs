using System;
using System.Collections.Generic;
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
                Match? match = GetMatchById(matchId);
                if (match == null) continue;

                int seed = baseSeed + matchId;
                _simulationEngine.Simulate(match, seed);

                _matchRepository.Update(match);
                _auditLog?.Log(AuditActions.SimulateMatch, $"Simulated elimination match {matchId} with seed {seed}.", settings.Id, "Match", match.MatchId);
            }

            _unitOfWork.Commit();
        }

        private Match? GetMatchById(int matchId)
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

        public List<(Team Team, string Group)> ComputeQualified32FromGroups(IReadOnlyCollection<Group> groups, TournamentSettings settings)
        {
            if (groups == null) throw new ArgumentNullException(nameof(groups));
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            var qualified = new List<(Team Team, string Group)>();
            var thirdPlaceCandidates = new List<(Team Team, int Pts, int GD, int GF, string Group)>();

            foreach (Group g in groups)
            {
                var matches = _matchRepository.GetByGroup(g.Name) ?? new List<Match>();
                var stats = new Dictionary<int, (Team Team, int Pts, int GF, int GA)>();

                foreach (Team t in g.Teams)
                {
                    if (!stats.ContainsKey(t.Id)) stats[t.Id] = (t, 0, 0, 0);
                }

                foreach (Match m in matches)
                {
                    if (m.HomeTeam == null || m.AwayTeam == null) continue;

                    if (!stats.ContainsKey(m.HomeTeam.Id)) stats[m.HomeTeam.Id] = (m.HomeTeam, 0, 0, 0);
                    if (!stats.ContainsKey(m.AwayTeam.Id)) stats[m.AwayTeam.Id] = (m.AwayTeam, 0, 0, 0);

                    var home = stats[m.HomeTeam.Id];
                    var away = stats[m.AwayTeam.Id];

                    home.GF += m.HomeGoals;
                    home.GA += m.AwayGoals;
                    away.GF += m.AwayGoals;
                    away.GA += m.HomeGoals;

                    if (m.HomeGoals > m.AwayGoals) home.Pts += 3;
                    else if (m.HomeGoals < m.AwayGoals) away.Pts += 3;
                    else { home.Pts += 1; away.Pts += 1; }

                    stats[m.HomeTeam.Id] = home;
                    stats[m.AwayTeam.Id] = away;
                }

                var standings = stats.Values
                    .Select(s => (s.Team, s.Pts, GD: s.GF - s.GA, GF: s.GF))
                    .OrderByDescending(s => s.Pts)
                    .ThenByDescending(s => s.GD)
                    .ThenByDescending(s => s.GF)
                    .ThenBy(s => s.Team.Id)
                    .ToList();

                if (standings.Count >= 1) qualified.Add((standings[0].Team, g.Name));
                if (standings.Count >= 2) qualified.Add((standings[1].Team, g.Name));
                if (standings.Count >= 3) thirdPlaceCandidates.Add((standings[2].Team, standings[2].Pts, standings[2].GD, standings[2].GF, g.Name));
            }
            var qualificationService = new QualificationService();
            var best8 = qualificationService.GetBestEightThirds(thirdPlaceCandidates, settings.CrossingsSeed);
            foreach (var t in best8)
            {
                var grp = thirdPlaceCandidates.FirstOrDefault(c => c.Team.Id == t.Id).Group;
                qualified.Add((t, grp));
            }
            if (qualified.Count != 32)
            {
                throw new InvalidOperationException($"Computed qualified teams count is {qualified.Count}, expected 32.");
            }
            return qualified;
        }
    }
}

