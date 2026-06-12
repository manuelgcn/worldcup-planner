using DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using ProjectCopa;

namespace DataAccess.Repositories;

// Repositorio polimorfico sobre la tabla TPH "Matches" (mismo patron que el resto: IDbContextFactory,
// autosave por operacion, lecturas AsNoTrackingWithIdentityResolution con el grafo incluido y filtros
// resueltos en la base). La logica EF delicada vive en MatchEf.
public class MatchRepository : IMatchRepository
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public MatchRepository(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public bool ExistsById(int matchId)
    {
        using AppDbContext context = _contextFactory.CreateDbContext();
        return context.Matches.AsNoTracking().Any(match => match.MatchId == matchId);
    }

    public void Add(Match match) => MatchEf.Add(_contextFactory, match);

    public void Update(Match match) => MatchEf.Update(_contextFactory, match);

    public void Delete(Match match) => MatchEf.Delete(_contextFactory, match.MatchId);

    public Match? GetById(int matchId)
    {
        using AppDbContext context = _contextFactory.CreateDbContext();
        return MatchEf.WithGraph(context.Matches).FirstOrDefault(match => match.MatchId == matchId);
    }

    public List<Match> GetByGroup(string groupName)
    {
        using AppDbContext context = _contextFactory.CreateDbContext();
        return MatchEf.WithGraph(context.Matches)
            .OfType<MatchGroup>()
            .Where(match => match.Group != null && match.Group.Name == groupName)
            .Cast<Match>()
            .ToList();
    }

    public List<Match> GetByJourney(int journeyNumber)
    {
        using AppDbContext context = _contextFactory.CreateDbContext();
        return MatchEf.WithGraph(context.Matches)
            .OfType<MatchGroup>()
            .Where(match => match.Journey != null && match.Journey.Number == journeyNumber)
            .Cast<Match>()
            .ToList();
    }

    public List<Match> GetByStadium(int stadiumId)
    {
        using AppDbContext context = _contextFactory.CreateDbContext();
        return MatchEf.WithGraph(context.Matches)
            .Where(match => match.Stadium != null && match.Stadium.Id == stadiumId)
            .ToList();
    }

    public List<Match> GetByPhase(Phase phase)
    {
        using AppDbContext context = _contextFactory.CreateDbContext();
        return MatchEf.WithGraph(context.Matches)
            .OfType<MatchElimination>()
            .Where(match => match.Phase == phase)
            .Cast<Match>()
            .ToList();
    }

    public List<Match> GetByDateRange(DateTime from, DateTime to)
    {
        using AppDbContext context = _contextFactory.CreateDbContext();
        return MatchEf.WithGraph(context.Matches)
            .Where(match => match.MatchDate >= from && match.MatchDate <= to)
            .ToList();
    }

    // Resuelve TODOS los filtros en una sola consulta sobre la jerarquia TPH "Matches":
    // construye el IQueryable, aplica los Where opcionales ANTES de materializar y recien
    // ahi hace ToList. No combina EliminationRepository (la TPH ya trae las eliminatorias),
    // por eso no hay duplicacion. From es inclusivo y To exclusivo (lo fija el caso de uso).
    public List<Match> Search(MatchQueryCriteria criteria)
    {
        using AppDbContext context = _contextFactory.CreateDbContext();
        IQueryable<Match> query = MatchEf.WithGraph(context.Matches);

        if (criteria.From.HasValue)
        {
            query = query.Where(match => match.MatchDate >= criteria.From.Value);
        }

        if (criteria.To.HasValue)
        {
            query = query.Where(match => match.MatchDate < criteria.To.Value);
        }

        if (criteria.StadiumId.HasValue)
        {
            query = query.Where(match => match.Stadium != null && match.Stadium.Id == criteria.StadiumId.Value);
        }

        if (!string.IsNullOrEmpty(criteria.GroupName))
        {
            query = query.Where(match =>
                match is MatchGroup && ((MatchGroup)match).Group != null && ((MatchGroup)match).Group!.Name == criteria.GroupName);
        }

        if (criteria.Phase.HasValue)
        {
            query = query.Where(match =>
                match is MatchElimination && ((MatchElimination)match).Phase == criteria.Phase.Value);
        }

        return query.ToList();
    }

    public List<Match> GetAll()
    {
        using AppDbContext context = _contextFactory.CreateDbContext();
        return MatchEf.WithGraph(context.Matches).ToList();
    }

    public int NextIncidentId()
    {
        using AppDbContext context = _contextFactory.CreateDbContext();
        int maxId = context.Set<MatchIncident>().AsNoTracking()
            .Select(incident => (int?)incident.Id)
            .Max() ?? 0;
        return maxId + 1;
    }

    public void Clear()
    {
        using AppDbContext context = _contextFactory.CreateDbContext();
        // RemoveRange sobre el DbSet trackeado (no ExecuteDelete: InMemory no lo soporta). Cargar todo
        // a tracking permite que ClientSetNull nullee las autorreferencias MatchHome/MatchAway antes de borrar.
        context.Matches.RemoveRange(context.Matches.ToList());
        context.SaveChanges();
    }
}
