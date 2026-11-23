using System.Security.Cryptography;
using System.Text;
using Moq;
using WorldCupPlanner.DataAccess.Interfaces;
using WorldCupPlanner.Domain;
using WorldCupPlanner.Services;
using WorldCupPlanner.Services.DTOs;
using WorldCupPlanner.Services.Exceptions;
using WorldCupPlanner.Services.Tests.TestDoubles.Mocks;

namespace WorldCupPlanner.Services.Tests;

[TestClass]
public class UserServicesTests
{
    private const int MaxTrackedLogins = 10_000;
    private const int MaxEmailLength = 254;

    private UserServices service;
    private MockUserRepository repository;

    [TestInitialize]
    public void Setup()
    {
        repository = new MockUserRepository();
        service = new UserServices(repository, new MockAuditLogService());
    }

    private static User BuildUser(
        int id,
        string name,
        string lastname,
        string email,
        string password,
        DateTime dateOfBirth,
        UserRoles role = UserRoles.Editor)
    {
        User user = new User(id, name, lastname, email, password, dateOfBirth);
        user.AddRole(role);
        return user;
    }

    [TestMethod]
    public void RegisterUser_BuildsDomainUserFromRequestAndPersistsIt()
    {
        service.RegisterUser(new CreateUserRequestDto
        {
            Id = 5,
            Name = "Ana",
            Lastname = "Lopez",
            Email = "ana@mail.com",
            Password = "Pass123@",
            DateOfBirth = new DateTime(1992, 4, 10),
            InitialRole = UserRoles.Editor
        });

        User stored = service.GetUserById(5);
        Assert.AreEqual("ana@mail.com", stored.email);
        // El registro publico siempre crea Periodista, sin importar el rol del request.
        Assert.IsTrue(stored.HasRole(UserRoles.Periodista));
    }

    [TestMethod]
    public void RegisterUser_IgnoresRequestedPrivilegedRole_AndForcesPeriodista()
    {
        service.RegisterUser(new CreateUserRequestDto
        {
            Id = 9,
            Name = "Eva",
            Lastname = "Diaz",
            Email = "eva@mail.com",
            Password = "Pass123@",
            DateOfBirth = new DateTime(1992, 4, 10),
            InitialRole = UserRoles.Admin
        });

        User stored = service.GetUserById(9);
        Assert.IsTrue(stored.HasRole(UserRoles.Periodista));
        Assert.IsFalse(stored.HasRole(UserRoles.Admin));
        Assert.IsFalse(stored.HasRole(UserRoles.Editor));
        Assert.AreEqual(1, stored.Roles.Count);
    }

    [TestMethod]
    public void RegisterUser_WithInvalidEmail_ThrowsArgumentException()
    {
        Assert.ThrowsException<ArgumentException>(() => service.RegisterUser(new CreateUserRequestDto
        {
            Id = 6,
            Name = "Ana",
            Lastname = "Lopez",
            Email = "correo-invalido",
            Password = "Pass123@",
            DateOfBirth = new DateTime(1992, 4, 10),
            InitialRole = UserRoles.Editor
        }));
    }

    [TestMethod]
    public void RegisterUser_WithDuplicateEmail_ThrowsUserServicesException()
    {
        service.RegisterUser(new CreateUserRequestDto
        {
            Id = 7,
            Name = "Ana",
            Lastname = "Lopez",
            Email = "ana@mail.com",
            Password = "Pass123@",
            DateOfBirth = new DateTime(1992, 4, 10),
            InitialRole = UserRoles.Editor
        });

        Assert.ThrowsException<UserServicesException>(() => service.RegisterUser(new CreateUserRequestDto
        {
            Id = 8,
            Name = "Otra",
            Lastname = "Persona",
            Email = "ana@mail.com",
            Password = "Pass123@",
            DateOfBirth = new DateTime(1990, 1, 1),
            InitialRole = UserRoles.Editor
        }));
    }

    [TestMethod]
    public void CreateUser_KeepsTheRoleChosenByTheAdmin_AndAuditsTheActor()
    {
        MockAuditLogService audit = new MockAuditLogService();
        UserServices auditedService = new UserServices(repository, audit);

        auditedService.CreateUser(new CreateUserRequestDto
        {
            Id = 11,
            Name = "Leo",
            Lastname = "Ruiz",
            Email = "leo@mail.com",
            Password = "Pass123@",
            DateOfBirth = new DateTime(1992, 4, 10),
            InitialRole = UserRoles.Editor
        }, "admin@copa.com");

        User stored = auditedService.GetUserById(11);
        Assert.IsTrue(stored.HasRole(UserRoles.Editor));
        Assert.AreEqual(1, stored.Roles.Count);
        Assert.AreEqual("admin@copa.com", audit.LoggedActions.Single().PerformedByEmail);
    }

