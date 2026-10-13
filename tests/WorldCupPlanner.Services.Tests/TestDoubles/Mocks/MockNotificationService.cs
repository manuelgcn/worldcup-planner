using WorldCupPlanner.Domain;
using WorldCupPlanner.Services.Interfaces;

namespace WorldCupPlanner.Services.Tests.TestDoubles.Mocks;

// Doble explicito de INotificationService para los tests de MatchService como controlador
// del caso de uso "registrar resultado". Registra las invocaciones recibidas para poder
// verificar que el servicio delega la notificacion (no la decide), siguiendo el patron de
// los demas mocks de ServiceTest/TestDoubles/Mocks/.
public sealed class MockNotificationService : INotificationService
{
    public List<string> JournalistMessages { get; } = new();
    public List<ResultLoadedCall> ResultLoadedCalls { get; } = new();
    public List<Match> MatchSimulatedCalls { get; } = new();
    public List<IReadOnlyCollection<Match>> SimulatedBatches { get; } = new();

    public void NotifyJournalists(string message)
    {
        JournalistMessages.Add(message);
    }

    public void NotifyResultLoaded(Match match, int previousHomeGoals, int previousAwayGoals)
    {
        ResultLoadedCalls.Add(new ResultLoadedCall(match, previousHomeGoals, previousAwayGoals));
    }

    public void NotifyMatchSimulated(Match match)
    {
        MatchSimulatedCalls.Add(match);
    }

    public void NotifyMatchesSimulated(IReadOnlyCollection<Match> matches)
    {
        SimulatedBatches.Add(matches);
        MatchSimulatedCalls.AddRange(matches);
    }

    public IReadOnlyCollection<Notification> GetUnreadForUser(int userId)
    {
        return Array.Empty<Notification>();
    }

    public int CountUnreadForUser(int userId)
    {
        return 0;
    }

    public void MarkAsRead(int notificationId, int userId)
    {
    }

    public void MarkAllAsRead(int userId)
    {
    }

    public sealed record ResultLoadedCall(Match Match, int PreviousHomeGoals, int PreviousAwayGoals);
}
