using DataAccess.Interfaces;
using ProjectCopa;
using Services.Interfaces;

namespace Services;

public class StadiumServices(IStadiumRepository stadiumRepository, IUnitOfWork unitOfWork, IAuditLogService? auditLogService = null)
{
    private int nextId = 1;

    public IReadOnlyCollection<Stadium> GetAllStadiums()
    {
        return stadiumRepository.GetAll();
    }

    public void AddStadium(Stadium stadium)
    {
        if (stadiumRepository.ExistsByName(stadium.Name))
            throw new ArgumentException("Este Nombre de estadio ya existe");

        stadium.Id = nextId++;
        stadiumRepository.Add(stadium);
        
        auditLogService?.Log(
            AuditActions.CreateStadium,
            $"Stadium '{stadium.Name}' created in {stadium.City}.",
            nameof(Stadium));
        unitOfWork.Commit();
    }
    
    public Stadium GetById(int id)
    {
        Stadium? stadium = stadiumRepository.GetById(id);
        if (stadium == null)
            throw new ArgumentException("Stadium not found");
        return stadium;
    }
    
    public Stadium GetByName(string name)
    {
        Stadium? stadium = stadiumRepository.GetByName(name);
        if (stadium == null)
            throw new ArgumentException("Stadium not found");
        return stadium;
    }
    
    public void RemoveById(int id)
    {
        Stadium stadium = GetById(id);
        stadiumRepository.Delete(stadium);
        auditLogService?.Log(
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
        auditLogService?.Log(
            AuditActions.UpdateStadium,
            $"Stadium '{stadium.Name}' updated.",
            nameof(Stadium));
        unitOfWork.Commit();
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
