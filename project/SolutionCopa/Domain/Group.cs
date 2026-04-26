namespace ProjectCopa;

public class Group
{
    public string Name { get; private set; }

    private List<Team> teams = new();
    public IReadOnlyCollection<Team> Teams => teams.AsReadOnly();

    public Group(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Group name is required");

        Name = name;
    }

}