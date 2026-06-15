using ProjectCopa.Exceptions;

namespace ProjectCopa;

public sealed class GroupFixtureGenerator
{
    private const int TeamsPerGroup = 4;
    private const int TotalJornadas = 3;
    private const int DefaultDaysBetweenJornadas = 3;
    private const int DefaultMaxMatchesPerDay = 3;
    private const int FirstMatchHour = 14;
    private const int HoursBetweenMatches = 4;
    // Tope fisico de partidos/dia por estadio: desde las 14:00 con 4h de separacion solo entran
    // 14:00, 18:00 y 22:00 antes de medianoche (CLAUDE.md §7).
    private const int MaxMatchesPerDayUpperBound = 3;
    private const int SimultaneousJornadaNumber = 3;
    private const string MatchCodePrefix = "M";
    private const int TeamAIndex = 0;
    private const int TeamBIndex = 1;
    private const int TeamCIndex = 2;
    private const int TeamDIndex = 3;

    // Configurables por TournamentSettings (H9). Se fijan al inicio de GenerateMatches y caen al
    // default cuando no se proveen, preservando el comportamiento previo.
    private int _daysBetweenJornadas = DefaultDaysBetweenJornadas;
    private TimeSpan[] _allowedTimes = BuildAllowedTimes(DefaultMaxMatchesPerDay);

    public List<Match> GenerateMatches(
        List<Group> groups,
        List<Stadium> stadiums,
        DateTime startDate,
        int? daysBetweenJourneys = null,
        int? maxMatchesPerDay = null)
    {
        ArgumentNullException.ThrowIfNull(groups);
        ArgumentNullException.ThrowIfNull(stadiums);

        if (stadiums.Count == 0)
            throw new MatchException("Se requiere al menos un estadio.");

        _daysBetweenJornadas = daysBetweenJourneys ?? DefaultDaysBetweenJornadas;
        _allowedTimes = BuildAllowedTimes(maxMatchesPerDay ?? DefaultMaxMatchesPerDay);

        List<Group> orderedGroups = new List<Group>(groups.OrderBy(g => g.Name));
        List<Stadium> orderedStadiums = new List<Stadium>(stadiums.OrderBy(s => s.Name));
        List<Match> matches = new List<Match>();
        StadiumScheduleTracker tracker = new StadiumScheduleTracker();
        
        int matchNumber = 0;
        int groupIndex = 0;

        foreach (Group group in orderedGroups)
        {
            if (group.Teams.Count != TeamsPerGroup)
                throw new MatchException("Each group must contain exactly 4 teams.");

            List<Team> teams = new List<Team>(group.Teams);
            int groupsPerDay = GroupsPerDay(orderedStadiums.Count);
            int dayOffset = groupIndex / groupsPerDay;

            for (int jornada = 1; jornada <= TotalJornadas; jornada++)
            {
                DateTime day = startDate.AddDays(dayOffset + ((jornada - 1) * _daysBetweenJornadas));
                List<(int home, int away)> pairs = GetPairsForJornada(jornada);

                if (jornada == SimultaneousJornadaNumber)
                {
                    ScheduleSimultaneous(group, teams, pairs, day, orderedStadiums, tracker, matches, ref matchNumber);
                }
                else
                {
                    ScheduleStandard(group, teams, pairs, jornada, day, orderedStadiums, tracker, matches, ref matchNumber);
                }
            }
            groupIndex++;
        }
        return matches;
    }

    private void ScheduleSimultaneous(Group group, List<Team> teams, List<(int home, int away)> pairs, DateTime day, List<Stadium> orderedStadiums, StadiumScheduleTracker tracker, List<Match> matches, ref int matchNumber)
    {
        int preferredIndex1 = PreferredStadiumIndex(matchNumber + 1, orderedStadiums.Count);
        int preferredIndex2 = PreferredStadiumIndex(matchNumber + 2, orderedStadiums.Count);
        bool scheduled = false;

        // Ambos partidos de la ultima jornada deben jugarse el mismo dia y misma hora.
        // Por eso buscamos un horario donde haya DOS estadios libres a la vez.
        foreach (TimeSpan time in _allowedTimes)
        {
            DateTime matchTime = day.Date + time;
            if (TryFindAvailableStadium(preferredIndex1, matchTime, orderedStadiums, tracker, null, out Stadium stadium1)
                && TryFindAvailableStadium(preferredIndex2, matchTime, orderedStadiums, tracker, stadium1, out Stadium stadium2))
            {
                matches.Add(BuildAndReserve(matchNumber + 1, matchTime, group, teams, pairs[0], stadium1, SimultaneousJornadaNumber, tracker));
                matches.Add(BuildAndReserve(matchNumber + 2, matchTime, group, teams, pairs[1], stadium2, SimultaneousJornadaNumber, tracker));
                matchNumber += 2;
                scheduled = true;
                break;
            }
        }
        if (!scheduled) throw new MatchException($"No available timeslot for Jornada 3 of group {group.Name}");
    }

