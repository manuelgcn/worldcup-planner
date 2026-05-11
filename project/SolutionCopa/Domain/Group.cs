namespace ProjectCopa;

public class Group
{
    private const string GroupIdMustBePositiveMessage = "Group id must be greater than zero";
    private const string GroupNameIsRequiredMessage = "Group name is required";

    public int Id { get; private set; }
    public string Name { get; private set; }

    private readonly List<Team> teams = new();
    public IReadOnlyCollection<Team> Teams => teams.AsReadOnly();

    public Group(int id, string name)
    {
        ValidateId(id);
        ValidateName(name);
        Id = id;
        Name = name;
    }
    
    
    public void AddTeam(Team team)
    {
        if (teams.Count >= 4)
            throw new ArgumentException("Group cannot have more than 4 teams");

        if (teams.Contains(team))
            throw new ArgumentException("Team already exists in group");

        teams.Add(team);
    }

    private static void ValidateId(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException(GroupIdMustBePositiveMessage);
        }
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(GroupNameIsRequiredMessage);
        }
    }


}
