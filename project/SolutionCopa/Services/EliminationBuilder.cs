using ProjectCopa;

namespace Services;

public class EliminationBuilder
{
    private int _nextMatchId = 2000;

    public List<MatchElimination> CreateRoundOf32(IEnumerable<(Team Team, string Group)> entriesWithGroup, IReadOnlyCollection<Stadium> stadiums, TournamentSettings settings, int crossingsSeed, Func<int, Stadium> stadiumProvider, Func<int, DateTime> dateProvider)
    {
        if (entriesWithGroup == null) throw new ArgumentNullException(nameof(entriesWithGroup));
        List<(Team Team, string Group)> entries = entriesWithGroup.ToList();
        if (entries.Count != 32) throw new ArgumentException("RoundOf32 requires exactly 32 teams");

        Random random = new Random(crossingsSeed);

        for (int i = entries.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (Team Team, string Group) tmp = entries[i];
            entries[i] = entries[j];
            entries[j] = tmp;
        }

        // Interleave by group to guarantee no same-group pairings.
        // Groups with more qualified teams are processed first so their teams
        // spread across different slots before lighter groups run out of slots.
        List<List<(Team Team, string Group)>> byGroup = entries
            .GroupBy(e => e.Group)
            .OrderByDescending(g => g.Count())
            .Select(g => g.ToList())
            .ToList();

        List<(Team Team, string Group)> interleaved = new List<(Team Team, string Group)>(entries.Count);
        int maxSlot = byGroup.Max(g => g.Count);
        for (int slot = 0; slot < maxSlot; slot++)
        {
            foreach (List<(Team Team, string Group)> group in byGroup)
            {
                if (slot < group.Count)
                    interleaved.Add(group[slot]);
            }
        }

        List<MatchElimination> result = new List<MatchElimination>();
        for (int i = 0; i < interleaved.Count; i += 2)
        {
            Team home = interleaved[i].Team;
            Team away = interleaved[i + 1].Team;
            Stadium stadium = stadiumProvider(i / 2);
            DateTime date = dateProvider(i / 2);
            MatchGroup sourceHome = MatchGroup.CreateMatchWithConfig(-_nextMatchId, settings.StartDate, home, away, stadium, settings, new Group("X"), Journey.CreateInstance(1));
            MatchGroup sourceAway = MatchGroup.CreateMatchWithConfig(-(_nextMatchId + 1), settings.StartDate, away, home, stadium, settings, new Group("Y"), Journey.CreateInstance(1));
            string matchCode = $"R32-{_nextMatchId}";
            MatchElimination elim = MatchElimination.CreateMatchWithConfig(_nextMatchId, date, home, away, stadium, settings, Phase.RoundOf32, matchCode, sourceHome, sourceAway);
            result.Add(elim);
            _nextMatchId++;
        }

        return result;
    }

    public List<MatchElimination> CreateNextRoundFromMatches(IReadOnlyList<Match> previousRoundMatches, Phase nextPhase, IReadOnlyCollection<Stadium> stadiums, TournamentSettings settings, Func<int, Stadium> stadiumProvider, Func<int, DateTime> dateProvider)
    {
        if (previousRoundMatches == null) throw new ArgumentNullException(nameof(previousRoundMatches));
        List<MatchElimination> result = new List<MatchElimination>();

        for (int i = 0; i < previousRoundMatches.Count; i += 2)
        {
            Match sourceHome = previousRoundMatches[i];
            Match sourceAway = previousRoundMatches[i + 1];
            Team home = sourceHome.HomeTeam ?? sourceHome.AwayTeam ?? throw new ArgumentException("Source match must have a team");
            Team away = sourceAway.HomeTeam ?? sourceAway.AwayTeam ?? throw new ArgumentException("Source match must have a team");
            Stadium stadium = stadiumProvider(i / 2);
            DateTime date = dateProvider(i / 2);
            string matchCode = $"{nextPhase}-{_nextMatchId}";
            MatchElimination elim = MatchElimination.CreateMatchWithConfig(_nextMatchId, date, home, away, stadium, settings, nextPhase, matchCode, sourceHome, sourceAway);
            result.Add(elim);
            _nextMatchId++;
        }

        return result;
    }
}