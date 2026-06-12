using DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using ProjectCopa;
using Services.Exceptions;
using Services.Interfaces;

namespace Services;

public class StadiumServices : IStadiumService
{
    private readonly IStadiumRepository stadiumRepository;
    private readonly IUnitOfWork unitOfWork;
    private readonly IAuditLogService auditLogService;

    private const string DuplicateStadiumNameMessage = "Stadium name must be unique in the system.";
    private const string StadiumNotFoundMessage = "Stadium was not found.";
    private const string StadiumReferencedByMatchMessage = "Stadium cannot be deleted because it is referenced by one or more matches.";
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

    public StadiumServices(IStadiumRepository stadiumRepository, IUnitOfWork unitOfWork, IAuditLogService auditLogService)
    {
        ArgumentNullException.ThrowIfNull(auditLogService);
        this.stadiumRepository = stadiumRepository;
        this.unitOfWork = unitOfWork;
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

        unitOfWork.Commit();
    }

    public void AddStadium(Stadium stadium)
    {
        ArgumentNullException.ThrowIfNull(stadium);

        if (stadiumRepository.ExistsByName(stadium.Name))
        {
            throw new StadiumServicesException(DuplicateStadiumNameMessage);
        }

        stadiumRepository.Add(stadium);
        
        auditLogService.Log(
            AuditActions.CreateStadium,
            $"Stadium '{stadium.Name}' created in {stadium.City}.",
            nameof(Stadium));
        unitOfWork.Commit();
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
    
    public void RemoveById(int id)
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
        auditLogService.Log(
            AuditActions.DeleteStadium,
            $"Stadium '{stadium.Name}' removed.",
            nameof(Stadium));
        unitOfWork.Commit();
    }
    
    public void UpdateById(int id, string city, string? description, int capacity)
    {
        Stadium stadium = GetById(id);
        stadium.Update(city, description, capacity);
        stadiumRepository.Update(stadium);
        auditLogService.Log(
            AuditActions.UpdateStadium,
            $"Stadium '{stadium.Name}' updated.",
            nameof(Stadium));
        unitOfWork.Commit();
    }
    
}
