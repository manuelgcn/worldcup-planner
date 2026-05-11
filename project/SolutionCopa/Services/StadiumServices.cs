using DataAccess.Interfaces;
using ProjectCopa;
using Services.Interfaces;

namespace Services;

public class StadiumServices
{
    private readonly IStadiumRepository _stadiumRepository;
    private readonly IAuditLogService? _auditLogService;
    private int nextId = 1;

    public StadiumServices(IStadiumRepository stadiumRepository, IAuditLogService? auditLogService = null)
    {
        _stadiumRepository = stadiumRepository;
        this._auditLogService = auditLogService;
    }

  

    public IReadOnlyCollection<Stadium> GetAllStadiums()
    {
        return _stadiumRepository.GetAll();
    }

    public void AddStadium(Stadium stadium)
    {
        if (_stadiumRepository.ExistsByName(stadium.Name))
            throw new ArgumentException("Este Nombre de estadio ya existe");

        stadium.Id = nextId++;
        _stadiumRepository.Add(stadium);
        
        _auditLogService?.Log(
            AuditActions.CreateStadium,
            $"Stadium '{stadium.Name}' created in {stadium.City}.",
            nameof(Stadium));
    }
    
    public Stadium GetById(int id)
    {
        Stadium? stadium = _stadiumRepository.GetById(id);
        if (stadium == null)
            throw new ArgumentException("Stadium not found");
        return stadium;
    }
    
    public Stadium GetByName(string name)
    {
        Stadium? stadium = _stadiumRepository.GetByName(name);
        if (stadium == null)
            throw new ArgumentException("Stadium not found");
        return stadium;
    }
    
    public void RemoveById(int id)
    {
        Stadium stadium = GetById(id);
        _stadiumRepository.Delete(stadium);
        _auditLogService?.Log(
            AuditActions.DeleteStadium,
            $"Stadium '{stadium.Name}' removed.",
            nameof(Stadium));
    }
    
    public void UpdateById(int id, string city, string? description, int capacity)
    {
        Stadium stadium = GetById(id);
        stadium.Update(city, description, capacity);
        _stadiumRepository.Update(stadium);
        _auditLogService?.Log(
            AuditActions.UpdateStadium,
            $"Stadium '{stadium.Name}' updated.",
            nameof(Stadium));
    }
    
    public string GetCity(string stadiumName)
    {
        return GetByName(stadiumName).City;
    }

    public int GetCapacity(string stadiumName)
    {
        return GetByName(stadiumName).Capacity;
    }

    public int GetId(string stadiumName)
    {
        return GetByName(stadiumName).Id;
    }
}
