namespace ProjectCopa;

public class Team
{
    private const int MaxNameLength = 60;
    private const int MinFifaRanking = 300;
    private const int MaxFifaRanking = 2500;
    private const string InvalidNameMessage = "El nombre del equipo no puede estar vacio.";
    private const string FifaRankingOutOfRangeMessage = "El ranking FIFA debe estar entre {0} y {1}.";
    private const string InvalidFlagMessage = "La bandera debe ser una cadena base64 valida.";
    private const int MaxFlagBytes = 200 * 1024;
    private const string FlagTooLargeMessage = "La bandera no debe superar los 200 KB una vez decodificada.";
    private const string InvalidIdMessage = "El id del equipo debe ser mayor a cero.";

    public int Id { get; }
    public string Name { get; }
    public int FifaRanking { get; private set; }
    public bool IsFictitious { get; }
    public Confederation Confederation { get; }
    public string? FlagBase64 { get; private set; }

    public Team(
        int id,
        string name,
        int fifaRanking,
        bool isFictitious,
        Confederation confederation,
        string? flagBase64 = null)
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
        FlagBase64 = NormalizeFlag(flagBase64);
    }

    public void UpdateFlag(string? flagBase64)
    {
        ValidateFlag(flagBase64);
        FlagBase64 = NormalizeFlag(flagBase64);
    }

    public void UpdateFifaRanking(int newFifaRanking)
    {
        FifaRanking = Math.Clamp(newFifaRanking, MinFifaRanking, MaxFifaRanking);
    }

    private static void ValidateId(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException(InvalidIdMessage, nameof(id));
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
            throw new ArgumentException(
                string.Format(FifaRankingOutOfRangeMessage, MinFifaRanking, MaxFifaRanking),
                nameof(fifaRanking));
        }
    }

    private static void ValidateFlag(string? flagBase64)
    {
        if (string.IsNullOrWhiteSpace(flagBase64))
        {
            return;
        }

        byte[]? decoded = DecodeBase64OrNull(flagBase64);

        if (decoded is null)
        {
            throw new ArgumentException(InvalidFlagMessage, nameof(flagBase64));
        }

        if (decoded.Length > MaxFlagBytes)
        {
            throw new ArgumentException(FlagTooLargeMessage, nameof(flagBase64));
        }
    }

    private static string? NormalizeFlag(string? flagBase64)
    {
        return string.IsNullOrWhiteSpace(flagBase64) ? null : flagBase64;
    }

    private static bool IsNameInvalid(string name)
    {
        return string.IsNullOrEmpty(name) || name.Length > MaxNameLength;
    }

    private static bool IsFifaRankingOutOfRange(int fifaRanking)
    {
        return fifaRanking < MinFifaRanking || fifaRanking > MaxFifaRanking;
    }

    private static byte[]? DecodeBase64OrNull(string value)
    {
        // No es un error silenciado: null es el valor centinela de "base64 invalido".
        // Solo se atrapa FormatException (entrada mal formada); ValidateFlag lo traduce
        // luego a una ArgumentException con mensaje de negocio.
        try
        {
            return Convert.FromBase64String(value);
        }
        catch (FormatException)
        {
            return null;
        }
    }
}