    private void ScheduleStandard(Group group, List<Team> teams, List<(int home, int away)> pairs, int jornada, DateTime day, List<Stadium> orderedStadiums, StadiumScheduleTracker tracker, List<Match> matches, ref int matchNumber)
    {
        foreach ((int home, int away) pair in pairs)
        {
            int preferredIndex = PreferredStadiumIndex(matchNumber + 1, orderedStadiums.Count);
            bool scheduledMatch = false;

            foreach (TimeSpan time in _allowedTimes)
            {
                DateTime matchTime = day.Date + time;
                if (TryFindAvailableStadium(preferredIndex, matchTime, orderedStadiums, tracker, null, out Stadium assignedStadium))
                {
                    matches.Add(BuildAndReserve(matchNumber + 1, matchTime, group, teams, pair, assignedStadium, jornada, tracker));
                    matchNumber++;
                    scheduledMatch = true;
                    break;
                }
            }
            if (!scheduledMatch) throw new MatchException($"No available timeslot for Jornada {jornada} of group {group.Name}");
        }
    }

    // Cantidad de grupos que arrancan el mismo dia. La jornada 3 (simultanea) es la
    // restriccion mas dura: cada grupo necesita un PAR de estadios al mismo horario,
    // por eso el tope es floor(estadios / 2) por cada horario disponible.
    private int GroupsPerDay(int stadiumCount) => (stadiumCount / 2) * _allowedTimes.Length;

    // Horarios permitidos: arrancan a las 14:00 y se separan 4h (CLAUDE.md §7); la cantidad la fija
    // MaxMatchesPerDay, acotada a los que entran en un dia (max 3: 14:00, 18:00, 22:00).
    private static TimeSpan[] BuildAllowedTimes(int maxMatchesPerDay)
    {
        int slots = Math.Clamp(maxMatchesPerDay, 1, MaxMatchesPerDayUpperBound);
        return Enumerable.Range(0, slots)
            .Select(slot => new TimeSpan(FirstMatchHour + (slot * HoursBetweenMatches), 0, 0))
            .ToArray();
    }

    // Indice del estadio preferido segun la rotacion circular por orden de nombre:
    // Partido n -> Estadio[(n - 1) mod CantEstadios].
    private static int PreferredStadiumIndex(int matchNumber, int stadiumCount) => (matchNumber - 1) % stadiumCount;

    // Busca un estadio disponible empezando por el preferido (respeta la rotacion) y,
    // si esta lleno, avanza circularmente al siguiente que tenga lugar en ese horario.
    private static bool TryFindAvailableStadium(int preferredIndex, DateTime matchTime, List<Stadium> orderedStadiums, StadiumScheduleTracker tracker, Stadium? exclude, out Stadium chosen)
    {
        int count = orderedStadiums.Count;
        for (int offset = 0; offset < count; offset++)
        {
            Stadium candidate = orderedStadiums[(preferredIndex + offset) % count];
            if (exclude is not null && ReferenceEquals(candidate, exclude))
            {
                continue;
            }

            if (tracker.CanSchedule(candidate, matchTime))
            {
                chosen = candidate;
                return true;
            }
        }

        chosen = null!;
        return false;
    }

    private MatchGroup BuildAndReserve(int mNum, DateTime mTime, Group group, List<Team> teams, (int home, int away) pair, Stadium stadium, int jornada, StadiumScheduleTracker tracker)
    {
        MatchGroup match = MatchGroup.CreateInstance(mNum, mTime, teams[pair.home], teams[pair.away], stadium, group, Journey.CreateInstance(jornada));
        match.SetMatchCode(MatchCodePrefix + mNum);
        tracker.Reserve(stadium, mTime);
        return match;
    }

    private static List<(int home, int away)> GetPairsForJornada(int jornada)
    {
        if (jornada == 1) return new List<(int, int)> { (TeamAIndex, TeamDIndex), (TeamBIndex, TeamCIndex) };
        if (jornada == 2) return new List<(int, int)> { (TeamAIndex, TeamCIndex), (TeamBIndex, TeamDIndex) };
        return new List<(int, int)> { (TeamAIndex, TeamBIndex), (TeamCIndex, TeamDIndex) };
    }
}