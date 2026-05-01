using DataAccess.Interfaces;
using ProjectCopa.Interfaces;

namespace ServicesTest.TestDoubles;

public sealed class FakeStadiumRepository : IStadiumRepository
{
    private readonly Dictionary<int, IStadium> _stadiums = new();

    public void Set(int id, IStadium stadium)
    {
        _stadiums[id] = stadium;
    }

    public IStadium? GetById(int stadiumId)
    {
        IStadium? stadium;
        var found = _stadiums.TryGetValue(stadiumId, out stadium);
        if (!found)
        {
            return null;
        }

        return stadium;
    }
}
