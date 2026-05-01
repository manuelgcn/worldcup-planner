using DataAccess.Interfaces;
using ProjectCopa.Interfaces;

namespace ServicesTest.TestDoubles;

public sealed class FakeJourneyRepository : IJourneyRepository
{
    private readonly Dictionary<int, IJourney> _journeys = new();

    public void Set(int id, IJourney journey)
    {
        _journeys[id] = journey;
    }

    public IJourney? GetById(int journeyId)
    {
        IJourney? journey;
        var found = _journeys.TryGetValue(journeyId, out journey);
        if (!found)
        {
            return null;
        }

        return journey;
    }
}
