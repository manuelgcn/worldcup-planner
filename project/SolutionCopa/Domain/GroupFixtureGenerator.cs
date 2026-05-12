using ProjectCopa.Exceptions;

namespace ProjectCopa;

public sealed class GroupFixtureGenerator
{
    private const int TeamsPerGroup = 4;
    private const int TotalJornadas = 3;
    private const int DaysBetweenJornadas = 3;
    private const int MaxMatchesPerStadiumPerDay = 3;
    private const int SimultaneousJornadaNumber = 3;
    private const int DefaultMatchId = 0;
    private const string MatchCodePrefix = "M";
    private const int TeamAIndex = 0;
    private const int TeamBIndex = 1;
    private const int TeamCIndex = 2;
    private const int TeamDIndex = 3;

    private static readonly TimeSpan[] AllowedTimes =
    [
        new TimeSpan(14, 0, 0), new TimeSpan(18, 0, 0), new TimeSpan(22, 0, 0)
    ];

    public List<Match> GenerateMatches(List<Group> groups, List<Stadium> stadiums, DateTime startDate)
    {
        ArgumentNullException.ThrowIfNull(groups);
        ArgumentNullException.ThrowIfNull(stadiums);
            
        if (stadiums.Count == 0) 
            throw new MatchException("At least one stadium is required.");

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
            int groupsPerDay = (orderedStadiums.Count * MaxMatchesPerStadiumPerDay) / 2;
            int dayOffset = groupIndex / groupsPerDay;

            for (int jornada = 1; jornada <= TotalJornadas; jornada++)
            {
                DateTime day = startDate.AddDays(dayOffset + ((jornada - 1) * DaysBetweenJornadas));
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
        Stadium stadium1 = GetStadiumForMatch(matchNumber + 1, orderedStadiums);
        Stadium stadium2 = GetStadiumForMatch(matchNumber + 2, orderedStadiums);
        bool scheduled = false;

        foreach (TimeSpan time in AllowedTimes)
        {
            DateTime matchTime = day.Date + time;
            if (tracker.CanSchedule(stadium1, matchTime) && tracker.CanSchedule(stadium2, matchTime))
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
            Stadium assignedStadium = GetStadiumForMatch(matchNumber + 1, orderedStadiums);
            bool scheduledMatch = false;

            foreach (TimeSpan time in AllowedTimes)
            {
                DateTime matchTime = day.Date + time;
                if (tracker.CanSchedule(assignedStadium, matchTime))
                {
                    matches.Add(BuildAndReserve(matchNumber + 1, matchTime, group, teams, pair, assignedStadium, jornada, tracker));
                    matchNumber++;
                    scheduledMatch = true;
                    break;
                }
            }
            if (!scheduledMatch) throw new MatchException($"No available timeslot for match in stadium {assignedStadium.Name}");
        }
    }

    private Stadium GetStadiumForMatch(int mNum, List<Stadium> orderedStadiums) => orderedStadiums[(mNum - 1) % orderedStadiums.Count];

    private MatchGroup BuildAndReserve(int mNum, DateTime mTime, Group group, List<Team> teams, (int home, int away) pair, Stadium stadium, int jornada, StadiumScheduleTracker tracker)
    {
        MatchGroup match = MatchGroup.CreateInstance(DefaultMatchId, mTime, teams[pair.home], teams[pair.away], stadium, group, Journey.CreateInstance(jornada));
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