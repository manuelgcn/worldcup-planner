namespace ProjectCopa;

public class Group
{
    private const string GroupNameIsRequiredMessage = "El nombre del grupo es obligatorio";
    private const string GroupFullMessage = "El grupo no puede tener mas de 4 equipos";
    private const string TeamAlreadyInGroupMessage = "El equipo ya existe en el grupo";

    public string Name { get; private set; }

    private readonly List<Team> teams = new();
    public IReadOnlyCollection<Team> Teams => teams.AsReadOnly();

    public Group(string name)
    {
        ValidateName(name);
        Name = name;
    }
    
    
    public void AddTeam(Team team)
    {
        if (teams.Count >= 4)
            throw new ArgumentException(GroupFullMessage);

        if (teams.Contains(team))
            throw new ArgumentException(TeamAlreadyInGroupMessage);

        teams.Add(team);
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(GroupNameIsRequiredMessage);
        }
    }


}
