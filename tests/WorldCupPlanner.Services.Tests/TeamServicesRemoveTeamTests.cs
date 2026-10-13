using WorldCupPlanner.DataAccess.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WorldCupPlanner.Domain;
using WorldCupPlanner.Services;
using WorldCupPlanner.Services.DTOs;
using WorldCupPlanner.Services.Exceptions;
using WorldCupPlanner.Services.Interfaces;

namespace WorldCupPlanner.Services.Tests;

[TestClass]
public class TeamServicesRemoveTeamTests
{
    [TestMethod]
    public void CannotRemoveTeamThatBelongsToAGroup()
    {
        Team team = new Team(1, "Brasil", 1500, false, Confederation.CONMEBOL);

        FakeTeamRepository teamRepository = new FakeTeamRepository(team);
        FakeGroupRepository groupRepository = new FakeGroupRepository();
        FakeUnitOfWork unitOfWork = new FakeUnitOfWork();
        NullAuditLogService auditLogService = new NullAuditLogService();

        Group group = new Group("A");
        group.AddTeam(team);
        groupRepository.Add(group);

        TeamServices service = new TeamServices(
            teamRepository,
            groupRepository,
            unitOfWork,
            auditLogService);

        Assert.ThrowsException<TeamServicesException>(() => service.RemoveTeam("Brasil"));
        Assert.IsNotNull(teamRepository.GetByName("Brasil"));
    }
}

internal sealed class FakeTeamRepository : ITeamRepository
{
    private readonly List<Team> teams = new();

    public FakeTeamRepository(params Team[] initialTeams)
    {
        teams.AddRange(initialTeams);
    }

    public void Add(Team team)
    {
        teams.Add(team);
    }

    public void Update(Team team)
    {
        Team? existing = GetById(team.Id);
        if (existing is not null)
        {
            teams.Remove(existing);
        }

        teams.Add(team);
    }

    public void Delete(Team team)
    {
        teams.Remove(team);
    }

    public Team? GetById(int teamId)
    {
        return teams.FirstOrDefault(t => t.Id == teamId);
    }

    public Team? GetByName(string name)
    {
        return teams.FirstOrDefault(t => t.Name == name);
    }

    public bool ExistsByName(string name)
    {
        return teams.Any(t => t.Name == name);
    }

    public IReadOnlyCollection<Team> GetAll()
    {
        return teams.ToList();
    }

    public int CountByConfederation(Confederation confederation)
    {
        return teams.Count(t => t.Confederation == confederation);
    }
}

internal sealed class FakeGroupRepository : IGroupRepository
{
    private readonly List<Group> groups = new();

    public void Add(Group group)
    {
        groups.Add(group);
    }

    public void Update(Group group)
    {
        Group? existing = GetByName(group.Name);
        if (existing is not null)
        {
            groups.Remove(existing);
        }

        groups.Add(group);
    }

    public void Delete(Group group)
    {
        groups.Remove(group);
    }

    public Group? GetByName(string name)
    {
        return groups.FirstOrDefault(g => g.Name == name);
    }

    public bool ExistsByName(string name)
    {
        return groups.Any(g => g.Name == name);
    }

    public IReadOnlyCollection<Group> GetAll()
    {
        return groups.ToList();
    }

    public void Clear()
    {
        groups.Clear();
    }
}

internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public bool CommitCalled { get; private set; }

    public void Commit()
    {
        CommitCalled = true;
    }
}

internal sealed class NullAuditLogService : IAuditLogService
{
    public void Log(string action, string details, int tournamentSettingsId, string? affectedEntityType = null, int? affectedEntityId = null)
    {
    }

    public void Log(string action, string details, string? affectedEntityType = null, int? affectedEntityId = null)
    {
    }

    public void LogPerformedBy(string action, string details, string? performedByEmail, string? affectedEntityType = null, int? affectedEntityId = null)
    {
    }

    public List<AuditLogDto> GetAll()
    {
        return new List<AuditLogDto>();
    }

    public List<AuditLogDto> GetFiltered(AuditLogFilterDto filter)
    {
        return new List<AuditLogDto>();
    }

    public string ExportAsText()
    {
        return string.Empty;
    }
}
