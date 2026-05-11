using System;
using System.Collections.Generic;
using System.Linq;
using DataAccess.Interfaces;
using ProjectCopa;

namespace Services
{
    public class EliminationBuilder
    {
        private readonly IEliminationMatchRepository _eliminationRepo;
        private int _nextMatchId = 1000;

        public EliminationBuilder(IEliminationMatchRepository eliminationRepo)
        {
            _eliminationRepo = eliminationRepo;
        }

        // Pairs teams sequentially to build Round of 32 elimination matches.
        public void BuildRoundOf32(IEnumerable<Team> qualifiedTeams, IReadOnlyCollection<Stadium> stadiums, TournamentSettings settings)
        {
            if (qualifiedTeams == null) throw new ArgumentNullException(nameof(qualifiedTeams));
            var teams = qualifiedTeams.ToList();
            if (teams.Count != 32) throw new ArgumentException("RoundOf32 requires exactly 32 teams");

            var stadiumList = stadiums?.ToList() ?? new List<Stadium>();
            int stadiumIndex = 0;

            for (int i = 0; i < teams.Count; i += 2)
            {
                Team home = teams[i];
                Team away = teams[i + 1];
                Stadium stadium = stadiumList.Count > 0 ? stadiumList[stadiumIndex % stadiumList.Count] : new Stadium("Default", "City", null, 30000);

                // create dummy source matches for the teams as group placeholders
                var sourceHome = MatchGroup.CreateInstance(-_nextMatchId, DateTime.UtcNow.AddDays(-1), home, away, stadium, new Group("X"), new Journey());
                var sourceAway = MatchGroup.CreateInstance(-(_nextMatchId+1), DateTime.UtcNow.AddDays(-1), away, home, stadium, new Group("Y"), new Journey());

                var matchCode = $"R32-{_nextMatchId}";
                var elim = MatchElimination.Create(_nextMatchId, settings.StartDate.AddDays(0), home, away, stadium, Phase.RoundOf32, matchCode, sourceHome, sourceAway);

                _eliminationRepo.Add(elim);

                _nextMatchId++;
                stadiumIndex++;
            }
        }
    }
}
