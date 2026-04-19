namespace ProjectCopa;

public class Stadium
{
    
    public string Name { get; private set; }
    public string City { get; private set; }
    public string? Description { get; private set; }
    public int Capacity { get; private set; }

    public Stadium(string name, string city, string? description, int capacity)
    {
        ValidateName(name);
        ValidateCity(city);
        ValidateCapacity(capacity);
        
        Name = name;
        City = city;
        Description = description;
        Capacity = capacity;
    }

    
    private void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required");

        if (name.Length > 80)
            throw new ArgumentException("Name too long");
    }

    private void ValidateCity(string city)
    {
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City is required");

        if (city.Length > 60)
            throw new ArgumentException("City too long");
    }

    private void ValidateCapacity(int capacity)
    {
        if (capacity < 20000)
            throw new ArgumentException("Capacity must be at least 20000");
    }

}