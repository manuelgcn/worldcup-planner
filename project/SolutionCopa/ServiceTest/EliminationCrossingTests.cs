using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectCopa;
using Services;
using DataAccess.Repositories;

namespace ServiceTest
{
    [TestClass]
    public class EliminationCrossingTests
    {
        [TestMethod]
        public void BuildRoundOf32_AvoidsSameGroupAndIsDeterministicWithSeed()
        {
            // Arrange: 16 groups with 2 teams each (32 teams total)
            var teamsWithGroup = new List<(Team Team, string Group)>();
            int id = 1;
            for (int g = 1; g <= 16; g++)
            {
                teamsWithGroup.Add((new Team(id++, $"T{g}A", 1000 + id, false, Confederation.UEFA), $"G{g}"));
                teamsWithGroup.Add((new Team(id++, $"T{g}B", 1000 + id, false, Confederation.UEFA), $"G{g}"));
            }

            var stadiums = new List<Stadium>
            {
                new Stadium("S1", "City1", null, 30000),
                new Stadium("S2", "City2", null, 35000),
                new Stadium("S3", "City3", null, 40000),
                new Stadium("S4", "City4", null, 45000)
            };

            var settings = new TournamentSettings(1, new DateTime(2026,6,1), 3, 3, 0, 0, 0, 0);
            int seed = 12345;

            // Act: first run
            var repo1 = new EliminationMatchRepository();
            var builder1 = new EliminationBuilder(repo1);
            builder1.BuildRoundOf32(teamsWithGroup, stadiums, settings, seed);
            var matches1 = repo1.GetAll().ToList();

            // Act: second run (should be deterministic)
            var repo2 = new EliminationMatchRepository();
            var builder2 = new EliminationBuilder(repo2);
            builder2.BuildRoundOf32(teamsWithGroup, stadiums, settings, seed);
            var matches2 = repo2.GetAll().ToList();

            // Assert: 16 matches created
            Assert.AreEqual(16, matches1.Count);
            Assert.AreEqual(16, matches2.Count);

            // Assert: no match has both teams from same group
            var mapping = teamsWithGroup.ToDictionary(t => t.Team.Id, t => t.Group);
            foreach (Match m in matches1)
            {
                var me = (MatchElimination)m;
                string gHome = mapping[me.HomeTeam!.Id];
                string gAway = mapping[me.AwayTeam!.Id];
                Assert.AreNotEqual(gHome, gAway, "Match should avoid same-group pairing when possible");
            }

            // Assert: deterministic order (team id pairs sequence equal)
            var seq1 = matches1.Select(m => (m.HomeTeam!.Id, m.AwayTeam!.Id)).ToList();
            var seq2 = matches2.Select(m => (m.HomeTeam!.Id, m.AwayTeam!.Id)).ToList();
            CollectionAssert.AreEqual(seq1, seq2);
        }
    }
}
