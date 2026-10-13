using WorldCupPlanner.Domain;
using WorldCupPlanner.Services.DTOs;

namespace WorldCupPlanner.Services.Interfaces;

public interface IMatchService
{
    MatchDto CreateMatch(CreateMatchRequestDto request);
    MatchDto UpdateMatch(int matchId, UpdateMatchRequestDto request);
    void DeleteMatch(int matchId);
    MatchDto GetById(int matchId);
    // Devuelve la entidad con su grafo (equipos/estadio) para alimentar el editor de
    // resultados en la UI sin que la pagina inyecte repositorios.
    Match GetEntity(int matchId);
    List<MatchDto> GetByGroup(string groupName);
    List<MatchDto> GetByJourney(int journeyNumber);
    List<MatchDto> GetByDateRange(DateTime from, DateTime to);
    void SetGoals(int matchId, int homeGoals, int awayGoals);
    MatchDto RegisterResult(int matchId, int homeGoals, int awayGoals, string? performedByEmail);
    void AddGoal(int matchId, bool isHomeTeam);
    void RemoveGoal(int matchId, bool isHomeTeam);
    void PublishResult(int matchId);
    void SimulateMatch(int matchId);
    void AddYellowCard(int matchId, int teamId, int minute, string? performedByEmail);
    void AddRedCard(int matchId, int teamId, int minute, string? performedByEmail);
    void RemoveIncident(int matchId, int incidentId, string? performedByEmail);
    List<MatchIncidentDto> GetIncidents(int matchId);
}
