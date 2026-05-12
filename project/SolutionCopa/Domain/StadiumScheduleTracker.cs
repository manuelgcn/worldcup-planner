using ProjectCopa.Exceptions;

namespace ProjectCopa;

public sealed class StadiumScheduleTracker
{
    private const int MaxMatchesPerStadiumPerDay = 3;
    private const string StadiumAlreadyOccupiedMessage = "Stadium already has a match at this time.";
    private const string StadiumMaxMatchesExceededMessage = "Stadium already has maximum matches for the day.";

    private readonly Dictionary<StadiumDateKey, DailySchedule> bookings = new();

    public bool CanSchedule(Stadium stadium, DateTime dateTime)
    {
        ArgumentNullException.ThrowIfNull(stadium);

        StadiumDateKey key = new(stadium, DateOnly.FromDateTime(dateTime));
        TimeSpan time = dateTime.TimeOfDay;

        if (!bookings.TryGetValue(key, out DailySchedule? schedule))
        {
            return true;
        }

        return schedule.CanBook(time);
    }

    public void Reserve(Stadium stadium, DateTime dateTime)
    {
        ArgumentNullException.ThrowIfNull(stadium);

        StadiumDateKey key = new(stadium, DateOnly.FromDateTime(dateTime));
        TimeSpan time = dateTime.TimeOfDay;

        if (!bookings.TryGetValue(key, out DailySchedule? schedule))
        {
            schedule = new DailySchedule();
            bookings[key] = schedule;
        }

        schedule.Book(time);
    }

    private readonly record struct StadiumDateKey(Stadium Stadium, DateOnly Date);

    private sealed class DailySchedule
    {
        private readonly HashSet<TimeSpan> _bookedTimes = new();

        public bool CanBook(TimeSpan time)
        {
            if (_bookedTimes.Contains(time))
            {
                return false;
            }

            if (_bookedTimes.Count >= MaxMatchesPerStadiumPerDay)
            {
                return false;
            }

            return true;
        }

        public void Book(TimeSpan time)
        {
            if (_bookedTimes.Contains(time))
            {
                throw new MatchException(StadiumAlreadyOccupiedMessage);
            }

            if (_bookedTimes.Count >= MaxMatchesPerStadiumPerDay)
            {
                throw new MatchException(StadiumMaxMatchesExceededMessage);
            }

            _bookedTimes.Add(time);
        }
    }
}