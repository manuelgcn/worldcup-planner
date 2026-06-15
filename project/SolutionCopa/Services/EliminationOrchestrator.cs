using DataAccess.Interfaces;
using ProjectCopa;
using Services.Interfaces;

namespace Services;

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
        List<(Team Team, string Group)> entries = qualifiedWithGroups.ToList();
        if (entries.Count != 32) throw new ArgumentException("Se requieren 32 equipos clasificados con su grupo");

        EliminationBuilder builder = new EliminationBuilder();
        int crossingsSeed = crossingsSeedOverride ?? settings.CrossingsSeed;

        List<Stadium> stadiumList = stadiums?.ToList() ?? new List<Stadium> { new Stadium("Default", "City", null, 30000) };
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

        List<MatchElimination> roundOf32 = builder.CreateRoundOf32(entries, stadiumList, settings, crossingsSeed, stadiumProvider, makeDateProvider(roundStart));
        foreach (MatchElimination m in roundOf32) _eliminationRepo.Add(m);

        _auditLog.Log(AuditActions.GenerateKnockout, $"RoundOf32 generated with seed {crossingsSeed}.", settings.Id);

        List<Match> currentRound = roundOf32.Cast<Match>().ToList();

        (Phase phase, int matchCount)[] rounds = new (Phase phase, int matchCount)[]
        {
            (Phase.RoundOf16, 8),
            (Phase.QuarterFinals, 4),
            (Phase.SemiFinal, 2),
            (Phase.Final, 1)
        };

        for (int r = 0; r < rounds.Length; r++)
        {
            roundStart = roundStart.AddDays(daysBetween);
            List<MatchElimination> nextMatches = builder.CreateNextRoundFromMatches(currentRound, rounds[r].phase, stadiumList, settings, stadiumProvider, makeDateProvider(roundStart));
            foreach (MatchElimination nm in nextMatches) _eliminationRepo.Add(nm);
            _auditLog.Log(AuditActions.GenerateKnockout, $"{rounds[r].phase} generated.", settings.Id);
            currentRound = nextMatches.Cast<Match>().ToList();
        }
    }
}
