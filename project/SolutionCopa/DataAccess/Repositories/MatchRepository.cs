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

    public List<Match> Search(MatchQueryCriteria criteria)
    {
        throw new NotImplementedException();
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
