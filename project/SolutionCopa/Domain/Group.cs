namespace ProjectCopa;

public class Group
{
    private const string GroupNameIsRequiredMessage = "Group name is required";

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
            throw new ArgumentException("Group cannot have more than 4 teams");

        if (teams.Contains(team))
            throw new ArgumentException("Team already exists in group");

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
