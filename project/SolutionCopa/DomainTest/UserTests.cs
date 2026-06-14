
using ProjectCopa;

namespace DomainTest;

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

    
    [TestMethod]
    public void PasswordIsNotStoredInPlainText()
    {
        string originalPassword = "Pass123@";

        User usuario = new User(1, "Juan", "Perez", "juan@mail.com", originalPassword, new DateTime(1990, 5, 15));

        Assert.AreNotEqual(originalPassword, usuario.password);
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
    
}
