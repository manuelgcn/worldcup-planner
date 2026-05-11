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
    public class EliminationBuilderTests
    {
        [TestMethod]
        public void BuildRoundOf32_Creates_16_EliminationMatches()
        {
            // Arrange: create 32 teams
            var teams = new List<Team>();
            for (int i = 1; i <= 32; i++)
            {
                teams.Add(new Team(i, $"T{i}", 1000 + i, false, Confederation.UEFA));
            }

            var stadiums = new List<Stadium>
            {
                new Stadium("S1", "City1", null, 30000),
                new Stadium("S2", "City2", null, 35000),
                new Stadium("S3", "City3", null, 40000),
                new Stadium("S4", "City4", null, 45000)
            };

            var settings = new TournamentSettings(1, new DateTime(2026,6,1), 3, 3, fixtureSeed:0, crossingsSeed:0, simulationSeed:0, completionSeed:0);

            var repo = new EliminationMatchRepository();
            var builder = new EliminationBuilder(repo);

            // Act
            builder.BuildRoundOf32(teams, stadiums, settings);

            // Assert
            var all = repo.GetAll();
            Assert.AreEqual(16, all.Count, "Should create 16 elimination matches for Round of 32");
            foreach (var m in all)
            {
                Assert.IsInstanceOfType(m, typeof(MatchElimination));
                var me = (MatchElimination)m;
                Assert.AreEqual(Phase.RoundOf32, me.Phase);
            }
        }
    }
}
