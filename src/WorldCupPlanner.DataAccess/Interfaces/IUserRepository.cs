using WorldCupPlanner.Domain;

namespace WorldCupPlanner.DataAccess.Interfaces;

public interface IUserRepository
{
    void Add(User user);
    IReadOnlyCollection<User> GetAll();
    // El repositorio EF filtra por rol en la consulta; el default sirve a los dobles de test.
    IReadOnlyCollection<User> GetByRole(UserRoles role) => GetAll().Where(user => user.HasRole(role)).ToList();
    User? GetById(int id);
    User? GetByEmail(string email);
    bool ExistsByEmail(string email);
    void Remove(User user);
    void Update(User user);
}
