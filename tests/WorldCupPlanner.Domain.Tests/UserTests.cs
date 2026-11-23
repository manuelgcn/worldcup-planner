
using System.Security.Cryptography;
using System.Text;
using WorldCupPlanner.Domain;

namespace WorldCupPlanner.Domain.Tests;

[TestClass]
public class UserTests
{
    private User usuario;

    [TestInitialize]
    public void Setup()
    {
        usuario = new User();
    }
    
    [TestMethod]
    public void UserNotNull()
    {
        Assert.IsNotNull(usuario);
    }
    
    [TestMethod]
    public void UserCanGetIdUser()
    {
        User usuario = new User(5,"Juan","Perez","Juan@gmail.com","Pass1234@",new DateTime(1990,5,15));
        Assert.AreEqual(5, usuario.idUser);
    }
    
    
    [TestMethod]
    public void UserCanSetIdUser()
    {
        usuario.idUser = 5;
        Assert.AreEqual(5, usuario.idUser);
    }

    [TestMethod]
    public void UserCanGetNameUser()
    {
        User usuario = new User(1,"Juan","Perez","Juan@gmail.com","Pass1234@",new DateTime(1990,5,15));
        Assert.AreEqual("Juan", usuario.name);
    }

    [TestMethod]
    public void UserCanSetNameUser()
    {
        usuario.name = "Juan";
        Assert.AreEqual("Juan", usuario.name);
    }

    [TestMethod]
    public void UserCanGetLastNameUser()
    {
        User usuario = new User(1,"Juan","Perez","Juan@gmail.com","Pass1234@",new DateTime(1990,5,15));
        Assert.AreEqual("Perez", usuario.lastname);
    }

    [TestMethod]
    public void UserCanSetLastNameUser()
    {
        usuario.lastname = "Perez";
        Assert.AreEqual("Perez", usuario.lastname);
    }

    [TestMethod]
    public void UserCanGetEmailUser()
    {
        User usuario = new User(1,"Juan","Perez","Juan@gmail.com","Pass1234@",new DateTime(1990,5,15));
        Assert.AreEqual("Juan@gmail.com", usuario.email);
    }

    [TestMethod]
    public void UserCanSetEmailUser()
    {
        usuario.email = "Juan@gmail.com";
        Assert.AreEqual("Juan@gmail.com", usuario.email);
    }
    
    [TestMethod]
    public void UserCanGetDateOfBirth()
    {
        User usuario = new User(1,"Juan","Perez","Juan@gmail.com","Pass1234@",new DateTime(1990,5,15));
        Assert.AreEqual(new DateTime(1990,5,15), usuario.dateOfBirth);
    }

    [TestMethod]
    public void UserCanSetDateOfBirth()
    {
        usuario.dateOfBirth = new DateTime(1990,5,15);
        Assert.AreEqual(new DateTime(1990,5,15), usuario.dateOfBirth);
    }
    
    [TestMethod]
    public void UserCanBeInitializedWithName()
    {
        User usuario = new User(1,"Juan","Perez","Juan@gmail.com","Pass1234@",new DateTime(1990,5,15));
        Assert.AreEqual("Juan", usuario.name);
    }
    
    [TestMethod]
    public void UserCanBeInitializedWithNameAndLastname()
    {
        User usuario = new User(1,"Juan","Perez","Juan@gmail.com","Pass1234@",new DateTime(1990,5,15));
        Assert.AreEqual("Juan", usuario.name);
        Assert.AreEqual("Perez", usuario.lastname);
    }
    


    [TestMethod]
    public void UserCanBeInitializedWithAllData()
    {
        DateTime birthDate = new DateTime(1990, 5, 15);
        User usuario = new User(1,"Juan","Perez","Juan@gmail.com","Pass1234@",birthDate);
        Assert.AreEqual("Juan", usuario.name);
        Assert.AreEqual("Perez", usuario.lastname);
        Assert.AreEqual("Juan@gmail.com", usuario.email);
        Assert.AreEqual(birthDate, usuario.dateOfBirth);

    }
    
    [TestMethod]
    public void UserCannotHaveEmptyEmail()
    {
        Assert.ThrowsException<ArgumentException>(() =>
            new User(1, "Juan", "Perez", "", "Pass1234@", DateTime.Now));
    }
    
    
    [TestMethod]
    public void UserCannotHaveInvalidEmailFormat()
    {
        Assert.ThrowsException<ArgumentException>(() =>
            new User(1, "Juan", "Perez", "mailinvalido", "Pass1234@", DateTime.Now));
    }

