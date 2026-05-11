using ProjectCopa;
using Services.Interfaces;

namespace Services;

public class StadiumServices
{
    private readonly IAuditLogService? _auditLogService;
    private List<Stadium> stadiums = new();

    public StadiumServices(IAuditLogService? auditLogService = null)
    {
        this._auditLogService = auditLogService;
    }

    public IReadOnlyCollection<Stadium> GetAllStadiums()
    {
        return stadiums.AsReadOnly();
    }

    public void AddStadium(Stadium stadium)
    {
        if (stadiums.Any(s => s.Name == stadium.Name))
            throw new ArgumentException("Este Nombre de estadio ya existe");

        stadiums.Add(stadium);
        _auditLogService?.Log(
            AuditActions.CreateStadium,
            $"Stadium '{stadium.Name}' created in {stadium.City}.",
            nameof(Stadium));
    }
    
    
    public Stadium GetByName(string name)
    {
        Stadium stadium = stadiums.FirstOrDefault(s => s.Name == name);

        if (stadium == null)
            throw new ArgumentException("Stadium not found");

        return stadium;
    }
    
    
    public void Remove(string name)
    {
        Stadium stadium = GetByName(name);
        stadiums.Remove(stadium);
        _auditLogService?.Log(
            AuditActions.DeleteStadium,
            $"Stadium '{stadium.Name}' removed.",
            nameof(Stadium));
    }
    
    public void Update(string name, string city, string? description, int capacity)
    {
        Stadium stadium = GetByName(name);
        stadium.Update(city, description, capacity);
        _auditLogService?.Log(
            AuditActions.UpdateStadium,
            $"Stadium '{stadium.Name}' updated.",
            nameof(Stadium));
    }



}
