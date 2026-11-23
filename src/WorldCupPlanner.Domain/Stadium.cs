namespace WorldCupPlanner.Domain;

public class Stadium
{
    private const string NameRequiredMessage = "El nombre es obligatorio";
    private const string NameTooLongMessage = "El nombre es demasiado largo";
    private const string NameHasControlCharactersMessage = "El nombre no puede tener saltos de línea ni otros caracteres de control";
    private const string NameWithoutLettersOrDigitsMessage = "El nombre debe tener al menos una letra o un número";
    private const string CityRequiredMessage = "La ciudad es obligatoria";
    private const string CityTooLongMessage = "La ciudad es demasiado larga";
    private const string CityHasControlCharactersMessage = "La ciudad no puede tener saltos de línea ni otros caracteres de control";
    private const string DescriptionTooLongMessage = "La descripción no puede superar los 400 caracteres";
    private const int MaxDescriptionLength = 400;
    private const int MaxNameLength = 80;
    private const int MaxCityLength = 60;
    private const int MinCapacity = 20000;
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
        ValidateDescription(description);
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

        if (name.Length > MaxNameLength)
            throw new ArgumentException(NameTooLongMessage);

        if (name.Any(char.IsControl))
            throw new ArgumentException(NameHasControlCharactersMessage);

        // Un nombre de solo signos se normaliza a "" y seria el mismo estadio que cualquier otro de solo signos.
        if (TextNormalizer.Normalize(name).Length == 0)
            throw new ArgumentException(NameWithoutLettersOrDigitsMessage);
    }

    private void ValidateCity(string city)
    {
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException(CityRequiredMessage);

        if (city.Length > MaxCityLength)
            throw new ArgumentException(CityTooLongMessage);

        if (city.Any(char.IsControl))
            throw new ArgumentException(CityHasControlCharactersMessage);
    }

    private void ValidateDescription(string? description)
    {
        if (description is not null && description.Length > MaxDescriptionLength)
            throw new ArgumentException(DescriptionTooLongMessage);
    }

    private void ValidateCapacity(int capacity)
    {
        if (capacity < MinCapacity)
            throw new ArgumentException(CapacityTooLowMessage);
    }

    public void Update(string city, string? description, int capacity)
    {
        ValidateCity(city);
        ValidateDescription(description);
        ValidateCapacity(capacity);

        City = city;
        Description = description;
        Capacity = capacity;
    }
}