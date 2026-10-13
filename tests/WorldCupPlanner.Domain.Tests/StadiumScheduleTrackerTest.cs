using WorldCupPlanner.Domain;
using WorldCupPlanner.Domain.Exceptions;

namespace WorldCupPlanner.Domain.Tests;

[TestClass]
public class StadiumScheduleTrackerTests
{
    private static Stadium CreateStadium()
    {
        return new Stadium("Estadio Centenario", "Montevideo", "Uruguay", 60000);
    }

    [TestMethod]
    public void CanSchedule_ReturnsTrue_WhenStadiumAndDateAreFree()
    {
        // Arrange
        StadiumScheduleTracker tracker = new StadiumScheduleTracker();
        Stadium stadium = CreateStadium();
        DateTime matchTime = new DateTime(2026, 6, 1, 14, 0, 0);

        // Act
        bool result = tracker.CanSchedule(stadium, matchTime);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void CanSchedule_NullStadium_ThrowsArgumentNullException()
    {
        // Arrange
        StadiumScheduleTracker tracker = new StadiumScheduleTracker();
        DateTime matchTime = new DateTime(2026, 6, 1, 14, 0, 0);

        // Act
        tracker.CanSchedule(null!, matchTime);
    }

    [TestMethod]
    public void Reserve_SuccessfullyBooksSlot_AndCanScheduleReturnsFalse()
    {
        // Arrange
        StadiumScheduleTracker tracker = new StadiumScheduleTracker();
        Stadium stadium = CreateStadium();
        DateTime matchTime = new DateTime(2026, 6, 1, 14, 0, 0);

        // Act
        tracker.Reserve(stadium, matchTime);
        bool isStillFree = tracker.CanSchedule(stadium, matchTime);

        // Assert
        Assert.IsFalse(isStillFree, "El horario debería estar ocupado después de reservarlo.");
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void Reserve_NullStadium_ThrowsArgumentNullException()
    {
        // Arrange
        StadiumScheduleTracker tracker = new StadiumScheduleTracker();
        DateTime matchTime = new DateTime(2026, 6, 1, 14, 0, 0);

        // Act
        tracker.Reserve(null!, matchTime);
    }

    [TestMethod]
    public void Reserve_ThrowsMatchException_WhenTimeSlotIsAlreadyBooked()
    {
        // Arrange
        StadiumScheduleTracker tracker = new StadiumScheduleTracker();
        Stadium stadium = CreateStadium();
        DateTime matchTime = new DateTime(2026, 6, 1, 14, 0, 0);
            
        tracker.Reserve(stadium, matchTime); // Primera reserva exitosa

        // Act & Assert
        MatchException exception = Assert.ThrowsException<MatchException>(() => 
        {
            tracker.Reserve(stadium, matchTime); // Intento de doble reserva en la misma hora
        });

        Assert.AreEqual("El estadio ya tiene un partido a esa hora.", exception.Message);
    }

    [TestMethod]
    public void Reserve_ThrowsMatchException_WhenDailyMaxIsExceeded()
    {
        // Arrange
        StadiumScheduleTracker tracker = new StadiumScheduleTracker();
        Stadium stadium = CreateStadium();
            
        DateTime slot1 = new DateTime(2026, 6, 1, 14, 0, 0);
        DateTime slot2 = new DateTime(2026, 6, 1, 18, 0, 0);
        DateTime slot3 = new DateTime(2026, 6, 1, 22, 0, 0);
        DateTime slot4 = new DateTime(2026, 6, 1, 10, 0, 0); // El 4to partido que debería fallar

        tracker.Reserve(stadium, slot1);
        tracker.Reserve(stadium, slot2);
        tracker.Reserve(stadium, slot3);

        // Act & Assert
        MatchException exception = Assert.ThrowsException<MatchException>(() => 
        {
            tracker.Reserve(stadium, slot4); // Intento de reservar un 4to partido en el mismo día
        });

        Assert.AreEqual("El estadio ya alcanzo el maximo de partidos del dia.", exception.Message);
    }

    [TestMethod]
    public void CanSchedule_ReturnsFalse_WhenDailyMaxIsExceeded()
    {
        // Arrange
        StadiumScheduleTracker tracker = new StadiumScheduleTracker();
        Stadium stadium = CreateStadium();
            
        DateTime slot1 = new DateTime(2026, 6, 1, 14, 0, 0);
        DateTime slot2 = new DateTime(2026, 6, 1, 18, 0, 0);
        DateTime slot3 = new DateTime(2026, 6, 1, 22, 0, 0);
        DateTime slot4 = new DateTime(2026, 6, 1, 10, 0, 0); 

        tracker.Reserve(stadium, slot1);
        tracker.Reserve(stadium, slot2);
        tracker.Reserve(stadium, slot3);

        // Act
        bool result = tracker.CanSchedule(stadium, slot4);

        // Assert
        Assert.IsFalse(result, "CanSchedule debería devolver falso si el estadio ya tiene 3 partidos en ese día.");
    }
}