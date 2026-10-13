using WorldCupPlanner.Domain;
using WorldCupPlanner.Domain.Models;

namespace WorldCupPlanner.Services;

public class EliminationBuilder
{
    private const int TopFirstsCount = 8;
    private const int RemainingFirstsCount = 4;
    private const int TopSecondsCount = 8;
    private const int WorstSecondsCount = 4;
    private const int BestThirdsCount = 8;

    private int _nextMatchId = 2000;

    // H2: construye el R32 respetando las categorias de CLAUDE.md §7 a partir de QualifiedTeams,
    // sin aplanar a (Team, Group) y perder la posicion (1ro/2do/3ro):
    //   A1..A8 = 8 mejores 1ros vs 8 mejores 3ros
    //   B1..B4 = 4 primeros restantes vs 4 segundos peores
    //   B5..B8 = 8 mejores segundos emparejados entre si
    // El CrossingsSeed (Fisher-Yates) solo sortea la asignacion dentro de cada categoria y resuelve
    // los choques de mismo grupo; no baraja los 32 indiscriminadamente. La salida queda intercalada
    // A1,B1,A2,B2,...,A8,B8 para que el bracket post-32 (C1..C8 = ganadores Ai vs Bi) encaje con el
    // emparejamiento adyacente de las rondas siguientes.
    public List<MatchElimination> CreateRoundOf32(QualifiedTeams qualified, IReadOnlyCollection<Stadium> stadiums, TournamentSettings settings, int crossingsSeed, Func<int, Stadium> stadiumProvider, Func<int, DateTime> dateProvider)
    {
        ArgumentNullException.ThrowIfNull(qualified);
        ValidateCategorySizes(qualified);

        Random random = new Random(crossingsSeed);
        Dictionary<int, string> groupByTeamId = qualified.GroupByTeamId;

        List<(Team Home, Team Away)> aMatches = PairAcross(qualified.TopEightFirsts, qualified.BestEightThirds, groupByTeamId, random);
        List<(Team Home, Team Away)> b1ToB4 = PairAcross(qualified.RemainingFourFirsts, qualified.WorstFourSeconds, groupByTeamId, random);
        List<(Team Home, Team Away)> b5ToB8 = PairWithin(qualified.TopEightSeconds, groupByTeamId, random);
        List<(Team Home, Team Away)> bMatches = b1ToB4.Concat(b5ToB8).ToList();

        List<(Team Home, Team Away, string Code)> ordered = new(aMatches.Count + bMatches.Count);
        for (int i = 0; i < aMatches.Count; i++)
        {
            ordered.Add((aMatches[i].Home, aMatches[i].Away, $"A{i + 1}"));
            ordered.Add((bMatches[i].Home, bMatches[i].Away, $"B{i + 1}"));
        }

        List<MatchElimination> result = new List<MatchElimination>(ordered.Count);
        for (int i = 0; i < ordered.Count; i++)
        {
            Stadium stadium = stadiumProvider(i);
            DateTime date = dateProvider(i);
            MatchElimination elim = MatchElimination.CreateInitialRound(
                _nextMatchId, date, ordered[i].Home, ordered[i].Away, stadium, settings, Phase.RoundOf32, ordered[i].Code);
            result.Add(elim);
            _nextMatchId++;
        }

        return result;
    }

    private static void ValidateCategorySizes(QualifiedTeams qualified)
    {
        if (qualified.TopEightFirsts.Count != TopFirstsCount
            || qualified.RemainingFourFirsts.Count != RemainingFirstsCount
            || qualified.TopEightSeconds.Count != TopSecondsCount
            || qualified.WorstFourSeconds.Count != WorstSecondsCount
            || qualified.BestEightThirds.Count != BestThirdsCount)
        {
            throw new ArgumentException("QualifiedTeams debe tener 8 primeros top, 4 primeros restantes, 8 segundos top, 4 segundos peores y 8 mejores terceros.");
        }
    }

    // Empareja homes[i] con un away, barajando los aways con la semilla y resolviendo los choques de
    // mismo grupo por intercambio (best-effort). Conserva el orden de homes (A1..A8 / B1..B4).
    private static List<(Team Home, Team Away)> PairAcross(List<Team> homes, List<Team> aways, Dictionary<int, string> groupByTeamId, Random random)
    {
        List<Team> pool = Shuffle(aways, random);
        AvoidSameGroupCollisions(homes, pool, groupByTeamId);
        return homes.Select((home, index) => (home, pool[index])).ToList();
    }

    // Empareja una lista par de equipos entre si (B5..B8), evitando mismo grupo cuando es posible.
    private static List<(Team Home, Team Away)> PairWithin(List<Team> teams, Dictionary<int, string> groupByTeamId, Random random)
    {
        List<Team> pool = Shuffle(teams, random);
        List<Team> homes = new();
        List<Team> aways = new();
        for (int i = 0; i < pool.Count; i += 2)
        {
            homes.Add(pool[i]);
            aways.Add(pool[i + 1]);
        }

        AvoidSameGroupCollisions(homes, aways, groupByTeamId);
        return homes.Select((home, index) => (home, aways[index])).ToList();
    }

