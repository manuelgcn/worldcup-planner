using DataAccess.Interfaces;
using ProjectCopa;
using Services.Exceptions;
using Services.Interfaces;

namespace Services;

public class StadiumServices(IStadiumRepository stadiumRepository, IUnitOfWork unitOfWork, IAuditLogService? auditLogService = null) : IStadiumService
{
    private const string DuplicateStadiumNameMessage = "Stadium name must be unique in the system.";
    private const string StadiumNotFoundMessage = "Stadium was not found.";

    public IReadOnlyCollection<Stadium> GetAllStadiums()
    {
        return stadiumRepository.GetAll();
    }

    public void AddStadium(Stadium stadium)
    {
        ArgumentNullException.ThrowIfNull(stadium);

        if (stadiumRepository.ExistsByName(stadium.Name))
        {
            throw new StadiumServicesException(DuplicateStadiumNameMessage);
        }

        stadiumRepository.Add(stadium);
        
        auditLogService?.Log(
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
    
}
