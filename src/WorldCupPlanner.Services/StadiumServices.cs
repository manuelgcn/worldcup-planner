using WorldCupPlanner.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using WorldCupPlanner.Domain;
using WorldCupPlanner.Services.Exceptions;
using WorldCupPlanner.Services.Interfaces;

namespace WorldCupPlanner.Services;

public class StadiumServices : IStadiumService
{
    private readonly IStadiumRepository stadiumRepository;
    private readonly IAuditLogService auditLogService;

    private const string DuplicateStadiumNameMessage = "El nombre del estadio debe ser unico en el sistema.";
    private const string StadiumNotFoundMessage = "No se encontro el estadio.";
    private const string StadiumReferencedByMatchMessage = "No se puede eliminar el estadio porque esta referenciado por uno o mas partidos.";
    private const int MinimumStadiumsForFixture = 4;

    private static readonly (string Name, string City, int Capacity)[] DefaultStadiumSeeds =
    {
        ("MetLife Stadium", "East Rutherford", 82500),
        ("AT&T Stadium", "Arlington", 80000),
        ("SoFi Stadium", "Inglewood", 70000),
        ("Estadio Azteca", "Ciudad de Mexico", 87000),
        ("BMO Field", "Toronto", 30000),
        ("Mercedes-Benz Stadium", "Atlanta", 71000),
        ("Hard Rock Stadium", "Miami Gardens", 65000),
        ("Estadio BBVA", "Monterrey", 51000)
    };

    public StadiumServices(IStadiumRepository stadiumRepository, IAuditLogService auditLogService)
    {
        ArgumentNullException.ThrowIfNull(auditLogService);
        this.stadiumRepository = stadiumRepository;
        this.auditLogService = auditLogService;
    }

    public IReadOnlyCollection<Stadium> GetAllStadiums()
    {
        return stadiumRepository.GetAll();
    }

    public void SeedDefaultStadiums(string? performedByEmail)
    {
        IReadOnlyCollection<Stadium> existing = stadiumRepository.GetAll();
        if (existing.Count >= MinimumStadiumsForFixture)
        {
            return;
        }

        HashSet<string> existingNames = existing.Select(stadium => stadium.Name).ToHashSet();
        int nextId = existing.Count == 0 ? 1 : existing.Max(stadium => stadium.Id) + 1;
        int addedSoFar = existing.Count;

        foreach ((string name, string city, int capacity) in DefaultStadiumSeeds)
        {
            if (addedSoFar >= MinimumStadiumsForFixture)
            {
                break;
            }

            if (existingNames.Contains(name))
            {
                continue;
            }

            Stadium stadium = new Stadium(name, city, null, capacity) { Id = nextId };
            stadiumRepository.Add(stadium);
            auditLogService.LogPerformedBy(
                AuditActions.CreateStadium,
                $"Generated default stadium '{stadium.Name}' in {stadium.City}.",
                performedByEmail,
                nameof(Stadium),
                stadium.Id);
            nextId++;
            addedSoFar++;
        }
    }

    public void AddStadium(Stadium stadium, string? performedByEmail = null)
    {
        ArgumentNullException.ThrowIfNull(stadium);

        if (stadiumRepository.ExistsByName(stadium.Name))
        {
            throw new StadiumServicesException(DuplicateStadiumNameMessage);
        }

        // La clave es ValueGeneratedNever: la UI crea estadios con Id 0, por lo que
        // dos altas consecutivas chocarian en la PK. Asignamos Id = max+1 (igual que el
        // seed por defecto y ImportService) cuando no llega un Id explicito.
        if (stadium.Id <= 0)
        {
            stadium.Id = NextStadiumId();
        }

        stadiumRepository.Add(stadium);

        auditLogService.LogPerformedBy(
            AuditActions.CreateStadium,
            $"Stadium '{stadium.Name}' created in {stadium.City}.",
            performedByEmail,
            nameof(Stadium),
            stadium.Id);
    }
    
    private int NextStadiumId()
    {
        IReadOnlyCollection<Stadium> existing = stadiumRepository.GetAll();
        return existing.Count == 0 ? 1 : existing.Max(stadium => stadium.Id) + 1;
    }

    public Stadium GetById(int id)
    {
        return stadiumRepository.GetById(id)
               ?? throw new StadiumServicesException(StadiumNotFoundMessage);
    }
    
    public Stadium GetByName(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        return stadiumRepository.GetByName(name)
               ?? throw new StadiumServicesException(StadiumNotFoundMessage);
    }
    
    public void RemoveById(int id, string? performedByEmail = null)
    {
        Stadium stadium = GetById(id);
        try
        {
            stadiumRepository.Delete(stadium);                          // FK Restrict: SaveChanges falla si un partido lo referencia
        }
        catch (DbUpdateException)
        {
            throw new StadiumServicesException(StadiumReferencedByMatchMessage);
        }
        auditLogService.LogPerformedBy(
            AuditActions.DeleteStadium,
            $"Stadium '{stadium.Name}' removed.",
            performedByEmail,
            nameof(Stadium),
            stadium.Id);
    }

    public void UpdateById(int id, string city, string? description, int capacity, string? performedByEmail = null)
    {
        Stadium stadium = GetById(id);
        stadium.Update(city, description, capacity);
        stadiumRepository.Update(stadium);
        auditLogService.LogPerformedBy(
            AuditActions.UpdateStadium,
            $"Stadium '{stadium.Name}' updated.",
            performedByEmail,
            nameof(Stadium),
            stadium.Id);
    }
    
}
