using DataAccess.Interfaces;
using ProjectCopa;
using Services.Exceptions;
using Services.Interfaces;

namespace Services;

public class JourneyServices(IJourneyRepository journeyRepository, IUnitOfWork unitOfWork, IAuditLogService? auditLogService = null) : IJourneyService
{
    private const string DuplicateJourneyNumberMessage = "Journey number must be unique in the system.";
    private const string JourneyNotFoundMessage = "Journey was not found.";

    public IReadOnlyCollection<Journey> GetAllJourneys()
    {
        return journeyRepository.GetAll();
    }

    public void AddJourney(Journey journey)
    {
        ArgumentNullException.ThrowIfNull(journey);

        if (journeyRepository.ExistsByNumber(journey.Number))
        {
            throw new JourneyServicesException(DuplicateJourneyNumberMessage);
        }

        journeyRepository.Add(journey);
        auditLogService?.Log(
            AuditActions.CreateJourney,
            $"Journey '{journey.Number}' created.",
            nameof(Journey),
            journey.Number);
        unitOfWork.Commit();
    }

    public Journey GetByNumber(int number)
    {
        return journeyRepository.GetByNumber(number)
               ?? throw new JourneyServicesException(JourneyNotFoundMessage);
    }

    public void RemoveByNumber(int number)
    {
        Journey journey = GetByNumber(number);
        journeyRepository.Delete(journey);
        auditLogService?.Log(
            AuditActions.DeleteJourney,
            $"Journey '{journey.Number}' removed.",
            nameof(Journey),
            journey.Number);
        unitOfWork.Commit();
    }
}