    [DataTestMethod]
    [DataRow("a@b.")]
    [DataRow("@.")]
    [DataRow("a@b")]
    [DataRow("a@.com")]
    [DataRow("nombre apellido@mail.com")]
    [DataRow("juan@@mail.com")]
    public void UserRejectsMalformedEmail(string email)
    {
        Assert.ThrowsException<ArgumentException>(() =>
            new User(1, "Juan", "Perez", email, "Pass1234@", new DateTime(1990, 5, 15)));
    }

    [DataTestMethod]
    [DataRow("juan@mail.com")]
    [DataRow("juan.perez@sub.dominio.uy")]
    [DataRow("user+tag@mail.co")]
    public void UserAcceptsWellFormedEmail(string email)
    {
        User user = new User(1, "Juan", "Perez", email, "Pass1234@", new DateTime(1990, 5, 15));

        Assert.AreEqual(email, user.email);
    }


    [TestMethod]
    public void PasswordMustHaveAtLeast8Characters()
    {
        Assert.ThrowsException<ArgumentException>(() =>
            new User(1, "Juan", "Perez", "juan@mail.com", "Pa1@", DateTime.Now));
    }

    
    [TestMethod]
    public void PasswordMustHaveUppercaseLetter()
    {
        Assert.ThrowsException<ArgumentException>(() =>
            new User(1, "Juan", "Perez", "juan@mail.com", "pass123@", DateTime.Now));
    }

    
    [TestMethod]
    public void PasswordMustHaveLowercaseLetter()
    {
        Assert.ThrowsException<ArgumentException>(() =>
            new User(1, "Juan", "Perez", "juan@mail.com", "PASS123@", new DateTime(1990, 5, 15)));
    }

    
    [TestMethod]
    public void PasswordMustHaveNumber()
    {
        Assert.ThrowsException<ArgumentException>(() =>
            new User(1, "Juan", "Perez", "juan@mail.com", "Passsss@", DateTime.Now));
    }

    
    [TestMethod]
    public void PasswordMustHaveSpecialCharacter()
    {
        Assert.ThrowsException<ArgumentException>(() =>
            new User(1, "Juan", "Perez", "juan@mail.com", "Pass1234", DateTime.Now));
    }

    
    // Sin tope, una contrasena enorme iria entera a PBKDF2.
    [TestMethod]
    public void PasswordCannotHaveMoreThan128Characters()
    {
        string tooLong = "Aa1@" + new string('a', 125);

        ArgumentException error = Assert.ThrowsException<ArgumentException>(() =>
            new User(1, "Juan", "Perez", "juan@mail.com", tooLong, new DateTime(1990, 5, 15)));

        StringAssert.Contains(error.Message, "128");
    }

    [TestMethod]
    public void PasswordCanHaveExactly128Characters()
    {
        string longest = "Aa1@" + new string('a', 124);

        User user = new User(1, "Juan", "Perez", "juan@mail.com", longest, new DateTime(1990, 5, 15));

        Assert.IsTrue(user.PasswordMatches(longest));
    }

    [TestMethod]
    public void PasswordIsNotStoredInPlainText()
    {
        string originalPassword = "Pass123@";

        User usuario = new User(1, "Juan", "Perez", "juan@mail.com", originalPassword, new DateTime(1990, 5, 15));

        Assert.AreNotEqual(originalPassword, usuario.password);
    }

    [TestMethod]
    public void SamePasswordIsStoredDifferentlyForEachUser()
    {
        User first = new User(1, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15));
        User second = new User(2, "Ana", "Gomez", "ana@mail.com", "Pass123@", new DateTime(1990, 5, 15));

