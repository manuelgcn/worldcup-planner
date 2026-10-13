using WorldCupPlanner.DataAccess.Interfaces;
using WorldCupPlanner.Domain;
using WorldCupPlanner.Services.Exceptions;
using WorldCupPlanner.Services.Interfaces;

namespace WorldCupPlanner.Services;

public class GroupServices : IGroupService
{
    private readonly IGroupRepository groupRepository;
    private readonly IUnitOfWork unitOfWork;
    private readonly IAuditLogService auditLogService;

    private const string DuplicateGroupNameMessage = "El nombre del grupo debe ser unico en el sistema.";
    private const string GroupNotFoundMessage = "No se encontro el grupo.";

    public GroupServices(IGroupRepository groupRepository, IUnitOfWork unitOfWork, IAuditLogService auditLogService)
    {
        ArgumentNullException.ThrowIfNull(auditLogService);
        this.groupRepository = groupRepository;
        this.unitOfWork = unitOfWork;
        this.auditLogService = auditLogService;
    }

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
        auditLogService.Log(
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
        auditLogService.Log(
            AuditActions.DeleteGroup,
            $"Group '{group.Name}' removed.",
            nameof(Group));
        unitOfWork.Commit();
    }
}
