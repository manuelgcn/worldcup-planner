using ProjectCopa.Interfaces;

namespace DataAccess.Interfaces;

public interface ITeamRepository
{
    ITeam? GetById(int teamId);
}
