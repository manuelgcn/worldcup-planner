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
    public class EliminationBuilderDefaultStadiumTests
    {
        [TestMethod]
        public void BuildRoundOf32_UsesDefaultStadiumWhenNoneProvided()
        {
            var teams = new List<(Team Team, string Group)>();
            int id = 1;
            for (int g = 1; g <= 16; g++)
            {
                teams.Add((new Team(id++, $"T{g}A", 1000 + id, false, Confederation.UEFA), $"G{g}"));
                teams.Add((new Team(id++, $"T{g}B", 1000 + id, false, Confederation.UEFA), $"G{g}"));
            }

            var repo = new EliminationMatchRepository();
            var builder = new EliminationBuilder(repo);

            builder.BuildRoundOf32(teams, new List<Stadium>(), new TournamentSettings(), 0);

            var matches = repo.GetAll().ToList();
            Assert.AreEqual(16, matches.Count);

            foreach (var m in matches)
            {
                Assert.IsNotNull(m.Stadium);
                Assert.AreEqual("Default", m.Stadium!.Name);
            }
        }
    }
}
