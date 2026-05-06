using ProjectCopa;

namespace DataAccess.Interfaces;

public interface ITeamRepository
{
    Team? GetById(int teamId);
}
