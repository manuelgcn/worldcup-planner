namespace ProjectCopa;

public class Team
{
    private const int MaxNameLength = 60;
    private const int MinFifaRanking = 300;
    private const int MaxFifaRanking = 2500;
    private const string InvalidNameMessage = "Team name cannot be empty.";
    private const string FifaRankingOutOfRangeMessage = "FIFA ranking must be between {0} and {1}.";

    public string Name { get; }
    public int FifaRanking { get; }
    public bool IsFictitious { get; }
    public Confederation Confederation { get; }

    public Team(string name, int fifaRanking, bool isFictitious, Confederation confederation)
    {
        ValidateName(name);
        ValidateFifaRanking(fifaRanking);
        Name = name;
        FifaRanking = fifaRanking;
        IsFictitious = isFictitious;
        Confederation = confederation;
    }

    private static void ValidateName(string name)
    {
        if (IsNameInvalid(name))
        {
            throw new ArgumentException(InvalidNameMessage, nameof(name));
        }
    }

    private static void ValidateFifaRanking(int fifaRanking)
    {
        if (IsFifaRankingOutOfRange(fifaRanking))
        {
            throw new ArgumentException(string.Format(FifaRankingOutOfRangeMessage, MinFifaRanking, MaxFifaRanking), nameof(fifaRanking));
        }
    }

    private static bool IsNameInvalid(string name)
    {
        return string.IsNullOrEmpty(name) || name.Length > MaxNameLength;
    }

    private static bool IsFifaRankingOutOfRange(int fifaRanking)
    {
        return fifaRanking < MinFifaRanking || fifaRanking > MaxFifaRanking;
    }
}


