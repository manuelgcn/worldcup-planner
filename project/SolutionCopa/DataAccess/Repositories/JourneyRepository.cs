using DataAccess.Interfaces;
using ProjectCopa;

namespace DataAccess.Repositories;

public class JourneyRepository : IJourneyRepository
{
    private readonly List<Journey> _journeys = new();

    public void Add(Journey journey)
    {
        _journeys.Add(journey);
    }

    public void Update(Journey journey)
    {
        int index = _journeys.FindIndex(existingJourney => existingJourney.Number == journey.Number);

        if (index >= 0)
        {
            _journeys[index] = journey;
            return;
        }

        _journeys.Add(journey);
    }

    public void Delete(Journey journey)
    {
        _journeys.RemoveAll(existingJourney => existingJourney.Number == journey.Number);
    }

    public Journey? GetByNumber(int number)
    {
        return _journeys.FirstOrDefault(journey => journey.Number == number);
    }

    public bool ExistsByNumber(int number)
    {
        return _journeys.Any(journey => journey.Number == number);
    }

    public IReadOnlyCollection<Journey> GetAll()
    {
        return _journeys.AsReadOnly();
    }
}
