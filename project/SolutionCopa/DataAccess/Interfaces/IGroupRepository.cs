using ProjectCopa.Interfaces;

namespace DataAccess.Interfaces;

public interface IGroupRepository
{
    IGroup? GetById(int groupId);
}
