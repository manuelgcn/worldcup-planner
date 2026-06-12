using DataAccess.Interfaces;
using ProjectCopa;
using Services.Interfaces;

namespace Services;

public class RankingService : IRankingService
{
    private readonly IAuditLogService _auditLogService;
    private readonly ITeamRepository _teamRepository;

    public RankingService(IAuditLogService auditLogService, ITeamRepository teamRepository)
    {
        _auditLogService = auditLogService;
        _teamRepository = teamRepository;
    }

    public void ApplyMatchResult(Match match, MatchPhase phase)
    {
        if (match.HomeTeam == null || match.AwayTeam == null)
            return;

        int homeRank = match.HomeTeam.FifaRanking;
        int awayRank = match.AwayTeam.FifaRanking;

        EloResult homeResult = EloResult.Draw;
        EloResult awayResult = EloResult.Draw;

        if (match.HasHomeTeamWon())
        {
            homeResult = EloResult.Win;
            awayResult = EloResult.Loss;
        }
        else if (match.HasAwayTeamWon())
        {
            homeResult = EloResult.Loss;
            awayResult = EloResult.Win;
        }

        int newHomeRank = EloRankingCalculator.Compute(homeRank, awayRank, homeResult, phase);
        int newAwayRank = EloRankingCalculator.Compute(awayRank, homeRank, awayResult, phase);

        match.HomeTeam.UpdateFifaRanking(newHomeRank);
        match.AwayTeam.UpdateFifaRanking(newAwayRank);

        // Los equipos llegan detached (lecturas con AsNoTracking): sin este Update
        // explicito el nuevo ranking solo vive en memoria y nunca llega a la base.
        _teamRepository.Update(match.HomeTeam);
        _teamRepository.Update(match.AwayTeam);

        _auditLogService.Log(
            AuditActions.RankingUpdated,
            $"Ranking updated for {match.HomeTeam.Name}: {homeRank} -> {newHomeRank}.",
            nameof(Team),
            match.HomeTeam.Id);

        _auditLogService.Log(
            AuditActions.RankingUpdated,
            $"Ranking updated for {match.AwayTeam.Name}: {awayRank} -> {newAwayRank}.",
            nameof(Team),
            match.AwayTeam.Id);
    }
}