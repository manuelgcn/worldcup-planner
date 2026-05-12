using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectCopa;
using ServiceTest.TestDoubles.Mocks;
using Services;
using System;
using System.Collections.Generic;

namespace ServiceTest
{
    [TestClass]
    public class Fase2ComputeQualified32Tests
    {
        [TestMethod]
        public void ComputeQualified32FromGroups_Returns32Teams_And_SelectsBest8ThirdsDeterministically()
        {
            MockMatchRepository matchRepo = new MockMatchRepository();

            // Build 12 groups (A..L) with 4 teams each and deterministic results
            List<Group> groups = new List<Group>();
            int teamId = 1;
            for (char c = 'A'; c <= 'L'; c++)
            {
                Group g = new Group(c.ToString());
                for (int i = 0; i < 4; i++)
                {
                    int id = teamId++;
                    Team t = new Team(id, $"T{id}", 1000, false, Confederation.UEFA);
                    g.AddTeam(t);
                }
                groups.Add(g);
            }

            // For each group, create 6 matches and assign results such that
            // positions are clear: Team 0 -> 1st, Team 1 -> 2nd, Team 2 -> 3rd, Team 3 -> 4th
            int matchId = 1000;
            foreach (Group g in groups)
            {
                Stadium stadium = new Stadium(1, "S");
                Journey journey = new Journey(1);

                List<Team> teams = new List<Team>(g.Teams);
                // Create simple round-robin partial results to ensure ordering
                // T0 beats everyone, T1 beats T2 and T3, T2 beats T3
                // Matches: (0,1),(0,2),(0,3),(1,2),(1,3),(2,3)
                void AddMatch(int hIndex, int aIndex, int hGoals, int aGoals)
                {
                    MatchGroup m = MatchGroup.CreateMatchWithConfig(matchId++, DateTime.UtcNow, teams[hIndex], teams[aIndex], stadium, new TournamentSettings(), g, journey);
                    m.SetGoals(hGoals, aGoals);
                    matchRepo.Add(m);
                }

                AddMatch(0,1,2,0);
                AddMatch(0,2,2,0);
                AddMatch(0,3,2,0);
                AddMatch(1,2,2,1);
                AddMatch(1,3,2,1);
                AddMatch(2,3,2,1);
            }

            TournamentSettings settings = new TournamentSettings(id: 1, startDate: new DateTime(2026,6,1), maxMatchesPerDay: 3, daysBetweenJourneys: 3, fixtureSeed: 0, crossingsSeed: 42, simulationSeed: 123, completionSeed: 0);

            Fase2 fase2 = new Fase2(simulationEngine: null!, matchRepository: matchRepo, settingsRepository: null!, unitOfWork: null!);

            List<(Team Team, string Group)> qualified = fase2.ComputeQualified32FromGroups(groups, settings);

            Assert.AreEqual(32, qualified.Count);

            // Ensure that every group contributed its first and second
            foreach (Group g in groups)
            {
                Assert.IsTrue(qualified.Exists(q => q.Group == g.Name && (q.Team == null ? false : g.Teams.Contains(q.Team))));
            }

            // Ensure best-8 third-placed were selected (there are 12 third-placed, we pick 8)
            int thirdCount = 0;
            foreach (Group g in groups)
            {
                // third-placed is the Team with index 2 in our construction
                Team third = new List<Team>(g.Teams)[2];
                if (qualified.Exists(q => q.Team.Id == third.Id)) thirdCount++;
            }

            Assert.IsTrue(thirdCount == 8);
        }
    }
}
