using WorldCupPlanner.DataAccess.Interfaces;
using WorldCupPlanner.Domain;
using WorldCupPlanner.Services.Interfaces;

namespace WorldCupPlanner.Services;

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
        int daysBetween = settings.DaysBetweenJourneys;

        DateTime roundStart = settings.StartDate;

        // Cada ronda usa su propio KnockoutRoundScheduler: rota estadio y franja horaria (14:00 +4h)
        // por indice de partido. Antes el dateProvider solo variaba el dia (nunca la hora), por lo
        // que los partidos de una misma ronda repartidos en un dia compartian fecha/hora.
        KnockoutRoundScheduler roundOf32Scheduler = new KnockoutRoundScheduler(stadiumList, settings.MaxMatchesPerDay, roundStart);

        List<MatchElimination> roundOf32 = builder.CreateRoundOf32(entries, stadiumList, settings, crossingsSeed, roundOf32Scheduler.StadiumProvider, roundOf32Scheduler.DateProvider);
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
            KnockoutRoundScheduler scheduler = new KnockoutRoundScheduler(stadiumList, settings.MaxMatchesPerDay, roundStart);
            List<MatchElimination> nextMatches = builder.CreateNextRoundFromMatches(currentRound, rounds[r].phase, stadiumList, settings, scheduler.StadiumProvider, scheduler.DateProvider);
            foreach (MatchElimination nm in nextMatches) _eliminationRepo.Add(nm);
            _auditLog.Log(AuditActions.GenerateKnockout, $"{rounds[r].phase} generated.", settings.Id);
            currentRound = nextMatches.Cast<Match>().ToList();
        }
    }
}
