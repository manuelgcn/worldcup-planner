using System;
using System.Collections.Generic;
using System.Linq;
using ProjectCopa;

namespace Services
{
    public class EliminationBuilderV2
    {
        private int _nextMatchId = 2000;

        public List<MatchElimination> CreateRoundOf32(IEnumerable<(Team Team, string Group)> entriesWithGroup, IReadOnlyCollection<Stadium> stadiums, TournamentSettings settings, int crossingsSeed, Func<int, Stadium> stadiumProvider, Func<int, DateTime> dateProvider)
        {
            if (entriesWithGroup == null) throw new ArgumentNullException(nameof(entriesWithGroup));
            var entries = entriesWithGroup.ToList();
            if (entries.Count != 32) throw new ArgumentException("RoundOf32 requires exactly 32 teams");

            var random = new Random(crossingsSeed);
            for (int i = entries.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                var tmp = entries[i];
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
                            var tmp = entries[i + 1];
                            entries[i + 1] = entries[k];
                            entries[k] = tmp;
                            break;
                        }
                    }
                }
            }

            var result = new List<MatchElimination>();

            for (int i = 0; i < entries.Count; i += 2)
            {
                var home = entries[i].Team;
                var away = entries[i + 1].Team;
                var stadium = stadiumProvider(i / 2);
                var date = dateProvider(i / 2);
                var sourceHome = MatchGroup.CreateInstance(-_nextMatchId, DateTime.UtcNow.AddDays(-1), home, away, stadium, new Group("X"), Journey.CreateInstance(1));
                var sourceAway = MatchGroup.CreateInstance(-(_nextMatchId + 1), DateTime.UtcNow.AddDays(-1), away, home, stadium, new Group("Y"), Journey.CreateInstance(1));
                var matchCode = $"R32-{_nextMatchId}";
                var elim = MatchElimination.Create(_nextMatchId, date, home, away, stadium, Phase.RoundOf32, matchCode, sourceHome, sourceAway);
                result.Add(elim);
                _nextMatchId++;
            }

            return result;
        }

        public List<MatchElimination> CreateNextRoundFromMatches(IReadOnlyList<Match> previousRoundMatches, Phase nextPhase, IReadOnlyCollection<Stadium> stadiums, TournamentSettings settings, Func<int, Stadium> stadiumProvider, Func<int, DateTime> dateProvider)
        {
            if (previousRoundMatches == null) throw new ArgumentNullException(nameof(previousRoundMatches));
            var result = new List<MatchElimination>();

            for (int i = 0; i < previousRoundMatches.Count; i += 2)
            {
                var sourceHome = previousRoundMatches[i];
                var sourceAway = previousRoundMatches[i + 1];
                var home = sourceHome.HomeTeam ?? sourceHome.AwayTeam ?? throw new ArgumentException("Source match must have a team");
                var away = sourceAway.HomeTeam ?? sourceAway.AwayTeam ?? throw new ArgumentException("Source match must have a team");
                var stadium = stadiumProvider(i / 2);
                var date = dateProvider(i / 2);
                var matchCode = $"{nextPhase}-{_nextMatchId}";
                var elim = MatchElimination.Create(_nextMatchId, date, home, away, stadium, nextPhase, matchCode, sourceHome, sourceAway);
                result.Add(elim);
                _nextMatchId++;
            }

            return result;
        }
    }
}
