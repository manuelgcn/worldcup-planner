using DataAccess.Interfaces;
using ProjectCopa;

namespace ServiceTest.TestDoubles.Mocks;

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
        return _stadiums.Values.FirstOrDefault(stadium => stadium.Name == name);
    }

    public bool ExistsByName(string name)
    {
        return _stadiums.Values.Any(stadium => stadium.Name == name);
    }

    public IReadOnlyCollection<Stadium> GetAll()
    {
        return _stadiums.Values.ToList();
    }
}
