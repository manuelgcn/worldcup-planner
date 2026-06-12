using ProjectCopa;

namespace Services.Interfaces;

public interface IUserService
{
    IReadOnlyCollection<User> GetAllUsers();
    void AddUser(User user);
    void RemoveRole(int userId, UserRoles role);
    User Authenticate(string email, string password);
    User GetUserByEmail(string email);
    User GetUserById(int id);
    void RemoveUser(int id);
    void UpdateUser(int id, string name, string lastname, string email);
    void AssignRole(int userId, UserRoles role);
}
