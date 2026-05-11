using System;
using System.Collections.Generic;
using System.Linq;
using DataAccess.Interfaces;
using ProjectCopa;
using Services.Interfaces;

namespace Services
{
    public class EliminationOrchestrator
    {
        private readonly IEliminationMatchRepository _eliminationRepo;
        private readonly IAuditLogService _auditLog;

        public EliminationOrchestrator(IEliminationMatchRepository eliminationRepo, IAuditLogService auditLog)
        {
            _eliminationRepo = eliminationRepo;
            _auditLog = auditLog;
        }

        public void OrchestrateEliminationPhase(IEnumerable<(Team Team, string Group)> qualifiedWithGroups, IReadOnlyCollection<Stadium> stadiums, TournamentSettings settings, int? crossingsSeedOverride = null)
        {
            if (qualifiedWithGroups == null) throw new ArgumentNullException(nameof(qualifiedWithGroups));
            var entries = qualifiedWithGroups.ToList();
            if (entries.Count != 32) throw new ArgumentException("Requires 32 qualified teams with group info");

            var builder = new EliminationBuilderV2();
            int crossingsSeed = crossingsSeedOverride ?? settings.CrossingsSeed;

            var stadiumList = stadiums?.ToList() ?? new List<Stadium> { new Stadium("Default", "City", null, 30000) };
            int maxPerDay = settings.MaxMatchesPerDay;
            int daysBetween = settings.DaysBetweenJourneys;

            DateTime roundStart = settings.StartDate;

            Func<int, Stadium> stadiumProvider = (matchIndex) => stadiumList[matchIndex % stadiumList.Count];

            Func<int, DateTime> makeDateProvider(DateTime start)
            {
                return (matchIndex) =>
                {
                    int dayOffset = matchIndex / maxPerDay;
                    return start.AddDays(dayOffset);
                };
            }

            var roundOf32 = builder.CreateRoundOf32(entries, stadiums, settings, crossingsSeed, stadiumProvider, makeDateProvider(roundStart));
            foreach (var m in roundOf32) _eliminationRepo.Add(m);

            _auditLog.Log("GenerateEliminationBracket", $"RoundOf32 generated with seed {crossingsSeed}.", settings.Id);

            var currentRound = roundOf32.Cast<Match>().ToList();

            var rounds = new (Phase phase, int matchCount)[]
            {
                (Phase.RoundOf16, 8),
                (Phase.QuarterFinals, 4),
                (Phase.SemiFinal, 2),
                (Phase.Final, 1)
            };

            for (int r = 0; r < rounds.Length; r++)
            {
                roundStart = roundStart.AddDays(daysBetween);
                var nextMatches = builder.CreateNextRoundFromMatches(currentRound, rounds[r].phase, stadiums, settings, stadiumProvider, makeDateProvider(roundStart));
                foreach (var nm in nextMatches) _eliminationRepo.Add(nm);
                _auditLog.Log("GenerateEliminationBracket", $"{rounds[r].phase} generated.", settings.Id);
                currentRound = nextMatches.Cast<Match>().ToList();
            }
        }
    }
}
