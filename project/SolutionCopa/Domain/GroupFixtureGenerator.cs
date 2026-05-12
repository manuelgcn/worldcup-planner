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

    private const string AtLeastOneStadiumMessage = "At least one stadium is required.";
    private const string EachGroupMustHaveFourTeamsMessage = "Each group must contain exactly 4 teams.";
    private const string NoAvailableJornada3MessageTemplate = "No available timeslot for Jornada 3 of group {0}.";
    private const string NoAvailableSlotMessageTemplate = "No available timeslot for match number {0} in stadium {1}.";

    public List<Match> GenerateMatches(List<Group> groups, List<Stadium> stadiums, DateTime startDate)
    {
        ArgumentNullException.ThrowIfNull(groups);
        ArgumentNullException.ThrowIfNull(stadiums);
            
        if (stadiums.Count == 0) 
            throw new MatchException(AtLeastOneStadiumMessage);

        List<Group> orderedGroups = new List<Group>(groups.OrderBy(g => g.Name));
        List<Stadium> orderedStadiums = new List<Stadium>(stadiums.OrderBy(s => s.Name));
            
        List<Match> matches = new List<Match>();
        StadiumScheduleTracker tracker = new StadiumScheduleTracker();
        int matchNumber = 0;
        int groupIndex = 0;

        foreach (Group group in orderedGroups)
        {
            ValidateGroup(group);
            ScheduleGroupMatches(group, orderedStadiums, startDate, tracker, matches, ref matchNumber, groupIndex);
            groupIndex++;
        }

        return matches;
    }

    private void ValidateGroup(Group group)
    {
        if (group.Teams.Count != TeamsPerGroup)
        {
            throw new MatchException(EachGroupMustHaveFourTeamsMessage);
        }
    }

    private void ScheduleGroupMatches(
        Group group, 
        List<Stadium> orderedStadiums, 
        DateTime startDate, 
        StadiumScheduleTracker tracker, 
        List<Match> matches, 
        ref int matchNumber,
        int groupIndex)
    {
        List<Team> teams = new List<Team>(group.Teams);
        int groupsPerDay = (orderedStadiums.Count * MaxMatchesPerStadiumPerDay) / 2;
        int dayOffset = groupIndex / groupsPerDay;

        for (int jornada = 1; jornada <= TotalJornadas; jornada++)
        {
            int daysToAdd = dayOffset + ((jornada - 1) * DaysBetweenJornadas);
            DateTime day = startDate.AddDays(daysToAdd);
            List<(int home, int away)> pairs = GetPairsForJornada(jornada);

            if (jornada == SimultaneousJornadaNumber)
            {
                ScheduleSimultaneousJornada(group, teams, pairs, jornada, day, orderedStadiums, tracker, matches, ref matchNumber);
            }
            else
            {
                ScheduleStandardJornada(group, teams, pairs, jornada, day, orderedStadiums, tracker, matches, ref matchNumber);
            }
        }
    }

    private void ScheduleSimultaneousJornada(
        Group group, 
        List<Team> teams, 
        List<(int home, int away)> pairs, 
        int jornada, 
        DateTime day, 
        List<Stadium> orderedStadiums, 
        StadiumScheduleTracker tracker, 
        List<Match> matches, 
        ref int matchNumber)
    {
        int firstMatchNumber = matchNumber + 1;
        int secondMatchNumber = matchNumber + 2;

        Stadium stadium1 = GetStadiumForMatch(firstMatchNumber, orderedStadiums);
        Stadium stadium2 = GetStadiumForMatch(secondMatchNumber, orderedStadiums);

        bool scheduled = false;

        foreach (TimeSpan allowed in AllowedTimes)
        {
            DateTime matchDateTime = day.Date + allowed;

            if (tracker.CanSchedule(stadium1, matchDateTime) && tracker.CanSchedule(stadium2, matchDateTime))
            {
                MatchGroup matchA = BuildAndReserveMatch(firstMatchNumber, matchDateTime, group, teams, pairs[0], stadium1, jornada, tracker);
                MatchGroup matchB = BuildAndReserveMatch(secondMatchNumber, matchDateTime, group, teams, pairs[1], stadium2, jornada, tracker);

                matches.Add(matchA);
                matches.Add(matchB);

                matchNumber += 2;
                scheduled = true;
                break;
            }
        }

        if (!scheduled)
        {
            string message = string.Format(NoAvailableJornada3MessageTemplate, group.Name);
            throw new MatchException(message);
        }
    }

    private void ScheduleStandardJornada(
        Group group, 
        List<Team> teams, 
        List<(int home, int away)> pairs, 
        int jornada, 
        DateTime day, 
        List<Stadium> orderedStadiums, 
        StadiumScheduleTracker tracker, 
        List<Match> matches, 
        ref int matchNumber)
    {
        foreach ((int home, int away) pair in pairs)
        {
            int currentMatchNumber = matchNumber + 1;
            Stadium assignedStadium = GetStadiumForMatch(currentMatchNumber, orderedStadiums);
            bool scheduledMatch = false;

            foreach (TimeSpan allowed in AllowedTimes)
            {
                DateTime matchDateTime = day.Date + allowed;

                if (tracker.CanSchedule(assignedStadium, matchDateTime))
                {
                    MatchGroup match = BuildAndReserveMatch(currentMatchNumber, matchDateTime, group, teams, pair, assignedStadium, jornada, tracker);
                    matches.Add(match);
                        
                    matchNumber++;
                    scheduledMatch = true;
                    break;
                }
            }

            if (!scheduledMatch)
            {
                string message = string.Format(NoAvailableSlotMessageTemplate, currentMatchNumber, assignedStadium.Name);
                throw new MatchException(message);
            }
        }
    }

    private Stadium GetStadiumForMatch(int matchNumber, List<Stadium> orderedStadiums)
    {
        int stadiumIndex = (matchNumber - 1) % orderedStadiums.Count;
        return orderedStadiums[stadiumIndex];
    }

    private MatchGroup BuildAndReserveMatch(
        int matchNumber, 
        DateTime matchDateTime, 
        Group group, 
        List<Team> teams, 
        (int home, int away) pair, 
        Stadium stadium, 
        int jornada, 
        StadiumScheduleTracker tracker)
    {
        Team homeTeam = teams[pair.home];
        Team awayTeam = teams[pair.away];
        string matchCode = MatchCodePrefix + matchNumber;

        MatchGroup match = MatchGroup.CreateInstance(DefaultMatchId, matchDateTime, homeTeam, awayTeam, stadium, group, Journey.CreateInstance(jornada));
        match.SetMatchCode(matchCode);
        tracker.Reserve(stadium, matchDateTime);

        return match;
    }

    private static List<(int home, int away)> GetPairsForJornada(int jornada)
    {
        if (jornada == 1)
        {
            return new List<(int home, int away)> { (TeamAIndex, TeamDIndex), (TeamBIndex, TeamCIndex) };
        }

        if (jornada == 2)
        {
            return new List<(int home, int away)> { (TeamAIndex, TeamCIndex), (TeamBIndex, TeamDIndex) };
        }

        return new List<(int home, int away)> { (TeamAIndex, TeamBIndex), (TeamCIndex, TeamDIndex) };
    }
}
