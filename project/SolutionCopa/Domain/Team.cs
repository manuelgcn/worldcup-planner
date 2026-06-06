
namespace ProjectCopa;

public class Team
{
    private const int MaxNameLength = 60;
    private const int MinFifaRanking = 300;
    private const int MaxFifaRanking = 2500;
    private const string InvalidNameMessage = "Team name cannot be empty.";
    private const string FifaRankingOutOfRangeMessage = "FIFA ranking must be between {0} and {1}.";
    private const string InvalidFlagMessage = "Flag must be a valid base64 string.";

    public int Id { get; }
    public string Name { get; }
    public int FifaRanking { get; }
    public bool IsFictitious { get; }
    public Confederation Confederation { get; }
    public string? FlagBase64 { get; }

    public Team(int id, string name, int fifaRanking, bool isFictitious, Confederation confederation, string? flagBase64 = null)
    {
        ValidateId(id);
        ValidateName(name);
        ValidateFifaRanking(fifaRanking);
        ValidateFlag(flagBase64);

        Id = id;
        Name = name;
        FifaRanking = fifaRanking;
        IsFictitious = isFictitious;
        Confederation = confederation;
        FlagBase64 = string.IsNullOrWhiteSpace(flagBase64) ? null : flagBase64;
    }

    private static void ValidateId(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException("Team id must be greater than zero.", nameof(id));
        }
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

    private static void ValidateFlag(string? flagBase64)
    {
        if (string.IsNullOrWhiteSpace(flagBase64))
        {
            return;
        }

        if (!IsValidBase64(flagBase64))
        {
            throw new ArgumentException(InvalidFlagMessage, nameof(flagBase64));
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

    private static bool IsValidBase64(string value)
    {
        Span<byte> buffer = stackalloc byte[value.Length];
        return Convert.TryFromBase64String(value, buffer, out _);
    }
}
