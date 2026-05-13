using DataAccess.Interfaces;
using ProjectCopa;
using ProjectCopa.Models;
using Services.Exceptions;
using Services.Interfaces;

namespace Services;

public sealed class TournamentSimulationService(
    IGroupRepository groupRepository,
    IMatchRepository matchRepository,
    IUnitOfWork unitOfWork,
    IMatchSimulationEngine matchSimulationEngine,
    IAuditLogService auditLogService,
    IKnockoutFixtureGenerator knockoutFixtureGenerator) : ITournamentSimulationService
{
    private const string FixtureNotGeneratedMessage = "El fixture aún no ha sido generado";
    private const string AuditLogMessage = "La fase de grupos fue simulada y la fase 2 fue generada.";

    public void SimulateGroupStageAndGenerateKnockout(int seed)
    {
        List<Group> groups = GetOrderedGroups();
        List<Match> groupStageMatches = GetGroupStageMatches(groups);

        ValidateFixtureGenerated(groupStageMatches);

        SimulateMatches(groupStageMatches, seed);

        QualifiedTeams qualifiedTeams = ClassifyTeams(groups, groupStageMatches, seed);

        GenerateAndPersistKnockoutPhase(qualifiedTeams);

        LogAndCommit();
    }

    private List<Group> GetOrderedGroups()
    {
        return groupRepository.GetAll()
            .OrderBy(group => group.Name)
            .ToList();
    }

    private List<Match> GetGroupStageMatches(List<Group> groups)
    {
        List<Match> groupStageMatches = new List<Match>();
        
        foreach (Group group in groups)
        {
            List<Match> matchesForGroup = matchRepository.GetByGroup(group.Name)
                .OrderBy(match => match.MatchDate)
                .ThenBy(match => match.MatchId)
                .ToList();

            groupStageMatches.AddRange(matchesForGroup);
        }

        return groupStageMatches;
    }

    private void ValidateFixtureGenerated(List<Match> matches)
    {
        if (matches.Count == 0)
        {
            throw new MatchServicesException(FixtureNotGeneratedMessage);
        }
    }

    private void SimulateMatches(List<Match> matches, int seed)
    {
        foreach (Match match in matches)
        {
            matchSimulationEngine.Simulate(match, seed);
            matchRepository.Update(match);
        }
    }

    private QualifiedTeams ClassifyTeams(List<Group> groups, List<Match> matches, int seed)
    {
        GroupQualifier qualifier = new GroupQualifier();
        return qualifier.ClassifyTeams(groups, matches, seed);
    }

    private void GenerateAndPersistKnockoutPhase(QualifiedTeams qualifiedTeams)
    {
        List<Match> knockoutMatches = knockoutFixtureGenerator.GeneratePhase2(qualifiedTeams);

        foreach (Match match in knockoutMatches)
        {
            matchRepository.Add(match);
        }
    }

    private void LogAndCommit()
    {
        auditLogService.Log(
            AuditActions.SimulateGroupStageAndGenerateKnockout,
            AuditLogMessage,
            affectedEntityType: nameof(TournamentSimulationService));

        unitOfWork.Commit();
    }
}