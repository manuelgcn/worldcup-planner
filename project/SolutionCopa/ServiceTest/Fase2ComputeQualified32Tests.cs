using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectCopa;
using ServiceTest.TestDoubles.Mocks;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ServiceTest
{
    [TestClass]
    public class Fase2ComputeQualified32Tests
    {
        [TestMethod]
        public void ComputeQualified32FromGroups_Returns32Teams_And_SelectsBest8Thirds_Deterministically()
        {
            MockMatchRepository matchRepo = new MockMatchRepository();

            List<Group> groups = new();
            int teamId = 1;

            // Build groups A-L with 4 teams each
            for (char c = 'A'; c <= 'L'; c++)
            {
                Group group = new Group(c.ToString());

                for (int i = 0; i < 4; i++)
                {
                    Team team = new Team(
                        id: teamId,
                        name: $"T{teamId}",
                        fifaRanking: 1000 + teamId,
                        isFictitious: false,
                        confederation: Confederation.UEFA
                    );

                    teamId++;
                    group.AddTeam(team);
                }

                groups.Add(group);
            }

            int matchId = 1000;

            // Create deterministic results per group
            foreach (Group g in groups)
            {
                Stadium stadium = new Stadium("S", "City", null, 30000);
                Journey journey = Journey.CreateInstance(1);

                List<Team> teams = g.Teams.ToList();

                void AddMatch(int h, int a, int hg, int ag)
                {
                    MatchGroup match = MatchGroup.CreateMatchWithConfig(
                        matchId++,
                        DateTime.UtcNow,
                        teams[h],
                        teams[a],
                        stadium,
                        new TournamentSettings(),
                        g,
                        journey
                    );

                    match.SetGoals(hg, ag);
                    matchRepo.Add(match);
                }

                // Ordering:
                // teams[0] first
                // teams[1] second
                // teams[2] third
                // teams[3] fourth
                AddMatch(0, 1, 2, 0);
                AddMatch(0, 2, 2, 0);
                AddMatch(0, 3, 2, 0);
                AddMatch(1, 2, 2, 1);
                AddMatch(1, 3, 2, 1);
                AddMatch(2, 3, 2, 1);
            }

            TournamentSettings settings = new TournamentSettings(
                id: 1,
                startDate: new DateTime(2026, 6, 1),
                maxMatchesPerDay: 3,
                daysBetweenJourneys: 3,
                fixtureSeed: 0,
                crossingsSeed: 42,
                simulationSeed: 123,
                completionSeed: 0
            );

            Fase2 fase2 = new Fase2(
                simulationEngine: null!,
                matchRepository: matchRepo,
                settingsRepository: null!,
                unitOfWork: null!
            );

            var result1 = fase2.ComputeQualified32FromGroups(groups, settings);
            var result2 = fase2.ComputeQualified32FromGroups(groups, settings);

            // Assert total
            Assert.AreEqual(32, result1.Count);

            // Assert determinism
            CollectionAssert.AreEqual(result1, result2);

            // Assert each group contributes 2 or 3 teams
            foreach (Group g in groups)
            {
                int count = result1.Count(q => q.Group == g.Name);
                Assert.IsTrue(count == 2 || count == 3);
            }

            // Assert exactly 8 third placed teams
            int thirdPlacedCount = 0;

            foreach (Group g in groups)
            {
                Team third = ComputeThirdPlacedTeam(g, matchRepo);

                if (result1.Any(q => q.Team.Id == third.Id))
                {
                    thirdPlacedCount++;
                }
            }

            Assert.AreEqual(8, thirdPlacedCount);
        }

        private static Team ComputeThirdPlacedTeam(Group group, MockMatchRepository repo)
        {
            Dictionary<Team, int> points = group.Teams.ToDictionary(t => t, _ => 0);

            var matches = repo.GetByGroup(group.Name).OfType<MatchGroup>();

            foreach (var m in matches)
            {
                if (m.HomeGoals > m.AwayGoals)
                {
                    points[m.HomeTeam!] += 3;
                }
                else if (m.AwayGoals > m.HomeGoals)
                {
                    points[m.AwayTeam!] += 3;
                }
                else
                {
                    points[m.HomeTeam!] += 1;
                    points[m.AwayTeam!] += 1;
                }
            }

            return points
                .OrderByDescending(p => p.Value)
                .Select(p => p.Key)
                .Skip(2)
                .First();
        }
    }
}