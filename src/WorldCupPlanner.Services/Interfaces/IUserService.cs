using WorldCupPlanner.Domain;
using WorldCupPlanner.Services.DTOs;

namespace WorldCupPlanner.Services.Interfaces;

public interface IUserService
{
    IReadOnlyCollection<User> GetAllUsers();
    void AddUser(User user, string? performedByEmail = null);
    void RegisterUser(CreateUserRequestDto request);
    void CreateUser(CreateUserRequestDto request, string? performedByEmail);
    void RemoveRole(int userId, UserRoles role, string? performedByEmail = null);
    User Authenticate(string email, string password);
    User GetUserByEmail(string email);
    User GetUserById(int id);
    void RemoveUser(int id, string? performedByEmail = null);
    void UpdateUser(int id, string name, string lastname, string email, string? performedByEmail = null);
    void AssignRole(int userId, UserRoles role, string? performedByEmail = null);
    void ToggleRole(int userId, UserRoles role, string? performedByEmail = null);
}
