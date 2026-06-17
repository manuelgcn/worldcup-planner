using ProjectCopa;

namespace Services;

// Calendario reutilizable para una ronda eliminatoria. Asigna a cada partido (por su indice
// 0-based dentro de la ronda) un estadio y una fecha/hora unicos, replicando las reglas del
// fixture de grupos (CLAUDE.md §7): rotacion circular de estadios por orden de nombre
// normalizado, arranque a las 14:00 con 4h de separacion y tope MaxMatchesPerDay por dia.
// Cada asignacion se reserva ademas en StadiumScheduleTracker como barrera explicita para
// garantizar que no se repita la terna (estadio, dia, hora) dentro de la ronda.
public sealed class KnockoutRoundScheduler
{
    private const int FirstMatchHour = 14;
    private const int HoursBetweenMatches = 4;
    // Tope fisico de partidos/dia: desde las 14:00 con 4h de separacion solo entran 14:00, 18:00
    // y 22:00 antes de medianoche (mismo limite que el fixture de grupos).
    private const int MaxMatchesPerDayUpperBound = 3;
    private const string StadiumsRequiredMessage =
        "Se requiere al menos un estadio para programar la ronda eliminatoria.";

    private readonly List<Stadium> _orderedStadiums;
    private readonly TimeSpan[] _allowedTimes;
    private readonly int _maxMatchesPerDay;
    private readonly DateTime _roundStartDay;
    private readonly StadiumScheduleTracker _tracker = new StadiumScheduleTracker();
    private readonly Dictionary<int, (Stadium Stadium, DateTime Date)> _assignments = new();

    public KnockoutRoundScheduler(IReadOnlyCollection<Stadium> stadiums, int maxMatchesPerDay, DateTime roundStartDate)
    {
        ArgumentNullException.ThrowIfNull(stadiums);
        if (stadiums.Count == 0)
        {
            throw new ArgumentException(StadiumsRequiredMessage, nameof(stadiums));
        }

        _orderedStadiums = stadiums.OrderBy(stadium => NormalizeName(stadium.Name)).ToList();
        _maxMatchesPerDay = Math.Clamp(maxMatchesPerDay, 1, MaxMatchesPerDayUpperBound);
        _allowedTimes = BuildAllowedTimes(_maxMatchesPerDay);
        _roundStartDay = roundStartDate.Date;
    }

    // Providers indexados, compatibles con la firma que ya consumen EliminationBuilder y el flujo
    // de KnockoutFixtureGenerator (stadiumProvider(i) / dateProvider(i)).
    public Func<int, Stadium> StadiumProvider => matchIndex => Resolve(matchIndex).Stadium;

    public Func<int, DateTime> DateProvider => matchIndex => Resolve(matchIndex).Date;

    // Calcula (la primera vez) y memoiza la asignacion del partido, reservando el cupo en el
    // tracker. La memoizacion permite que StadiumProvider y DateProvider, invocados por separado
    // para el mismo indice, devuelvan la misma terna y reserven una sola vez.
    private (Stadium Stadium, DateTime Date) Resolve(int matchIndex)
    {
        if (_assignments.TryGetValue(matchIndex, out (Stadium Stadium, DateTime Date) cached))
        {
            return cached;
        }

        int dayOffset = matchIndex / _maxMatchesPerDay;
        int slotInDay = matchIndex % _maxMatchesPerDay;
        DateTime date = _roundStartDay.AddDays(dayOffset) + _allowedTimes[slotInDay];
        Stadium stadium = _orderedStadiums[matchIndex % _orderedStadiums.Count];

        // Barrera explicita pedida por el fix: garantiza que la ronda no repita (estadio, dia, hora).
        // Con el esquema indexado cada indice cae en un (dia, franja) unico, asi que Reserve no
        // deberia lanzar; queda como red de seguridad ante cambios futuros del calculo.
        _tracker.Reserve(stadium, date);

        _assignments[matchIndex] = (stadium, date);
        return (stadium, date);
    }

    // Horarios permitidos: arrancan a las 14:00 y se separan 4h; la cantidad la fija
    // MaxMatchesPerDay, acotada a los que entran en un dia (max 3: 14:00, 18:00, 22:00).
    private static TimeSpan[] BuildAllowedTimes(int maxMatchesPerDay)
    {
        return Enumerable.Range(0, maxMatchesPerDay)
            .Select(slot => new TimeSpan(FirstMatchHour + (slot * HoursBetweenMatches), 0, 0))
            .ToArray();
    }

    // Normalizacion para el orden de rotacion: trim + minusculas invariantes. Mantiene un orden
    // estable e independiente de mayusculas/espacios accidentales en el nombre del estadio.
    private static string NormalizeName(string name)
    {
        return name?.Trim().ToLowerInvariant() ?? string.Empty;
    }
}
