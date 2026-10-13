using WorldCupPlanner.Domain;

namespace WorldCupPlanner.Services;

public class QualificationService
{
    public List<Team> GetBestEightThirds(IEnumerable<(Team Team, int Pts, int GD, int GF, string Group)> thirdPlaceCandidates, int seed)
    {
        if (thirdPlaceCandidates == null) throw new ArgumentNullException(nameof(thirdPlaceCandidates));

        // Order by Pts desc, GD desc, GF desc
        var ordered = thirdPlaceCandidates
            .OrderByDescending(t => t.Pts)
            .ThenByDescending(t => t.GD)
            .ThenByDescending(t => t.GF)
            .ToList();

        List<Team> result = new List<Team>();

        int idx = 0;
        Random random = new Random(seed);

        while (result.Count < 8 && idx < ordered.Count)
        {
            // Find group of tied teams at current position based on Pts, GD, GF
            var current = ordered[idx];
            var tiedGroup = ordered.Skip(idx).TakeWhile(t => t.Pts == current.Pts && t.GD == current.GD && t.GF == current.GF).ToList();

            int remainingSlots = 8 - result.Count;

            if (tiedGroup.Count <= remainingSlots)
            {
                result.AddRange(tiedGroup.Select(t => t.Team));
            }
            else
            {
                // Need to pick a subset from tiedGroup deterministically using seed
                List<(Team Team,int Pts,int GD,int GF,string Group)> shuffled = tiedGroup.OrderBy(_ => 0).ToList();
                // Fisher-Yates on indices
                var arr = shuffled.ToArray();
                for (int i = arr.Length - 1; i > 0; i--)
                {
                    int j = random.Next(i + 1);
                    var tmp = arr[i];
                    arr[i] = arr[j];
                    arr[j] = tmp;
                }

                result.AddRange(arr.Take(remainingSlots).Select(x => x.Team));
            }

            idx += tiedGroup.Count;
        }

        return result;
    }
}