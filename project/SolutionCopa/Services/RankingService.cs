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

        // Se guarda el delta aplicado para poder revertirlo exactamente si luego se edita el
        // resultado (H7); el Elo no es invertible sin conocer el cambio que produjo.
        match.SetAppliedRankingDeltas(newHomeRank - homeRank, newAwayRank - awayRank);

        // Los equipos llegan detached (lecturas con AsNoTracking): sin este Update
        // explicito el nuevo ranking solo vive en memoria y nunca llega a la base.
        // Limitacion conocida: cada repositorio usa un contexto efimero con autosave,
        // por lo que el ranking de los equipos y el resultado del partido se persisten
        // en SaveChanges separados (no es una operacion atomica).
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

    public void RevertMatchResult(Match match)
    {
        if (!match.RankingApplied || match.HomeTeam == null || match.AwayTeam == null)
        {
            return;
        }

        int previousHomeRank = match.HomeTeam.FifaRanking;
        int previousAwayRank = match.AwayTeam.FifaRanking;
        int restoredHomeRank = previousHomeRank - match.AppliedHomeRankingDelta;
        int restoredAwayRank = previousAwayRank - match.AppliedAwayRankingDelta;

        match.HomeTeam.UpdateFifaRanking(restoredHomeRank);
        match.AwayTeam.UpdateFifaRanking(restoredAwayRank);

        _teamRepository.Update(match.HomeTeam);
        _teamRepository.Update(match.AwayTeam);

        _auditLogService.Log(
            AuditActions.RankingUpdated,
            $"Ranking reverted for {match.HomeTeam.Name}: {previousHomeRank} -> {restoredHomeRank}.",
            nameof(Team),
            match.HomeTeam.Id);

        _auditLogService.Log(
            AuditActions.RankingUpdated,
            $"Ranking reverted for {match.AwayTeam.Name}: {previousAwayRank} -> {restoredAwayRank}.",
            nameof(Team),
            match.AwayTeam.Id);

        match.ClearRankingApplication();
    }
}