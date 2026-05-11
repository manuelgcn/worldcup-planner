using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectCopa;
using Services;
using DataAccess.Repositories;

namespace ServiceTest
{
    [TestClass]
    public class EliminationBuilderArgumentTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BuildRoundOf32_ThrowsOnNullQualifiedTeams()
        {
            var repo = new EliminationMatchRepository();
            var builder = new EliminationBuilder(repo);
            builder.BuildRoundOf32((IEnumerable<(Team Team, string Group)>)null!, new List<Stadium>(), new TournamentSettings(), 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void BuildRoundOf32_ThrowsOnWrongTeamCount()
        {
            var repo = new EliminationMatchRepository();
            var builder = new EliminationBuilder(repo);
            var teams = new List<(Team, string)> { (new Team(1, "T1", 1000, false, Confederation.UEFA), "G1") };
            builder.BuildRoundOf32(teams, new List<Stadium>(), new TournamentSettings(), 0);
        }
    }
}
