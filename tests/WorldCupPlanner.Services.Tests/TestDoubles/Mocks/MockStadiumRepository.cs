using WorldCupPlanner.DataAccess.Interfaces;
using WorldCupPlanner.Domain;

namespace WorldCupPlanner.Services.Tests.TestDoubles.Mocks;

public sealed class MockStadiumRepository : IStadiumRepository
{
    private readonly Dictionary<int, Stadium> _stadiums = new();

    public int AddCalls { get; private set; }
    public int UpdateCalls { get; private set; }
    public int DeleteCalls { get; private set; }

    public void Add(Stadium stadium)
    {
        _stadiums[stadium.Id] = stadium;
        AddCalls++;
    }

    public void Update(Stadium stadium)
    {
        _stadiums[stadium.Id] = stadium;
        UpdateCalls++;
    }

    public void Delete(Stadium stadium)
    {
        _stadiums.Remove(stadium.Id);
        DeleteCalls++;
    }

    public Stadium? GetById(int stadiumId)
    {
        return _stadiums.GetValueOrDefault(stadiumId);
    }

    public Stadium? GetByName(string name)
    {
        string target = TextNormalizer.Normalize(name);
        return _stadiums.Values.FirstOrDefault(stadium => TextNormalizer.Normalize(stadium.Name) == target);
    }

    public bool ExistsByName(string name)
    {
        string target = TextNormalizer.Normalize(name);
        return _stadiums.Values.Any(stadium => TextNormalizer.Normalize(stadium.Name) == target);
    }

    public IReadOnlyCollection<Stadium> GetAll()
    {
        return _stadiums.Values.ToList();
    }
}
