using DataAccess.Interfaces;
using ProjectCopa;
using Services.Exceptions;
using Services.Interfaces;

namespace Services;

public class GroupServices(IGroupRepository groupRepository, IUnitOfWork unitOfWork, IAuditLogService? auditLogService = null) : IGroupService
{
    private const string DuplicateGroupNameMessage = "Group name must be unique in the system.";
    private const string GroupNotFoundMessage = "Group was not found.";

    public IReadOnlyCollection<Group> GetAllGroups()
    {
        return groupRepository.GetAll();
    }

    public void AddGroup(Group group)
    {
        ArgumentNullException.ThrowIfNull(group);

        if (groupRepository.ExistsByName(group.Name))
        {
            throw new GroupServicesException(DuplicateGroupNameMessage);
        }

        groupRepository.Add(group);
        auditLogService?.Log(
            AuditActions.CreateGroup,
            $"Group '{group.Name}' created.",
            nameof(Group));
        unitOfWork.Commit();
    }

    public Group GetByName(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        return groupRepository.GetByName(name)
               ?? throw new GroupServicesException(GroupNotFoundMessage);
    }

    public void RemoveByName(string name)
    {
        Group group = GetByName(name);
        groupRepository.Delete(group);
        auditLogService?.Log(
            AuditActions.DeleteGroup,
            $"Group '{group.Name}' removed.",
            nameof(Group));
        unitOfWork.Commit();
    }
}
