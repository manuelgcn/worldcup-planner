namespace ProjectCopa;

public class Group
{
    public string Name { get; private set; }

    private List<Team> teams = new List<Team>();
    public IReadOnlyCollection<Team> Teams => teams.AsReadOnly();

    public Group(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Group name is required");

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


}