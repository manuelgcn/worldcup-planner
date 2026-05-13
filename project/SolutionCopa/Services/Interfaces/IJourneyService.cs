using ProjectCopa;

namespace Services.Interfaces;

public interface IJourneyService
{
    IReadOnlyCollection<Journey> GetAllJourneys();
    void AddJourney(Journey journey);
    Journey GetByNumber(int number);
    void RemoveByNumber(int number);
}
