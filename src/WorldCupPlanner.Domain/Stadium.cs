namespace WorldCupPlanner.Domain;

public class Stadium
{
    private const string NameRequiredMessage = "El nombre es obligatorio";
    private const string NameTooLongMessage = "El nombre es demasiado largo";
    private const string CityRequiredMessage = "La ciudad es obligatoria";
    private const string CityTooLongMessage = "La ciudad es demasiado larga";
    private const string CapacityTooLowMessage = "La capacidad debe ser de al menos 20000";

    public int Id { get; set; }
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
            throw new ArgumentException(NameRequiredMessage);

        if (name.Length > 80)
            throw new ArgumentException(NameTooLongMessage);
    }

    private void ValidateCity(string city)
    {
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException(CityRequiredMessage);

        if (city.Length > 60)
            throw new ArgumentException(CityTooLongMessage);
    }

    private void ValidateCapacity(int capacity)
    {
        if (capacity < 20000)
            throw new ArgumentException(CapacityTooLowMessage);
    }

    public void Update(string city, string? description, int capacity)
    {
        ValidateCity(city);
        ValidateCapacity(capacity);

        City = city;
        Description = description;
        Capacity = capacity;
    }
}