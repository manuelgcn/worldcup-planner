using DataAccess;
using Microsoft.EntityFrameworkCore;
using ProjectCopa;

namespace DataAccessTest.TestDoubles;

// Helpers compartidos por las suites de repositorios de fixture/partidos sobre EF Core InMemory.
//
// Construyen grafos de dominio con instancias controladas (para poder COMPARTIR equipos/estadios
// entre partidos, como hace el fixture real) y persisten los prerrequisitos en la BD que comparte
// la factory. Los repos de partidos/grupos asumen que las entidades relacionadas (equipos, estadios,
// grupos, jornadas, partidos fuente) YA existen: las attachean como Unchanged en vez de insertarlas,
// igual que en produccion (los agregados los gobiernan sus propios repositorios). Por eso los tests
// siembran primero esos prerrequisitos y recien despues ejercitan el repo bajo prueba.
internal static class EfFixtureData
{
    public static Team MakeTeam(int id, string name) =>
        new Team(id, name, 1000 + id, false, Confederation.CONMEBOL);

    public static Stadium MakeStadium(int id, string name) =>
        new Stadium(name, "Test City", null, 50000) { Id = id };

    public static Group MakeGroup(string name, params Team[] teams)
    {
        Group group = new Group(name);
        foreach (Team team in teams)
        {
            group.AddTeam(team);
        }
        return group;
    }

    public static Journey MakeJourney(int number) => Journey.CreateInstance(number);

    public static MatchGroup MakeGroupMatch(int id, DateTime date, Team home, Team away, Stadium stadium, Group group, Journey journey)
    {
        MatchGroup match = MatchGroup.CreateInstance(id, date, home, away, stadium, group, journey);
        match.SetMatchCode("M" + id);
        return match;
    }

    public static MatchElimination MakeEliminationMatch(
        int id, DateTime date, Team home, Team away, Stadium stadium, Phase phase, Match sourceHome, Match sourceAway) =>
        MatchElimination.Create(id, date, home, away, stadium, phase, "E" + id, sourceHome, sourceAway);

    // Persiste un grafo NUEVO (equipos/estadios/grupos/jornadas/partidos fuente) en la BD compartida.
    // Todo el grafo es nuevo, asi que un Add directo + SaveChanges inserta sin colisiones de PK.
    public static void Seed(IDbContextFactory<AppDbContext> factory, params object[] entities)
    {
        using AppDbContext context = factory.CreateDbContext();
        context.AddRange(entities);
        context.SaveChanges();
    }

    public static int Count<TEntity>(IDbContextFactory<AppDbContext> factory) where TEntity : class
    {
        using AppDbContext context = factory.CreateDbContext();
        return context.Set<TEntity>().Count();
    }
}
