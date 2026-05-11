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
            for (int i = entries.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (Team Team, string Group) tmp = entries[i];
                entries[i] = entries[j];
                entries[j] = tmp;
            }

            for (int i = 0; i < entries.Count; i += 2)
            {
                if (entries[i].Group == entries[i + 1].Group)
                {
                    for (int k = i + 2; k < entries.Count; k++)
                    {
                        if (entries[k].Group != entries[i].Group)
                        {
                            (Team Team, string Group) tmp = entries[i + 1];
                            entries[i + 1] = entries[k];
                            entries[k] = tmp;
                            break;
                        }
                    }
                }
            }

            List<MatchElimination> result = new List<MatchElimination>();

            for (int i = 0; i < entries.Count; i += 2)
            {
                Team home = entries[i].Team;
                Team away = entries[i + 1].Team;
                Stadium stadium = stadiumProvider(i / 2);
                DateTime date = dateProvider(i / 2);
                MatchGroup sourceHome = MatchGroup.CreateInstance(-_nextMatchId, DateTime.UtcNow.AddDays(-1), home, away, stadium, new Group("X"), Journey.CreateInstance(1));
                MatchGroup sourceAway = MatchGroup.CreateInstance(-(_nextMatchId + 1), DateTime.UtcNow.AddDays(-1), away, home, stadium, new Group("Y"), Journey.CreateInstance(1));
                string matchCode = $"R32-{_nextMatchId}";
                MatchElimination elim = MatchElimination.Create(_nextMatchId, date, home, away, stadium, Phase.RoundOf32, matchCode, sourceHome, sourceAway);
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
                MatchElimination elim = MatchElimination.Create(_nextMatchId, date, home, away, stadium, nextPhase, matchCode, sourceHome, sourceAway);
                result.Add(elim);
                _nextMatchId++;
            }

            return result;
        }
    }
}
