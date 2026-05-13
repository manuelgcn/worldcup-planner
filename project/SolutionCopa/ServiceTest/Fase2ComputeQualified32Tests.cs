using ProjectCopa;
using ServiceTest.TestDoubles.Mocks;
using Services;


namespace ServiceTest
{
    [TestClass]
    public class Fase2ComputeQualified32Tests
    {
        private const int PointsForWin = 3;
        private const int PointsForDraw = 1;
        private const int BaseFifaRanking = 1000;
        private const int ExpectedQualifiedCount = 32;
        private const int ExpectedThirdPlaceCount = 8;
        private const int TeamsPerGroupCount = 4;
        private const char FirstGroupChar = 'A';
        private const char LastGroupChar = 'L';
        private const int StadiumCapacity = 30000;
 
        private static readonly DateTime DefaultStartDate = new DateTime(2026, 6, 1);
 
        [TestMethod]
        public void ComputeQualified32FromGroups_Returns32Teams_And_SelectsBest8Thirds_Deterministically()
        {
           
            MockMatchRepository matchRepo = new MockMatchRepository();
            TournamentSettings settings = new TournamentSettings(
                id: 1,
                startDate: DefaultStartDate,
                maxMatchesPerDay: 3,
                daysBetweenJourneys: 3,
                fixtureSeed: 0,
                crossingsSeed: 42,
                simulationSeed: 123,
                completionSeed: 0
            );
 
            List<Group> groups = BuildGroups();
            PopulateMatches(groups, matchRepo, settings);
 
            Fase2 fase2 = new Fase2(
                simulationEngine: null!,
                matchRepository: matchRepo,
                settingsRepository: null!,
                unitOfWork: null!
            );
 
           
            List<(Team Team, string Group)> result1 = fase2.ComputeQualified32FromGroups(groups, settings);
            List<(Team Team, string Group)> result2 = fase2.ComputeQualified32FromGroups(groups, settings);
 
            
            Assert.AreEqual(ExpectedQualifiedCount, result1.Count);
            CollectionAssert.AreEqual(result1, result2);
 
            foreach (Group group in groups)
            {
                int count = result1.Count(q => q.Group == group.Name);
                Assert.IsTrue(count == 2 || count == 3, $"El grupo {group.Name} debería tener 2 o 3 clasificados.");
            }
 
            int thirdPlacedCount = 0;
            foreach (Group group in groups)
            {
                Team third = ComputeThirdPlacedTeam(group, matchRepo);
                if (result1.Any(q => q.Team.Id == third.Id))
                {
                    thirdPlacedCount++;
                }
            }
            Assert.AreEqual(ExpectedThirdPlaceCount, thirdPlacedCount);
        }
 
        // --- Helper Methods ---
 
        private static List<Group> BuildGroups()
        {
            List<Group> groups = new List<Group>();
            int teamId = 1;
 
            for (char c = FirstGroupChar; c <= LastGroupChar; c++)
            {
                Group group = new Group(c.ToString());
                for (int i = 0; i < TeamsPerGroupCount; i++)
                {
                    Team team = new Team(
                        id: teamId,
                        name: $"T{teamId}",
                        fifaRanking: BaseFifaRanking + teamId,
                        isFictitious: false,
                        confederation: Confederation.UEFA
                    );
                    teamId++;
                    group.AddTeam(team);
                }
                groups.Add(group);
            }
 
            return groups;
        }
 
        private static void PopulateMatches(List<Group> groups, MockMatchRepository matchRepo, TournamentSettings settings)
        {
            int matchId = 1000;
            Stadium stadium = new Stadium("S", "City", string.Empty, StadiumCapacity);
            Journey journey = Journey.CreateInstance(1);
 
            foreach (Group group in groups)
            {
                List<Team> teams = group.Teams.ToList();
                
                // Usamos la fecha de inicio del torneo para que la validación del Match no falle
                DateTime matchDate = settings.StartDate;
 
                MatchGroup match1 = MatchGroup.CreateMatchWithConfig(matchId++, matchDate, teams[0], teams[1], stadium, settings, group, journey);
                match1.SetGoals(2, 0);
                matchRepo.Add(match1);
 
                MatchGroup match2 = MatchGroup.CreateMatchWithConfig(matchId++, matchDate, teams[0], teams[2], stadium, settings, group, journey);
                match2.SetGoals(2, 0);
                matchRepo.Add(match2);
 
                MatchGroup match3 = MatchGroup.CreateMatchWithConfig(matchId++, matchDate, teams[0], teams[3], stadium, settings, group, journey);
                match3.SetGoals(2, 0);
                matchRepo.Add(match3);
 
                MatchGroup match4 = MatchGroup.CreateMatchWithConfig(matchId++, matchDate, teams[1], teams[2], stadium, settings, group, journey);
                match4.SetGoals(2, 1);
                matchRepo.Add(match4);
 
                MatchGroup match5 = MatchGroup.CreateMatchWithConfig(matchId++, matchDate, teams[1], teams[3], stadium, settings, group, journey);
                match5.SetGoals(2, 1);
                matchRepo.Add(match5);
 
                MatchGroup match6 = MatchGroup.CreateMatchWithConfig(matchId++, matchDate, teams[2], teams[3], stadium, settings, group, journey);
                match6.SetGoals(2, 1);
                matchRepo.Add(match6);
            }
        }
 
        private static Team ComputeThirdPlacedTeam(Group group, MockMatchRepository repo)
        {
            Dictionary<Team, int> points = group.Teams.ToDictionary(team => team, _ => 0);
            IEnumerable<MatchGroup> matches = repo.GetByGroup(group.Name).OfType<MatchGroup>();
 
            foreach (MatchGroup match in matches)
            {
                if (match.HomeGoals > match.AwayGoals)
                {
                    points[match.HomeTeam!] += PointsForWin;
                }
                else if (match.AwayGoals > match.HomeGoals)
                {
                    points[match.AwayTeam!] += PointsForWin;
                }
                else
                {
                    points[match.HomeTeam!] += PointsForDraw;
                    points[match.AwayTeam!] += PointsForDraw;
                }
            }
 
            return points
                .OrderByDescending(keyValuePair => keyValuePair.Value)
                .Select(keyValuePair => keyValuePair.Key)
                .Skip(2)
                .First();
        }
    }

    
}