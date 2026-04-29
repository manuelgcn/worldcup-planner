using Services.DTOs;

namespace Services.Interfaces;

public interface IMatchService
{
    Task DeleteMatchAsync(int matchId, CancellationToken ct = default);
    Task<MatchDto> GetByIdAsync(int matchId, CancellationToken ct = default);
    Task<IReadOnlyCollection<MatchDto>> GetByGroupAsync(string groupName, CancellationToken ct = default);
    Task<IReadOnlyCollection<MatchDto>> GetByJourneyAsync(int journeyNumber, CancellationToken ct = default);
    Task<IReadOnlyCollection<MatchDto>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken ct = default);
    Task SetGoalsAsync(int matchId, int homeGoals, int awayGoals, CancellationToken ct = default);
    Task AddGoalAsync(int matchId, bool isHomeTeam, CancellationToken ct = default);
    Task RemoveGoalAsync(int matchId, bool isHomeTeam, CancellationToken ct = default);
    Task PublishResultAsync(int matchId, CancellationToken ct = default);
}