        Assert.AreNotEqual(first.password, second.password);
    }

    [TestMethod]
    public void PasswordMatchesAcceptsTheLegacyUnsaltedSha256Hash()
    {
        User user = WithLegacyHash(new User(1, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15)), "Pass123@");

        Assert.IsTrue(user.HasLegacyPasswordHash);
        Assert.IsTrue(user.PasswordMatches("Pass123@"));
        Assert.IsFalse(user.PasswordMatches("Other123@"));
    }

    [TestMethod]
    public void UpgradeLegacyPasswordHashStoresASaltedHashForTheSamePassword()
    {
        User user = WithLegacyHash(new User(1, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15)), "Pass123@");

        user.UpgradeLegacyPasswordHash("Pass123@");

        Assert.IsFalse(user.HasLegacyPasswordHash);
        Assert.IsTrue(user.PasswordMatches("Pass123@"));
    }

    [TestMethod]
    public void UpgradeLegacyPasswordHashRejectsAWrongPassword()
    {
        User user = WithLegacyHash(new User(1, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15)), "Pass123@");

        Assert.ThrowsException<ArgumentException>(() => user.UpgradeLegacyPasswordHash("Other123@"));
        Assert.IsTrue(user.HasLegacyPasswordHash);
    }

    [TestMethod]
    public void PasswordMatchesReturnsTrueForCorrectPassword()
    {
        User usuario = new User(1, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15));

        Assert.IsTrue(usuario.PasswordMatches("Pass123@"));
    }

    [TestMethod]
    public void PasswordMatchesReturnsFalseForWrongPassword()
    {
        User usuario = new User(1, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15));

        Assert.IsFalse(usuario.PasswordMatches("Other123@"));
    }

    [TestMethod]
    public void PasswordIsStoredAsAVersionedHashWith600000Iterations()
    {
        User user = new User(1, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15));

        string[] parts = user.password.Split(':');
        Assert.AreEqual(4, parts.Length);
        Assert.AreEqual("v2", parts[0]);
        Assert.AreEqual("600000", parts[1]);
        Assert.AreEqual(ZeroSalt.Length, parts[2].Length);
        Assert.AreEqual(ZeroHash.Length, parts[3].Length);
    }

    [TestMethod]
    public void PasswordMatchesUsesTheIterationCountStoredWithTheHash()
    {
        byte[] salt = Convert.FromBase64String(ZeroSalt);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2("Pass123@", salt, 1000, HashAlgorithmName.SHA256, 32);
        User user = WithStoredPassword(new User(), $"v2:1000:{ZeroSalt}:{Convert.ToBase64String(hash)}");

        Assert.IsTrue(user.PasswordMatches("Pass123@"));
        Assert.IsFalse(user.PasswordMatches("Other123@"));
    }

    // Los "sal:hash" (PBKDF2, 100.000 iteraciones) de db/scripts/seed-full.sql, con las contrasenas del README.
    [DataTestMethod]
    [DataRow(SeedScriptPassword, SeedScriptHash)]
    [DataRow("Admin@456", "Iaz3OUXq6gNg9ouIZreHaw==:PujOzyetnEYZf7SDY/Jqb2xfTI3+aFBGuDKXyM2X8dg=")]
    [DataRow("Editor@123", "og79LuOs2AVfjMwwLZ5eDQ==:Wjw4utF31I4jBYRFYKB3kLYaL/u24n7YCduRyqJG9gY=")]
    [DataRow("Editor@456", "Ysso6hoqbX/CmssIkLk/4A==:5+ZSsjQQSduSoE+SgCSRcSaY5W7g2ZPDX1bJnLD1iRA=")]
    [DataRow("Prensa@123", "lfFxKRmCcvTS085MJYUNmQ==:MuMoDfBiRkE4VI6ZLp5QA7baQC+AdhEciP2F+C5T38w=")]
    [DataRow("Prensa@456", "L0TnkMYvZA4rWs0Bde7htQ==:e23TJPpzaq7I27ZlX6MHGyxKPztJEFe74IrB4IcT4j4=")]
    public void PasswordMatchesAcceptsTheSaltedHashesOfTheSeedScript(string plainTextPassword, string stored)
    {
        User user = WithStoredPassword(new User(), stored);

        Assert.IsTrue(user.PasswordMatches(plainTextPassword));
        Assert.IsFalse(user.PasswordMatches("Other123@"));
    }

    // Legado es todo hash mas debil que el actual, no solo el SHA-256 sin sal.
    [TestMethod]
    public void UpgradeLegacyPasswordHashReplacesAnUnversionedSaltedHash()
    {
        User user = WithStoredPassword(new User(), SeedScriptHash);
        Assert.IsTrue(user.HasLegacyPasswordHash);

        user.UpgradeLegacyPasswordHash(SeedScriptPassword);

        Assert.IsFalse(user.HasLegacyPasswordHash);
        StringAssert.StartsWith(user.password, "v2:600000:");
        Assert.IsTrue(user.PasswordMatches(SeedScriptPassword));
    }

    [TestMethod]
    public void UpgradeLegacyPasswordHashReplacesAVersionedHashWithFewerIterations()
    {
        byte[] salt = Convert.FromBase64String(ZeroSalt);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2("Pass123@", salt, 1000, HashAlgorithmName.SHA256, 32);
        User user = WithStoredPassword(new User(), $"v2:1000:{ZeroSalt}:{Convert.ToBase64String(hash)}");
        Assert.IsTrue(user.HasLegacyPasswordHash);

        user.UpgradeLegacyPasswordHash("Pass123@");

        StringAssert.StartsWith(user.password, "v2:600000:");
        Assert.IsTrue(user.PasswordMatches("Pass123@"));
    }

    [TestMethod]
    public void UpgradeLegacyPasswordHashRejectsAWrongPasswordForASaltedHash()
    {
        User user = WithStoredPassword(new User(), SeedScriptHash);

        Assert.ThrowsException<ArgumentException>(() => user.UpgradeLegacyPasswordHash("Other123@"));
        Assert.AreEqual(SeedScriptHash, user.password);
    }

    [TestMethod]
    public void UpgradeLegacyPasswordHashLeavesACurrentHashAsItIs()
    {
        User user = new User(1, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15));
        string stored = user.password;
        Assert.IsFalse(user.HasLegacyPasswordHash);

        user.UpgradeLegacyPasswordHash("Pass123@");

        Assert.AreEqual(stored, user.password);
    }

    // Mas iteraciones que las actuales no es mas debil: no hay nada que mejorar.
    [TestMethod]
    public void AHashWithMoreIterationsThanTheCurrentOnesIsNotLegacy()
    {
        User user = WithStoredPassword(new User(), $"v2:700000:{ZeroSalt}:{ZeroHash}");

        Assert.IsFalse(user.HasLegacyPasswordHash);
    }

    // Lo que el login verifica cuando el email no existe: tiene que fallar siempre, sea cual sea la contrasena.
    [DataTestMethod]
    [DataRow("Pass123@")]
    [DataRow("")]
    public void NoPasswordMatchesTheAccountThatDoesNotExist(string plainTextPassword)
    {
        Assert.IsFalse(User.PasswordMatchesNoAccount(plainTextPassword));
    }

    // Un valor guardado que no se puede leer nunca coincide, y tampoco rompe el login con una excepcion.
    [DataTestMethod]
    [DataRow(":")]
    [DataRow("no-es-base64:no-es-base64")]
    [DataRow(ZeroSalt + ":")]
    [DataRow("v2:600000::")]
    [DataRow("v2:600000:" + ZeroSalt + ":")]
    [DataRow("v2:600000:" + ZeroSalt)]
    [DataRow("v2:600000:" + ZeroSalt + ":" + ZeroHash + ":sobra")]
    [DataRow("v2:600000:no-es-base64:" + ZeroHash)]
    [DataRow("v3:600000:" + ZeroSalt + ":" + ZeroHash)]
    [DataRow("v2::" + ZeroSalt + ":" + ZeroHash)]
    [DataRow("v2:muchas:" + ZeroSalt + ":" + ZeroHash)]
    [DataRow("v2:0:" + ZeroSalt + ":" + ZeroHash)]
    [DataRow("v2:-1:" + ZeroSalt + ":" + ZeroHash)]
    [DataRow("v2:99999999999:" + ZeroSalt + ":" + ZeroHash)]
    public void PasswordMatchesReturnsFalseForAMalformedStoredValue(string stored)
    {
        User user = WithStoredPassword(new User(), stored);

        Assert.IsFalse(user.PasswordMatches("Pass123@"));
    }

    // Con el maximo de un int derivar llevaria minutos: tiene que rechazarse sin llegar a PBKDF2.
    [TestMethod]
    [Timeout(5000)]
    public void PasswordMatchesRejectsAnAbsurdIterationCountWithoutDerivingIt()
    {
        User user = WithStoredPassword(new User(), $"v2:{int.MaxValue}:{ZeroSalt}:{ZeroHash}");

        Assert.IsFalse(user.PasswordMatches("Pass123@"));
    }

    [TestMethod]
    public void ChangePasswordStoresTheNewPasswordAsACurrentHash()
    {
        User user = new User(1, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15));

        user.ChangePassword("Pass123@", "Nueva456#");

        StringAssert.StartsWith(user.password, "v2:600000:");
        Assert.IsTrue(user.PasswordMatches("Nueva456#"));
        Assert.IsFalse(user.PasswordMatches("Pass123@"));
    }

    // Cambiar la contrasena tambien deja atras un hash mas debil que el actual.
    [TestMethod]
    public void ChangePasswordReplacesALegacyHash()
    {
        User user = WithStoredPassword(new User(), SeedScriptHash);

        user.ChangePassword(SeedScriptPassword, "Nueva456#");

        Assert.IsFalse(user.HasLegacyPasswordHash);
        Assert.IsTrue(user.PasswordMatches("Nueva456#"));
    }


    [TestMethod]
    public void UserStartsWithoutRoles()
    {
        
        User usuario = new User(1, "Juan", "Perez", "juan@mail.com", "Passs1234@", new DateTime(1990, 5, 15));
        Assert.AreEqual(0, usuario.Roles.Count);
    }

    
    [TestMethod]
    public void UserCanAddRole()
    {
        User usuario = new User(1, "Juan", "Perez", "juan@mail.com", "Passs1234@", new DateTime(1990, 5, 15));
        usuario.AddRole(UserRoles.Admin);

        Assert.IsTrue(usuario.Roles.Any(r => r.Role == UserRoles.Admin));
    }

    
    [TestMethod]
    public void UserCannotHaveDuplicateRoles()
    {
        User usuario = new User(
            1,
            "Juan",
            "Perez",
            "juan@mail.com",
            "Pass123@",
            new DateTime(1990, 5, 15)
        );

        usuario.AddRole(UserRoles.Admin);
        usuario.AddRole(UserRoles.Admin);

        Assert.AreEqual(1, usuario.Roles.Count);
    }
    
    
    [TestMethod]
    public void CannotCreateUserWithFutureDateOfBirth()
    {
        DateTime futureDate = DateTime.Today.AddDays(1);

        Assert.ThrowsException<ArgumentException>(() =>
            new User(
                1,
                "Juan",
                "Perez",
                "juan@mail.com",
                "Pass123@",
                futureDate
            )
        );
    }

    
    [TestMethod]
    public void CannotCreateUserYoungerThan18()
    {
        DateTime underageDate = DateTime.Today.AddYears(-17);

        Assert.ThrowsException<ArgumentException>(() =>
            new User(
                1,
                "Juan",
                "Perez",
                "juan@mail.com",
                "Pass123@",
                underageDate
            )
        );
    }

    
    [TestMethod]
    public void CanCreateUserWhoIsExactly18YearsOld()
    {
        DateTime exactly18YearsOld = DateTime.Today.AddYears(-18);

        User user = new User(
            1,
            "Juan",
            "Perez",
            "juan@mail.com",
            "Pass123@",
            exactly18YearsOld
        );

        Assert.AreEqual(exactly18YearsOld, user.dateOfBirth);
    }

    
    [TestMethod]
    public void CannotCreateUserWhoIsOneDayYoungerThan18()
    {
        DateTime oneDayYoungerThan18 = DateTime.Today.AddYears(-18).AddDays(1);

        Assert.ThrowsException<ArgumentException>(() =>
            new User(
                1,
                "Juan",
                "Perez",
                "juan@mail.com",
                "Pass123@",
                oneDayYoungerThan18
            )
        );
    }

    [TestMethod]
    public void UserCannotHaveEmailWithoutLocalPart()
    {
        Assert.ThrowsException<ArgumentException>(() =>
            new User(
                1,
                "Juan",
                "Perez",
                "@mail.com",
                "Pass1234@",
                new DateTime(1990, 5, 15)
            )
        );
    }

    [TestMethod]
    public void UserCannotHaveEmailWithSpaces()
    {
        Assert.ThrowsException<ArgumentException>(() =>
            new User(
                1,
                "Juan",
                "Perez",
                "juan @mail.com",
                "Pass1234@",
                new DateTime(1990, 5, 15)
            )
        );
    }
    
    [TestMethod]
    public void UserCannotHaveEmailWithMultipleAtSymbols()
    {
        Assert.ThrowsException<ArgumentException>(() =>
            new User(
                1,
                "Juan",
                "Perez",
                "juan@@mail.com",
                "Pass1234@",
                new DateTime(1990, 5, 15)
            )
        );
    }
    
    
    [TestMethod]
    public void UserCannotHaveEmailWithoutDotInDomain()
    {
        Assert.ThrowsException<ArgumentException>(() =>
            new User(
                1,
                "Juan",
                "Perez",
                "juan@mailcom",
                "Pass1234@",
                new DateTime(1990, 5, 15)
            )
        );
    }

    [TestMethod]
    public void UserCannotHaveEmailWithDomainStartingWithDot()
    {
        Assert.ThrowsException<ArgumentException>(() =>
            new User(
                1,
                "Juan",
                "Perez",
                "juan@.com",
                "Pass1234@",
                new DateTime(1990, 5, 15)
            )
        );
    }
    
    [TestMethod]
    public void UserCannotHaveEmailWithConsecutiveDotsInDomain()
    {
        Assert.ThrowsException<ArgumentException>(() =>
            new User(
                1,
                "Juan",
                "Perez",
                "juan@mail..com",
                "Pass1234@",
                new DateTime(1990, 5, 15)
            )
        );
    }
    
    [TestMethod]
    public void UserCannotHaveEmailWithLocalPartEndingWithDot()
    {
        Assert.ThrowsException<ArgumentException>(() =>
            new User(
                1,
                "Juan",
                "Perez",
                "juan.@mail.com",
                "Pass1234@",
                new DateTime(1990, 5, 15)
            )
        );
    }
    
    [TestMethod]
    public void UserCannotHaveEmailWithLocalPartStartingWithDot()
    {
        Assert.ThrowsException<ArgumentException>(() =>
            new User(
                1,
                "Juan",
                "Perez",
                ".juan@mail.com",
                "Pass1234@",
                new DateTime(1990, 5, 15)
            )
        );
    }

    // Lo que guardaba la version anterior: SHA-256 del texto en base64, sin sal.
    private static User WithLegacyHash(User user, string plainTextPassword)
    {
        string legacyHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(plainTextPassword)));
        return WithStoredPassword(user, legacyHash);
    }

    // admin1@copa.com en db/scripts/seed-full.sql.
    private const string SeedScriptPassword = "Admin@123";
    private const string SeedScriptHash = "NayVITcSJcByRMrVNU74ug==:mbu4770CN/kEqP065+upgXhAC4zzKJ9smvFh25ZH5BU=";

    // Una sal de 16 bytes y un hash de 32, todos en cero, en base64: partes bien formadas para armar valores guardados.
    private const string ZeroSalt = "AAAAAAAAAAAAAAAAAAAAAA==";
    private const string ZeroHash = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=";

    private static User WithStoredPassword(User user, string stored)
    {
        typeof(User).GetProperty(nameof(User.password))!.SetValue(user, stored);
        return user;
    }

    // Límites de UserConfiguration: sin validarlos en el dominio, el error llegaba como DbUpdateException.
    [TestMethod]
    public void User_WithBlankName_Throws()
    {
        Assert.ThrowsException<ArgumentException>(
            () => new User(1, "   ", "Perez", "juan@mail.com", "Pass1234@", new DateTime(1990, 5, 15)));
    }

    [TestMethod]
    public void User_WithNameOver100Chars_Throws()
    {
        Assert.ThrowsException<ArgumentException>(
            () => new User(1, new string('a', 101), "Perez", "juan@mail.com", "Pass1234@", new DateTime(1990, 5, 15)));
    }

    [TestMethod]
    public void User_WithLastnameOver100Chars_Throws()
    {
        Assert.ThrowsException<ArgumentException>(
            () => new User(1, "Juan", new string('a', 101), "juan@mail.com", "Pass1234@", new DateTime(1990, 5, 15)));
    }

    [TestMethod]
    public void User_WithEmailOver254Chars_Throws()
    {
        string longEmail = new string('a', 60) + "@" + string.Concat(Enumerable.Repeat(new string('b', 60) + ".", 4)) + "com";

        Assert.ThrowsException<ArgumentException>(
            () => new User(1, "Juan", "Perez", longEmail, "Pass1234@", new DateTime(1990, 5, 15)));
    }

    [TestMethod]
    public void UpdatePersonalData_WithBlankName_Throws()
    {
        User user = new User(1, "Juan", "Perez", "juan@mail.com", "Pass1234@", new DateTime(1990, 5, 15));

        Assert.ThrowsException<ArgumentException>(() => user.UpdatePersonalData(" ", "Perez", "juan@mail.com"));
    }
}
