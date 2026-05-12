using ProjectCopa;

namespace Services
{
    public class EliminationBuilder
    {
        private int _nextMatchId = 2000;

        public List<MatchElimination> CreateRoundOf32(IEnumerable<(Team Team, string Group)> entriesWithGroup, IReadOnlyCollection<Stadium> stadiums, TournamentSettings settings, int crossingsSeed, Func<int, Stadium> stadiumProvider, Func<int, DateTime> dateProvider)
        {
            if (entriesWithGroup == null) throw new ArgumentNullException(nameof(entriesWithGroup));
            List<(Team Team, string Group)> entries = entriesWithGroup.ToList();
            if (entries.Count != 32) throw new ArgumentException("RoundOf32 requires exactly 32 teams");

            Random random = new Random(crossingsSeed);
            // Fisher-Yates shuffle for deterministic randomness
            for (int i = entries.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (Team Team, string Group) tmp = entries[i];
                entries[i] = entries[j];
                entries[j] = tmp;
            }

            // Build pairs trying to avoid same-group matchups when possible (greedy, deterministic with seed)
            int n = entries.Count;
            bool[] used = new bool[n];
            List<(int a, int b)> pairs = new List<(int a, int b)>();
n            for (int i = 0; i < n; i++)
            {
                if (used[i]) continue;
                used[i] = true;

                // collect candidates with different group
                List<int> candidates = new List<int>();
                for (int j = i + 1; j < n; j++)
                {
                    if (used[j]) continue;
                    if (entries[j].Group != entries[i].Group) candidates.Add(j);
                }
n                int selected;
                if (candidates.Count > 0)
                {
                    selected = candidates[random.Next(candidates.Count)];
                }
                else
                {
                    // no different-group candidate, pick any remaining deterministically
                    List<int> anyRemaining = new List<int>();
                    for (int j = i + 1; j < n; j++) if (!used[j]) anyRemaining.Add(j);
                    if (anyRemaining.Count == 0) break;
                    selected = anyRemaining[random.Next(anyRemaining.Count)];
                }
n                used[selected] = true;
                pairs.Add((i, selected));
            }

            List<MatchElimination> result = new List<MatchElimination>();
            int matchIndex = 0;
            foreach (var p in pairs)
            {
                Team home = entries[p.a].Team;
                Team away = entries[p.b].Team;
                Stadium stadium = stadiumProvider(matchIndex);
                DateTime date = dateProvider(matchIndex);
                MatchGroup sourceHome = MatchGroup.CreateMatchWithConfig(-_nextMatchId, settings.StartDate, home, away, stadium, settings, new Group("X"), Journey.CreateInstance(1));
                MatchGroup sourceAway = MatchGroup.CreateMatchWithConfig(-(_nextMatchId + 1), settings.StartDate, away, home, stadium, settings, new Group("Y"), Journey.CreateInstance(1));
                string matchCode = $"R32-{_nextMatchId}";
                MatchElimination elim = MatchElimination.CreateMatchWithConfig(_nextMatchId, date, home, away, stadium, settings, Phase.RoundOf32, matchCode, sourceHome, sourceAway);
                result.Add(elim);
                _nextMatchId++;
                matchIndex++;
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
}
