namespace ProjectCopa;

public class Stadium
{
    
    public string Name { get; private set; }
    public string City { get; private set; }
    public string? Description { get; private set; }
    public int Capacity { get; private set; }

    public Stadium(string name, string city, string? description, int capacity)
    {
        Name = name;
        City = city;
        Description = description;
        Capacity = capacity;
    }

}