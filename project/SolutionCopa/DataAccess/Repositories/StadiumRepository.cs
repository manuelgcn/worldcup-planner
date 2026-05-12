using DataAccess.Interfaces;
using ProjectCopa;

namespace DataAccess.Repositories;

public class StadiumRepository : IStadiumRepository
{
    private readonly List<Stadium> _stadiums = new List<Stadium>();
    private int _nextId = 1;

    public void Add(Stadium stadium)
    {
        if (stadium.Id <= 0)
        {
            stadium.Id = _nextId;
        }

        _nextId = Math.Max(_nextId, stadium.Id + 1);
        _stadiums.Add(stadium);
    }

    public void Update(Stadium stadium)
    {
        int index = _stadiums.FindIndex(s => s.Id == stadium.Id);
        if (index >= 0)
        {
            _stadiums[index] = stadium;
        }
        else
        {
            if (stadium.Id <= 0)
            {
                stadium.Id = _nextId;
            }

            _nextId = Math.Max(_nextId, stadium.Id + 1);
            _stadiums.Add(stadium);
        }
    }

    public void Delete(Stadium stadium)
    {
        _stadiums.RemoveAll(s => s.Id == stadium.Id);
    }

    public Stadium? GetById(int stadiumId)
    {
        return _stadiums.FirstOrDefault(s => s.Id == stadiumId);
    }

    public Stadium? GetByName(string name)
    {
        return _stadiums.FirstOrDefault(s => s.Name == name);
    }

    public bool ExistsByName(string name)
    {
        return _stadiums.Any(s => s.Name == name);
    }

    public IReadOnlyCollection<Stadium> GetAll()
    {
        return _stadiums.AsReadOnly();
    }
}
