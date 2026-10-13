using WorldCupPlanner.DataAccess;
using WorldCupPlanner.DataAccess.Repositories;
using WorldCupPlanner.DataAccess.Tests.TestDoubles;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WorldCupPlanner.Domain;

namespace WorldCupPlanner.DataAccess.Tests;

[TestClass]
public class UserRepositoryTests
{
    private InMemoryDbContextFactory _factory = null!;
    private UserRepository _repository = null!;

    [TestInitialize]
    public void Setup()
    {
        // Contextos efimeros sobre una BD InMemory aislada por test (mismo patron que TeamRepositoryTests).
        _factory = new InMemoryDbContextFactory();
        _repository = new UserRepository(_factory);
    }

    private static User CreateUser(string email, params UserRoles[] roles)
    {
        // idUser = 0: la BD lo asigna (IDENTITY), igual que en el alta real desde Signup.
        User user = new User(0, "Juan", "Perez", email, "Pass123@", new DateTime(1990, 5, 15));
        foreach (UserRoles role in roles)
            user.AddRole(role);
        return user;
    }

    private int CountRoleRows(int userId)
    {
        using AppDbContext context = _factory.CreateDbContext();
        return context.UserRoles.Count(r => r.UserId == userId);
    }

    [TestMethod]
    public void AddPersistsUserWithRolesRetrievableFromNewContext()
    {
        User user = CreateUser("juan@mail.com", UserRoles.Editor);

        _repository.Add(user);                              // SaveChanges interno (autosave)

        User? stored = _repository.GetByEmail("juan@mail.com");   // contexto nuevo: el dato viene de la BD
        Assert.IsNotNull(stored);
        Assert.AreEqual("juan@mail.com", stored!.email);
        Assert.AreEqual("Juan", stored.name);
        Assert.AreEqual(1, stored.Roles.Count);
        Assert.IsTrue(stored.HasRole(UserRoles.Editor));
    }

    [TestMethod]
    public void AddAssignsIdentityIdOnTheEntity()
    {
        User user = CreateUser("juan@mail.com", UserRoles.Editor);

        _repository.Add(user);

        Assert.AreNotEqual(0, user.idUser);                 // la BD asigna el id y EF lo refleja en la entidad
    }

    [TestMethod]
    public void GetByIdReturnsUserWithRoles()
    {
        User user = CreateUser("juan@mail.com", UserRoles.Editor, UserRoles.Admin);
        _repository.Add(user);

        User? stored = _repository.GetById(user.idUser);

        Assert.IsNotNull(stored);
        Assert.AreEqual("juan@mail.com", stored!.email);
        Assert.AreEqual(2, stored.Roles.Count);
    }

    [TestMethod]
    public void GetByIdReturnsNullWhenUserDoesNotExist()
    {
        Assert.IsNull(_repository.GetById(999));
    }

    [TestMethod]
    public void GetAllReturnsUsersWithRoles()
    {
        _repository.Add(CreateUser("a@mail.com", UserRoles.Admin));
        _repository.Add(CreateUser("b@mail.com", UserRoles.Editor));

        IReadOnlyCollection<User> all = _repository.GetAll();

        Assert.AreEqual(2, all.Count);
        Assert.IsTrue(all.All(u => u.Roles.Count == 1));
    }

    [TestMethod]
    public void ExistsByEmailReturnsTrueWhenUserIsStored()
    {
        _repository.Add(CreateUser("juan@mail.com", UserRoles.Editor));

        Assert.IsTrue(_repository.ExistsByEmail("juan@mail.com"));
    }

    [TestMethod]
    public void ExistsByEmailReturnsFalseWhenUserIsNotStored()
    {
        Assert.IsFalse(_repository.ExistsByEmail("nope@mail.com"));
    }

    [TestMethod]
    public void UpdatePersistsPersonalDataChanges()
    {
        User user = CreateUser("juan@mail.com", UserRoles.Editor);
        _repository.Add(user);

        User stored = _repository.GetById(user.idUser)!;
        stored.UpdatePersonalData("Juan Carlos", "Gomez", "jc@mail.com");
        _repository.Update(stored);

        User? reloaded = _repository.GetByEmail("jc@mail.com");
        Assert.IsNotNull(reloaded);
        Assert.AreEqual("Juan Carlos", reloaded!.name);
        Assert.AreEqual("Gomez", reloaded.lastname);
        Assert.IsNull(_repository.GetByEmail("juan@mail.com"));
        Assert.AreEqual(1, reloaded.Roles.Count);           // los roles se preservan
    }

    [TestMethod]
    public void UpdateAddsNewRoleWithoutDuplicating()
    {
        User user = CreateUser("juan@mail.com", UserRoles.Editor);
        _repository.Add(user);

        User stored = _repository.GetById(user.idUser)!;
        stored.AddRole(UserRoles.Admin);
        _repository.Update(stored);

        User reloaded = _repository.GetById(user.idUser)!;
        Assert.AreEqual(2, reloaded.Roles.Count);
        Assert.IsTrue(reloaded.HasRole(UserRoles.Editor));
        Assert.IsTrue(reloaded.HasRole(UserRoles.Admin));
        Assert.AreEqual(2, CountRoleRows(user.idUser));     // sin filas duplicadas
    }

    [TestMethod]
    public void UpdateRemovesRoleFromTable()
    {
        User user = CreateUser("juan@mail.com", UserRoles.Editor, UserRoles.Admin);
        _repository.Add(user);

        User stored = _repository.GetById(user.idUser)!;
        stored.RemoveRoles(UserRoles.Admin);
        _repository.Update(stored);

        User reloaded = _repository.GetById(user.idUser)!;
        Assert.AreEqual(1, reloaded.Roles.Count);
        Assert.IsTrue(reloaded.HasRole(UserRoles.Editor));
        Assert.IsFalse(reloaded.HasRole(UserRoles.Admin));
        Assert.AreEqual(1, CountRoleRows(user.idUser));     // el rol quitado se borra de la tabla
    }

    [TestMethod]
    public void UpdateDoesNothingWhenUserDoesNotExist()
    {
        User ghost = CreateUser("ghost@mail.com", UserRoles.Editor);   // nunca se agrega al repositorio

        _repository.Update(ghost);                          // no debe explotar ni insertar (semantica no-op)

        Assert.AreEqual(0, _repository.GetAll().Count);
        Assert.IsNull(_repository.GetByEmail("ghost@mail.com"));
    }

    [TestMethod]
    public void RemoveDeletesUserAndRolesViaCascade()
    {
        User user = CreateUser("juan@mail.com", UserRoles.Editor, UserRoles.Admin);
        _repository.Add(user);
        int id = user.idUser;

        _repository.Remove(user);

        Assert.IsNull(_repository.GetById(id));
        Assert.AreEqual(0, CountRoleRows(id));              // cascade borra los roles
    }
}
