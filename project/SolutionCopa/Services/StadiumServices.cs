using ProjectCopa;

namespace Services;

public class StadiumServices
{
    
    private List<Stadium> stadiums = new();
    
    public IReadOnlyCollection<Stadium> GetAllStadiums()
    {
        return stadiums.AsReadOnly();
    }

    public void AddStadium(Stadium stadium)
    {
        if (stadiums.Any(s => s.Name == stadium.Name))
            throw new ArgumentException("Este Nombre de estadio ya existe");

        stadiums.Add(stadium);
    }
    
    
    public Stadium GetByName(string name)
    {
        Stadium stadium = stadiums.FirstOrDefault(s => s.Name == name);

        if (stadium == null)
            throw new ArgumentException("Stadium not found");

        return stadium;
    }


}