    [TestMethod]
    public void CanAddUser()
    {
        User user = BuildUser(1, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15));
        service.AddUser(user);
        Assert.AreEqual(1, service.GetAllUsers().Count);
    }

    [TestMethod]
    public void AddUser_LogsAuditEntry()
    {
        MockUserRepository userRepo = new MockUserRepository();
        MockAuditLogRepository auditRepo = new MockAuditLogRepository();

        UserServices auditedService = new UserServices(
            userRepo,
            AuditLogService.CreateInstance(auditRepo)
        );

        User user = BuildUser(1, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15));

        auditedService.AddUser(user);

        List<AuditLog> logs = auditRepo.GetAll();
        Assert.AreEqual(1, logs.Count);
        Assert.AreEqual("CREATE_USER", logs[0].Action);
    }

    [TestMethod]
    public void UpdateUser_LogsAuditEntry()
    {
        MockUserRepository userRepo = new MockUserRepository();
        MockAuditLogRepository auditRepo = new MockAuditLogRepository();

        UserServices auditedService = new UserServices(
            userRepo,
            AuditLogService.CreateInstance(auditRepo)
        );

        User user = BuildUser(2, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15));
        auditedService.AddUser(user);

        auditedService.UpdateUser(2, "Juan Carlos", "Perez", "juan.c@mail.com");

        Assert.IsTrue(auditRepo.GetAll().Any(log => log.Action == "UPDATE_USER"));
    }

    [TestMethod]
    public void AddUser_RecordsPerformingActor()
    {
        MockAuditLogRepository auditRepo = new MockAuditLogRepository();
        UserServices auditedService = new UserServices(
            new MockUserRepository(),
            AuditLogService.CreateInstance(auditRepo));

        User user = BuildUser(2, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15));

        auditedService.AddUser(user, performedByEmail: "admin@copa.com");

        AuditLog log = auditRepo.GetAll().Single(l => l.Action == "CREATE_USER");
        Assert.AreEqual("admin@copa.com", log.PerformedByEmail);
    }

    [TestMethod]
    public void UpdateUser_RecordsPerformingActor()
    {
        MockAuditLogRepository auditRepo = new MockAuditLogRepository();
        UserServices auditedService = new UserServices(
            new MockUserRepository(),
            AuditLogService.CreateInstance(auditRepo));

        User user = BuildUser(2, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15));
        auditedService.AddUser(user);

        auditedService.UpdateUser(2, "Juan Carlos", "Perez", "juan.c@mail.com", performedByEmail: "admin@copa.com");

        AuditLog log = auditRepo.GetAll().Single(l => l.Action == "UPDATE_USER");
        Assert.AreEqual("admin@copa.com", log.PerformedByEmail);
    }

    [TestMethod]
    public void RemoveUser_RecordsPerformingActor()
    {
        MockAuditLogRepository auditRepo = new MockAuditLogRepository();
        UserServices auditedService = new UserServices(
            new MockUserRepository(),
            AuditLogService.CreateInstance(auditRepo));

        User user = BuildUser(2, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15));
        auditedService.AddUser(user);

        auditedService.RemoveUser(2, performedByEmail: "admin@copa.com");

        AuditLog log = auditRepo.GetAll().Single(l => l.Action == "DELETE_USER");
        Assert.AreEqual("admin@copa.com", log.PerformedByEmail);
    }

    [TestMethod]
    public void AssignRole_RecordsPerformingActor()
    {
        MockAuditLogRepository auditRepo = new MockAuditLogRepository();
        UserServices auditedService = new UserServices(
            new MockUserRepository(),
            AuditLogService.CreateInstance(auditRepo));

        User user = BuildUser(2, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15));
        auditedService.AddUser(user);

        auditedService.AssignRole(2, UserRoles.Admin, performedByEmail: "admin@copa.com");

        AuditLog log = auditRepo.GetAll().Single(l => l.Action == "ASSIGN_USER_ROLE");
        Assert.AreEqual("admin@copa.com", log.PerformedByEmail);
    }

    [TestMethod]
    public void Authenticate_WithValidCredentials_ReturnsUser()
    {
        User user = BuildUser(1, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15));
        service.AddUser(user);

        User authenticated = service.Authenticate("juan@mail.com", "Pass123@");

        Assert.AreEqual(user.idUser, authenticated.idUser);
    }

    [TestMethod]
    public void Authenticate_WithALegacyHash_UpgradesItAndAuditsTheUpgrade()
    {
        MockAuditLogService audit = new MockAuditLogService();
        UserServices auditedService = new UserServices(repository, audit);
        User user = BuildUser(1, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15));
        string legacyHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes("Pass123@")));
        typeof(User).GetProperty(nameof(User.password))!.SetValue(user, legacyHash);
        repository.Add(user);

        auditedService.Authenticate("juan@mail.com", "Pass123@");

        Assert.IsFalse(user.HasLegacyPasswordHash);
        Assert.IsTrue(audit.LoggedActions.Any(log => log.Action == AuditActions.UpgradePasswordHash && log.PerformedByEmail == "juan@mail.com"));
    }

    // El "sal:hash" de 100.000 iteraciones de admin1@copa.com en db/scripts/seed-full.sql.
    [TestMethod]
    public void Authenticate_WithASaltedHashWeakerThanTheCurrentOne_UpgradesItAndAuditsTheUpgrade()
    {
        MockAuditLogService audit = new MockAuditLogService();
        UserServices auditedService = new UserServices(repository, audit);
        User user = new User { idUser = 1, email = "admin1@copa.com" };
        typeof(User).GetProperty(nameof(User.password))!.SetValue(user, "NayVITcSJcByRMrVNU74ug==:mbu4770CN/kEqP065+upgXhAC4zzKJ9smvFh25ZH5BU=");
        repository.Add(user);

        auditedService.Authenticate("admin1@copa.com", "Admin@123");

        StringAssert.StartsWith(user.password, "v2:600000:");
        Assert.IsTrue(user.PasswordMatches("Admin@123"));
        Assert.IsTrue(audit.LoggedActions.Any(log => log.Action == AuditActions.UpgradePasswordHash && log.PerformedByEmail == "admin1@copa.com"));
    }

    [TestMethod]
    public void Authenticate_WithACurrentHash_LeavesItAsItIs()
    {
        MockAuditLogService audit = new MockAuditLogService();
        UserServices auditedService = new UserServices(repository, audit);
        User user = BuildUser(1, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15));
        string stored = user.password;
        repository.Add(user);

        auditedService.Authenticate("juan@mail.com", "Pass123@");

        Assert.AreEqual(stored, user.password);
        Assert.IsFalse(audit.LoggedActions.Any(log => log.Action == AuditActions.UpgradePasswordHash));
    }

    [TestMethod]
    public void Authenticate_WithWrongPassword_Throws()
    {
        User user = BuildUser(1, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15));
        service.AddUser(user);

        Assert.ThrowsException<UserServicesException>(() => service.Authenticate("juan@mail.com", "Wrong123@"));
    }

    [TestMethod]
    public void Authenticate_WithUnknownEmail_Throws()
    {
        Assert.ThrowsException<UserServicesException>(() => service.Authenticate("nobody@mail.com", "Pass123@"));
    }

    // Un email desconocido tiene que verse igual que una contrasena equivocada: mismo mensaje y mismo conteo.
    [TestMethod]
    public void Authenticate_WithUnknownEmail_FailsWithTheMessageOfAWrongPassword()
    {
        service.AddUser(BuildUser(1, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15)));

        UserServicesException wrongPassword = Assert.ThrowsException<UserServicesException>(
            () => service.Authenticate("juan@mail.com", "Wrong123@"));
        UserServicesException unknownEmail = Assert.ThrowsException<UserServicesException>(
            () => service.Authenticate("nobody@mail.com", "Wrong123@"));

        Assert.AreEqual(wrongPassword.Message, unknownEmail.Message);
    }

    [TestMethod]
    public void Authenticate_WithUnknownEmail_CountsTheFailuresAndLocksIt()
    {
        UserServices lockingService = new UserServices(repository, new MockAuditLogService(), new ManualTimeProvider());
        FailLogins(lockingService, "nobody@mail.com", times: 5);

        UserServicesException locked = Assert.ThrowsException<UserServicesException>(
            () => lockingService.Authenticate("nobody@mail.com", "Pass123@"));

        StringAssert.Contains(locked.Message, "intentos");
        Assert.AreEqual(1, lockingService.TrackedLoginCount);
    }

    [TestMethod]
    public void Authenticate_AfterFiveFailedAttempts_RejectsEvenTheRightPassword()
    {
        UserServices lockingService = new UserServices(repository, new MockAuditLogService(), new ManualTimeProvider());
        lockingService.AddUser(BuildUser(1, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15)));
        FailLogins(lockingService, "juan@mail.com", times: 5);

        UserServicesException locked = Assert.ThrowsException<UserServicesException>(
            () => lockingService.Authenticate("juan@mail.com", "Pass123@"));

        StringAssert.Contains(locked.Message, "intentos");
    }

    [TestMethod]
    public void Authenticate_AfterTheLockoutWindow_AcceptsTheRightPassword()
    {
        ManualTimeProvider clock = new ManualTimeProvider();
        UserServices lockingService = new UserServices(repository, new MockAuditLogService(), clock);
        lockingService.AddUser(BuildUser(1, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15)));
        FailLogins(lockingService, "juan@mail.com", times: 5);

        clock.Advance(TimeSpan.FromMinutes(5));

        Assert.AreEqual(1, lockingService.Authenticate("juan@mail.com", "Pass123@").idUser);
    }

    [TestMethod]
    public void Authenticate_SuccessResetsTheFailedAttempts()
    {
        UserServices lockingService = new UserServices(repository, new MockAuditLogService(), new ManualTimeProvider());
        lockingService.AddUser(BuildUser(1, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15)));
        FailLogins(lockingService, "juan@mail.com", times: 4);
        lockingService.Authenticate("juan@mail.com", "Pass123@");
        FailLogins(lockingService, "JUAN@mail.com ", times: 4);

        Assert.AreEqual(1, lockingService.Authenticate("juan@mail.com", "Pass123@").idUser);
    }

    [TestMethod]
    public void Authenticate_FailuresOlderThanTheWindow_DoNotCountTowardsTheLock()
    {
        ManualTimeProvider clock = new ManualTimeProvider();
        UserServices lockingService = new UserServices(repository, new MockAuditLogService(), clock);
        lockingService.AddUser(BuildUser(1, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15)));
        FailLogins(lockingService, "juan@mail.com", times: 4);

        clock.Advance(TimeSpan.FromDays(7));
        FailLogins(lockingService, "juan@mail.com", times: 1);

        Assert.AreEqual(1, lockingService.Authenticate("juan@mail.com", "Pass123@").idUser);
    }

    // Guarda: el bloqueo dura cinco minutos desde el quinto fallo, no desde el primero.
    [TestMethod]
    public void Authenticate_LockLastsFiveMinutesFromTheFifthFailure()
    {
        ManualTimeProvider clock = new ManualTimeProvider();
        UserServices lockingService = new UserServices(repository, new MockAuditLogService(), clock);
        lockingService.AddUser(BuildUser(1, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15)));
        FailLogins(lockingService, "juan@mail.com", times: 4);
        clock.Advance(TimeSpan.FromMinutes(4));
        FailLogins(lockingService, "juan@mail.com", times: 1);

        clock.Advance(TimeSpan.FromMinutes(4));

        UserServicesException locked = Assert.ThrowsException<UserServicesException>(
            () => lockingService.Authenticate("juan@mail.com", "Pass123@"));
        StringAssert.Contains(locked.Message, "intentos");
    }

    [TestMethod]
    public void Authenticate_WhenTrackedEmailsReachTheCap_ForgetsTheExpiredOnes()
    {
        ManualTimeProvider clock = new ManualTimeProvider();
        UserServices lockingService = new UserServices(AccountsThatFailFast("random"), new MockAuditLogService(), clock);
        FailLoginOnDistinctEmails(lockingService, "random", MaxTrackedLogins);

        clock.Advance(TimeSpan.FromMinutes(5));
        FailLogins(lockingService, "nobody@mail.com", times: 1);

        Assert.AreEqual(1, lockingService.TrackedLoginCount);
    }

    [TestMethod]
    public void Authenticate_WhenTheCapIsFullOfLiveEntries_DropsTheOneClosestToExpiry()
    {
        ManualTimeProvider clock = new ManualTimeProvider();
        UserServices lockingService = new UserServices(AccountsThatFailFast("random"), new MockAuditLogService(), clock);
        FailLogins(lockingService, "oldest@mail.com", times: 4);
        clock.Advance(TimeSpan.FromMinutes(1));
        FailLogins(lockingService, "locked@mail.com", times: 5);
        FailLoginOnDistinctEmails(lockingService, "random", MaxTrackedLogins - 2);

        FailLogins(lockingService, "nobody@mail.com", times: 1);

        Assert.AreEqual(MaxTrackedLogins, lockingService.TrackedLoginCount);
        UserServicesException locked = Assert.ThrowsException<UserServicesException>(
            () => lockingService.Authenticate("locked@mail.com", "Pass123@"));
        StringAssert.Contains(locked.Message, "intentos");

        // Si "oldest" siguiera en memoria, este seria su quinto fallo y el intento siguiente quedaria bloqueado.
        FailLogins(lockingService, "oldest@mail.com", times: 1);
        UserServicesException forgotten = Assert.ThrowsException<UserServicesException>(
            () => lockingService.Authenticate("oldest@mail.com", "Pass123@"));
        StringAssert.Contains(forgotten.Message, "Credenciales");
    }

    // La base puede resolver a la misma cuenta un email escrito distinto (aca, con una "j" de ancho completo).
    [TestMethod]
    public void Authenticate_WithSpellingsTheDatabaseMapsToOneAccount_SharesOneCounter()
    {
        const string storedEmail = "juan@mail.com";
        const string fullWidthSpelling = "ｊuan@mail.com";
        User user = BuildUser(1, "Juan", "Perez", storedEmail, "Pass123@", new DateTime(1990, 5, 15));
        Mock<IUserRepository> collatingRepository = new Mock<IUserRepository>();
        collatingRepository.Setup(users => users.GetByEmail(It.IsIn(storedEmail, fullWidthSpelling))).Returns(user);
        UserServices lockingService = new UserServices(
            collatingRepository.Object, new MockAuditLogService(), new ManualTimeProvider());
        FailLogins(lockingService, storedEmail, times: 3);
        FailLogins(lockingService, fullWidthSpelling, times: 2);

        foreach (string spelling in new[] { storedEmail, fullWidthSpelling })
        {
            UserServicesException locked = Assert.ThrowsException<UserServicesException>(
                () => lockingService.Authenticate(spelling, "Pass123@"));
            StringAssert.Contains(locked.Message, "intentos");
        }
    }

    // Un email mas largo que el de cualquier cuenta no puede ocupar memoria sin limite como clave.
    [TestMethod]
    public void Authenticate_WithEmailsLongerThanAnyAccount_TracksOnlyTheirFirstCharacters()
    {
        string sharedStart = new string('a', MaxEmailLength);
        FailLogins(service, sharedStart + "1@mail.com", times: 1);
        FailLogins(service, sharedStart + "2@mail.com", times: 1);

        Assert.AreEqual(1, service.TrackedLoginCount);
    }

    // Los emails que empiezan con el prefijo son cuentas con el hash vacio, que nunca coincide ni pasa por PBKDF2:
    // asi llenar el tope con miles de emails no depende de cuanto cueste verificar una contrasena.
    private static IUserRepository AccountsThatFailFast(string prefix)
    {
        Mock<IUserRepository> accounts = new Mock<IUserRepository>();
        accounts.Setup(users => users.GetByEmail(It.Is<string>(email => email.StartsWith(prefix))))
            .Returns((string email) => new User { email = email });
        return accounts.Object;
    }

    private static void FailLoginOnDistinctEmails(UserServices target, string prefix, int emails)
    {
        for (int index = 0; index < emails; index++)
        {
            FailLogins(target, $"{prefix}{index}@mail.com", times: 1);
        }
    }

    private static void FailLogins(UserServices target, string email, int times)
    {
        for (int attempt = 0; attempt < times; attempt++)
        {
            Assert.ThrowsException<UserServicesException>(() => target.Authenticate(email, "Wrong123@"));
        }
    }

    [TestMethod]
    public void RemoveUser_LogsAuditEntry()
    {
        MockUserRepository userRepo = new MockUserRepository();
        MockAuditLogRepository auditRepo = new MockAuditLogRepository();

        UserServices auditedService = new UserServices(
            userRepo,
            AuditLogService.CreateInstance(auditRepo)
        );

        User user = BuildUser(2, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15));
        auditedService.AddUser(user);

        auditedService.RemoveUser(2);

        Assert.IsTrue(auditRepo.GetAll().Any(log => log.Action == "DELETE_USER"));
    }

    [TestMethod]
    public void AssignRole_LogsAuditEntry()
    {
        MockUserRepository userRepo = new MockUserRepository();
        MockAuditLogRepository auditRepo = new MockAuditLogRepository();

        UserServices auditedService = new UserServices(
            userRepo,
            AuditLogService.CreateInstance(auditRepo)
        );

        User user = BuildUser(1, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15));
        auditedService.AddUser(user);

        auditedService.AssignRole(1, UserRoles.Admin);

        Assert.IsTrue(auditRepo.GetAll().Any(log => log.Action == "ASSIGN_USER_ROLE"));
    }

    [TestMethod]
    public void CanGetUserByEmail()
    {
        User user = BuildUser(1, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15));
        service.AddUser(user);

        User result = service.GetUserByEmail("juan@mail.com");

        Assert.AreEqual(user, result);
    }

    [TestMethod]
    public void GetUserByEmailThrowsIfNotFound()
    {
        Assert.ThrowsException<UserServicesException>(() =>
            service.GetUserByEmail("noexiste@mail.com"));
    }

    [TestMethod]
    public void CanGetUserById()
    {
        User user = BuildUser(1, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15));
        service.AddUser(user);

        User result = service.GetUserById(1);

        Assert.AreEqual(user, result);
    }

    [TestMethod]
    public void AddUserThrowsWhenEmailAlreadyExists()
    {
        User firstUser = BuildUser(1, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15));
        User secondUser = BuildUser(2, "Ana", "Lopez", "juan@mail.com", "Pass123@", new DateTime(1995, 3, 10));
        service.AddUser(firstUser);

        Assert.ThrowsException<UserServicesException>(() => service.AddUser(secondUser));
    }

    [TestMethod]
    public void AddUserThrowsWhenIdAlreadyExists()
    {
        User firstUser = BuildUser(1, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15));
        User secondUser = BuildUser(1, "Ana", "Lopez", "ana@mail.com", "Pass123@", new DateTime(1995, 3, 10));
        service.AddUser(firstUser);

        Assert.ThrowsException<UserServicesException>(() => service.AddUser(secondUser));
    }

    [TestMethod]
    public void CanRemoveUser()
    {
        // idUser != 1: el id 1 esta reservado al admin del sistema y la regla de negocio impide eliminarlo.
        User user = BuildUser(2, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15));
        service.AddUser(user);
        service.RemoveUser(2);
        Assert.AreEqual(0, service.GetAllUsers().Count);
    }

    [TestMethod]
    public void CannotRemoveNonExistingUser()
    {
        Assert.ThrowsException<UserServicesException>(() =>
            service.RemoveUser(99));
    }

    [TestMethod]
    public void CanUpdateUserData()
    {
        User user = BuildUser(2, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15));

        service.AddUser(user);

        service.UpdateUser(2, "Juan Carlos", "Perez", "juan.c@mail.com");

        User updated = service.GetUserById(2);

        Assert.AreEqual("Juan Carlos", updated.name);
        Assert.AreEqual("juan.c@mail.com", updated.email);
    }

    [TestMethod]
    public void CannotUpdateUserThatDoesNotExist()
    {
        Assert.ThrowsException<UserServicesException>(() =>
            service.UpdateUser(99, "Ana", "Lopez", "ana@mail.com"));
    }

    [TestMethod]
    public void CannotUpdateUserWithDuplicatedEmail()
    {
        User user1 = BuildUser(1, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15));
        User user2 = BuildUser(2, "Ana", "Lopez", "ana@mail.com", "Pass123@", new DateTime(1995, 3, 10));

        service.AddUser(user1);
        service.AddUser(user2);

        Assert.ThrowsException<UserServicesException>(() =>
            service.UpdateUser(2, "Ana", "Lopez", "juan@mail.com"));
    }

    [TestMethod]
    public void AdminCanAssignRoleToUser()
    {
        User user = BuildUser(1, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15));

        service.AddUser(user);

        service.AssignRole(1, UserRoles.Admin);

        User updated = service.GetUserById(1);

        Assert.IsTrue(updated.HasRole(UserRoles.Admin));
    }

    [TestMethod]
    public void CannotAssignRoleToNonExistingUser()
    {
        Assert.ThrowsException<UserServicesException>(() =>
            service.AssignRole(99, UserRoles.Editor));
    }

    [TestMethod]
    public void GetAllUsers_ReturnsAllRegisteredUsersInInsertionOrder()
    {
        User first = BuildUser(1, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15));
        User second = BuildUser(2, "Ana", "Lopez", "ana@mail.com", "Pass123@", new DateTime(1995, 3, 10));

        service.AddUser(first);
        service.AddUser(second);

        List<User> all = service.GetAllUsers().ToList();

        Assert.AreEqual(2, all.Count);
        Assert.AreEqual(1, all[0].idUser);
        Assert.AreEqual(2, all[1].idUser);
    }

    [TestMethod]
    public void FullAbmLifecycle_ExecutesEveryAuditAction()
    {
        MockUserRepository userRepo = new MockUserRepository();
        MockAuditLogRepository auditRepo = new MockAuditLogRepository();

        UserServices auditedService = new UserServices(
            userRepo,
            AuditLogService.CreateInstance(auditRepo)
        );

        User user = BuildUser(2, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15));

        auditedService.AddUser(user);
        auditedService.UpdateUser(2, "Juan Carlos", "Perez", "juan.c@mail.com");
        auditedService.AssignRole(2, UserRoles.Admin);
        auditedService.RemoveUser(2);

        List<string> actions = auditRepo.GetAll().Select(log => log.Action).ToList();

        Assert.IsTrue(actions.Contains("CREATE_USER"));
        Assert.IsTrue(actions.Contains("UPDATE_USER"));
        Assert.IsTrue(actions.Contains("ASSIGN_USER_ROLE"));
        Assert.IsTrue(actions.Contains("DELETE_USER"));
        Assert.AreEqual(0, auditedService.GetAllUsers().Count);
    }

    [TestMethod]
    public void AddUser_WithoutAnyRole_Throws()
    {
        User user = new User(3, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15));

        Assert.ThrowsException<UserServicesException>(() => service.AddUser(user));
    }

    [TestMethod]
    public void RemoveUser_SystemAdmin_Throws()
    {
        Assert.ThrowsException<UserServicesException>(() => service.RemoveUser(1));
    }

    [TestMethod]
    public void RemoveUser_ActorDeletingThemselves_Throws()
    {
        // El actor (id 2) intenta eliminar su propia cuenta usando su email como performedBy.
        User actor = BuildUser(2, "Admin", "Self", "self@mail.com", "Pass123@", new DateTime(1990, 5, 15));
        service.AddUser(actor);

        Assert.ThrowsException<UserServicesException>(
            () => service.RemoveUser(2, performedByEmail: "self@mail.com"));

        // La cuenta sigue existiendo: la auto-eliminacion fue rechazada.
        Assert.AreEqual(1, service.GetAllUsers().Count);
    }

    [TestMethod]
    public void RemoveUser_ActorDeletingAnotherUser_Succeeds()
    {
        User actor = BuildUser(2, "Admin", "Real", "admin@mail.com", "Pass123@", new DateTime(1990, 5, 15));
        User target = BuildUser(3, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1991, 6, 16));
        service.AddUser(actor);
        service.AddUser(target);

        service.RemoveUser(3, performedByEmail: "admin@mail.com");

        Assert.IsNull(service.GetAllUsers().FirstOrDefault(u => u.idUser == 3));
        Assert.IsNotNull(service.GetAllUsers().FirstOrDefault(u => u.idUser == 2));
    }

    [TestMethod]
    public void UpdateUser_SystemAdmin_Throws()
    {
        Assert.ThrowsException<UserServicesException>(() =>
            service.UpdateUser(1, "Ana", "Lopez", "ana@mail.com"));
    }

    [TestMethod]
    public void UpdateUser_KeepingSameEmail_Succeeds()
    {
        User user = BuildUser(2, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15));
        service.AddUser(user);

        service.UpdateUser(2, "Juan Carlos", "Perez", "juan@mail.com");

        User updated = service.GetUserById(2);
        Assert.AreEqual("Juan Carlos", updated.name);
        Assert.AreEqual("juan@mail.com", updated.email);
    }

    // Con la cuenta en el id 1 la unica razon para rechazar es que sea el administrador del sistema.
    [TestMethod]
    public void ChangePassword_SystemAdmin_Throws()
    {
        User admin = BuildUser(1, "Admin", "Sistema", "admin@copa.com", "Admin@123", new DateTime(1990, 1, 1), UserRoles.Admin);
        string stored = admin.password;
        service.AddUser(admin);

        Assert.ThrowsException<UserServicesException>(() => service.ChangePassword(1, "Admin@123", "Nueva456#"));
        Assert.AreEqual(stored, admin.password);
    }

    [TestMethod]
    public void ChangePassword_UserThatDoesNotExist_Throws()
    {
        Assert.ThrowsException<UserServicesException>(() => service.ChangePassword(99, "Pass123@", "Nueva456#"));
    }

    [TestMethod]
    public void RemoveRole_SystemAdmin_Throws()
    {
        Assert.ThrowsException<UserServicesException>(() =>
            service.RemoveRole(1, UserRoles.Editor));
    }

    [TestMethod]
    public void RemoveRole_WhenUserDoesNotHaveRole_DoesNothing()
    {
        User user = BuildUser(2, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15), UserRoles.Editor);
        service.AddUser(user);

        service.RemoveRole(2, UserRoles.Admin);

        User unchanged = service.GetUserById(2);
        Assert.IsTrue(unchanged.HasRole(UserRoles.Editor));
        Assert.AreEqual(1, unchanged.Roles.Count);
    }

    [TestMethod]
    public void RemoveRole_WhenUserHasOnlyOneRole_Throws()
    {
        User user = BuildUser(2, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15), UserRoles.Editor);
        service.AddUser(user);

        Assert.ThrowsException<UserServicesException>(() =>
            service.RemoveRole(2, UserRoles.Editor));
    }

    [TestMethod]
    public void RemoveRole_RemovesRoleAndLogsAuditEntry()
    {
        MockAuditLogRepository auditRepo = new MockAuditLogRepository();
        UserServices auditedService = new UserServices(
            new MockUserRepository(),
            AuditLogService.CreateInstance(auditRepo));

        User user = BuildUser(2, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15), UserRoles.Editor);
        auditedService.AddUser(user);
        auditedService.AssignRole(2, UserRoles.Admin);

        auditedService.RemoveRole(2, UserRoles.Admin, performedByEmail: "admin@copa.com");

        User updated = auditedService.GetUserById(2);
        Assert.IsFalse(updated.HasRole(UserRoles.Admin));
        Assert.IsTrue(updated.HasRole(UserRoles.Editor));
        AuditLog log = auditRepo.GetAll().Single(l => l.Action == "REMOVE_ROLES");
        Assert.AreEqual("admin@copa.com", log.PerformedByEmail);
    }

    [TestMethod]
    public void ToggleRole_SystemAdmin_Throws()
    {
        Assert.ThrowsException<UserServicesException>(() =>
            service.ToggleRole(1, UserRoles.Admin));
    }

    [TestMethod]
    public void ToggleRole_WhenUserLacksRole_AddsItAndLogsAssign()
    {
        MockAuditLogRepository auditRepo = new MockAuditLogRepository();
        UserServices auditedService = new UserServices(
            new MockUserRepository(),
            AuditLogService.CreateInstance(auditRepo));

        User user = BuildUser(2, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15), UserRoles.Editor);
        auditedService.AddUser(user);

        auditedService.ToggleRole(2, UserRoles.Admin);

        Assert.IsTrue(auditedService.GetUserById(2).HasRole(UserRoles.Admin));
        Assert.IsTrue(auditRepo.GetAll().Any(l => l.Action == "ASSIGN_USER_ROLE"));
    }

    [TestMethod]
    public void ToggleRole_WhenUserHasRole_RemovesItAndLogsRemove()
    {
        MockAuditLogRepository auditRepo = new MockAuditLogRepository();
        UserServices auditedService = new UserServices(
            new MockUserRepository(),
            AuditLogService.CreateInstance(auditRepo));

        User user = BuildUser(2, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15), UserRoles.Editor);
        auditedService.AddUser(user);
        auditedService.AssignRole(2, UserRoles.Admin);

        auditedService.ToggleRole(2, UserRoles.Admin);

        Assert.IsFalse(auditedService.GetUserById(2).HasRole(UserRoles.Admin));
        Assert.IsTrue(auditRepo.GetAll().Any(l => l.Action == "REMOVE_ROLES"));
    }

    [TestMethod]
    public void ToggleRole_WhenUserHasOnlyThatRole_Throws()
    {
        User user = BuildUser(2, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15), UserRoles.Editor);
        service.AddUser(user);

        Assert.ThrowsException<UserServicesException>(() =>
            service.ToggleRole(2, UserRoles.Editor));
    }
}
