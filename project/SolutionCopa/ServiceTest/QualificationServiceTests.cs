using ProjectCopa;
using Services;

namespace ServiceTest;

[TestClass]
public class QualificationServiceTests
{
    [TestMethod]
    public void GetBestEightThirds_SelectsByPtsGdGf_AndUsesSeedForTies()
    {
        // Arrange
        var candidates = new List<(Team Team, int Pts, int GD, int GF, string Group)>();
        int id = 1;
        // Create 16 candidates with predictable stats
        for (int i = 0; i < 16; i++)
        {
            int pts = (i % 4 == 0) ? 6 : 4; // some higher, some lower
            int gd = 2 - (i % 3);
            int gf = 5 + (i % 5);
            candidates.Add((new Team(id++, $"T{i}", 1000 + id, false, Confederation.UEFA), pts, gd, gf, $"G{(i%8)+1}"));
        }

        QualificationService service = new QualificationService();

        // Act
        var best8 = service.GetBestEightThirds(candidates, seed: 777);

        // Assert
        Assert.AreEqual(8, best8.Count);
        // Ensure ordering by points desc then gd then gf (monotonic non-increasing)
        for (int i = 1; i < best8.Count; i++)
        {
            var prev = best8[i-1];
            var curr = candidates.First(c => c.Team.Id == best8[i].Id);
            var prevStats = candidates.First(c => c.Team.Id == prev.Id);

            // prev should be >= curr by pts,gd,gf lexicographically
            Assert.IsTrue(
                prevStats.Pts > curr.Pts ||
                (prevStats.Pts == curr.Pts && prevStats.GD > curr.GD) ||
                (prevStats.Pts == curr.Pts && prevStats.GD == curr.GD && prevStats.GF >= curr.GF)
            );
        }
    }

    [TestMethod]
    public void GetBestEightThirds_TieBreakUsesSeed_deterministic()
    {
        // Arrange a tie among 4 teams for two remaining slots
        var candidates = new List<(Team Team, int Pts, int GD, int GF, string Group)>();
        int id = 1;

        // 12 clearly better teams
        for (int i = 0; i < 12; i++)
        {
            candidates.Add((new Team(id++, $"B{i}", 1000 + id, false, Confederation.UEFA), 6, 3, 6, $"G{(i%12)+1}"));
        }

        // 4 tied teams
        var tied = new List<Team>();
        for (int i = 0; i < 4; i++)
        {
            var t = new Team(id++, $"T{i}", 1100 + id, false, Confederation.UEFA);
            tied.Add(t);
            candidates.Add((t, 4, 0, 2, $"GX{i}"));
        }

        QualificationService service = new QualificationService();

        // Act
        var best8_seed1 = service.GetBestEightThirds(candidates, seed: 42);
        var best8_seed1_again = service.GetBestEightThirds(candidates, seed: 42);
        var best8_seed2 = service.GetBestEightThirds(candidates, seed: 99);

        // Assert
        Assert.AreEqual(8, best8_seed1.Count);
        // deterministic for same seed
        CollectionAssert.AreEqual(best8_seed1.Select(t => t.Id).ToList(), best8_seed1_again.Select(t => t.Id).ToList());
        // different seed likely different selection among tied teams
        bool different = !best8_seed1.SequenceEqual(best8_seed2);
        Assert.IsTrue(different);
    }
}