    // Reordena 'aways' in-place para que ningun par homes[i]/aways[i] comparta grupo, cuando exista
    // un intercambio que lo resuelva sin crear un nuevo choque.
    private static void AvoidSameGroupCollisions(List<Team> homes, List<Team> aways, Dictionary<int, string> groupByTeamId)
    {
        for (int i = 0; i < homes.Count; i++)
        {
            if (!SameGroup(homes[i], aways[i], groupByTeamId))
            {
                continue;
            }

            for (int j = 0; j < aways.Count; j++)
            {
                if (j == i)
                {
                    continue;
                }

                if (!SameGroup(homes[i], aways[j], groupByTeamId) && !SameGroup(homes[j], aways[i], groupByTeamId))
                {
                    (aways[i], aways[j]) = (aways[j], aways[i]);
                    break;
                }
            }
        }
    }

    private static bool SameGroup(Team a, Team b, Dictionary<int, string> groupByTeamId)
    {
        return groupByTeamId.TryGetValue(a.Id, out string? groupA)
            && groupByTeamId.TryGetValue(b.Id, out string? groupB)
            && groupA == groupB;
    }

    private static List<Team> Shuffle(List<Team> source, Random random)
    {
        List<Team> copy = new(source);
        for (int i = copy.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (copy[i], copy[j]) = (copy[j], copy[i]);
        }

        return copy;
    }

    public List<MatchElimination> CreateRoundOf32(IEnumerable<(Team Team, string Group)> entriesWithGroup, IReadOnlyCollection<Stadium> stadiums, TournamentSettings settings, int crossingsSeed, Func<int, Stadium> stadiumProvider, Func<int, DateTime> dateProvider)
    {
        if (entriesWithGroup == null) throw new ArgumentNullException(nameof(entriesWithGroup));
        List<(Team Team, string Group)> entries = entriesWithGroup.ToList();
        if (entries.Count != 32) throw new ArgumentException("RoundOf32 requires exactly 32 teams");

        Random random = new Random(crossingsSeed);

        for (int i = entries.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (Team Team, string Group) tmp = entries[i];
            entries[i] = entries[j];
            entries[j] = tmp;
        }

        // Interleave by group to guarantee no same-group pairings.
        // Groups with more qualified teams are processed first so their teams
        // spread across different slots before lighter groups run out of slots.
        List<List<(Team Team, string Group)>> byGroup = entries
            .GroupBy(e => e.Group)
            .OrderByDescending(g => g.Count())
            .Select(g => g.ToList())
            .ToList();

        List<(Team Team, string Group)> interleaved = new List<(Team Team, string Group)>(entries.Count);
        int maxSlot = byGroup.Max(g => g.Count);
        for (int slot = 0; slot < maxSlot; slot++)
        {
            foreach (List<(Team Team, string Group)> group in byGroup)
            {
                if (slot < group.Count)
                    interleaved.Add(group[slot]);
            }
        }

        List<MatchElimination> result = new List<MatchElimination>();
        for (int i = 0; i < interleaved.Count; i += 2)
        {
            Team home = interleaved[i].Team;
            Team away = interleaved[i + 1].Team;
            Stadium stadium = stadiumProvider(i / 2);
            DateTime date = dateProvider(i / 2);
            string matchCode = $"R32-{_nextMatchId}";
            // R32 es la ronda inicial: los emparejamientos salen de la clasificacion de grupos, no de
            // partidos previos, asi que se crea SIN partidos fuente. Antes se fabricaban fuentes
            // sinteticas con ids negativos (y grupos "X"/"Y") solo para satisfacer la invariante del
            // dominio; con EF Core eso provocaba una violacion de FK al persistir el R32.
            MatchElimination elim = MatchElimination.CreateInitialRound(_nextMatchId, date, home, away, stadium, settings, Phase.RoundOf32, matchCode);
            result.Add(elim);
            _nextMatchId++;
        }

        return result;
    }

    public List<MatchElimination> CreateNextRoundFromMatches(IReadOnlyList<Match> previousRoundMatches, Phase nextPhase, IReadOnlyCollection<Stadium> stadiums, TournamentSettings settings, Func<int, Stadium> stadiumProvider, Func<int, DateTime> dateProvider)
    {
        if (previousRoundMatches == null) throw new ArgumentNullException(nameof(previousRoundMatches));
        List<MatchElimination> result = new List<MatchElimination>();

        for (int i = 0; i < previousRoundMatches.Count; i += 2)
        {
            Match sourceHome = previousRoundMatches[i];
            Match sourceAway = previousRoundMatches[i + 1];
            Team home = sourceHome.HomeTeam ?? sourceHome.AwayTeam ?? throw new ArgumentException("El partido de origen debe tener un equipo");
            Team away = sourceAway.HomeTeam ?? sourceAway.AwayTeam ?? throw new ArgumentException("El partido de origen debe tener un equipo");
            Stadium stadium = stadiumProvider(i / 2);
            DateTime date = dateProvider(i / 2);
            string matchCode = $"{nextPhase}-{_nextMatchId}";
            MatchElimination elim = MatchElimination.CreateMatchWithConfig(_nextMatchId, date, home, away, stadium, settings, nextPhase, matchCode, sourceHome, sourceAway);
            result.Add(elim);
            _nextMatchId++;
        }

        return result;
    }
}