using WorldCupPlanner.DataAccess.Interfaces;
using WorldCupPlanner.Domain;

namespace WorldCupPlanner.Services.Tests.TestDoubles.Mocks;

public sealed class MockJourneyRepository : IJourneyRepository
{
    private readonly Dictionary<int, Journey> _journeys = new();

    public int AddCalls { get; private set; }
    public int UpdateCalls { get; private set; }
    public int DeleteCalls { get; private set; }

    public void Add(Journey journey)
    {
        _journeys[journey.Number] = journey;
        AddCalls++;
    }

    public void Update(Journey journey)
    {
        _journeys[journey.Number] = journey;
        UpdateCalls++;
    }

    public void Delete(Journey journey)
    {
        _journeys.Remove(journey.Number);
        DeleteCalls++;
    }

    public Journey? GetByNumber(int number)
    {
        return _journeys.GetValueOrDefault(number);
    }

    public bool ExistsByNumber(int number)
    {
        return _journeys.ContainsKey(number);
    }

    public IReadOnlyCollection<Journey> GetAll()
    {
        return _journeys.Values.ToList();
    }

    public void Clear()
    {
        _journeys.Clear();
    }
}
