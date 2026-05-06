using ProjectCopa;

namespace DataAccess.Interfaces;

public interface IGroupRepository
{
    Group? GetById(int groupId);
}
