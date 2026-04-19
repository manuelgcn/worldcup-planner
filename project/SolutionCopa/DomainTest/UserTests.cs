
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
        User usuario = new User(5,"Juan","Perez","Juan@gmail.com","pass123",new DateTime(1990,5,15));
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
        User usuario = new User(1,"Juan","Perez","Juan@gmail.com","pass123",new DateTime(1990,5,15));
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
        User usuario = new User(1,"Juan","Perez","Juan@gmail.com","pass123",new DateTime(1990,5,15));
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
        User usuario = new User(1,"Juan","Perez","Juan@gmail.com","pass123",new DateTime(1990,5,15));
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
        User usuario = new User(1,"Juan","Perez","Juan@gmail.com","pass123",new DateTime(1990,5,15));
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
        User usuario = new User(1,"Juan","Perez","Juan@gmail.com","pass123",new DateTime(1990,5,15));
        Assert.AreEqual("Juan", usuario.name);
    }
    
    [TestMethod]
    public void UserCanBeInitializedWithNameAndLastname()
    {
        User usuario = new User(1,"Juan","Perez","Juan@gmail.com","pass123",new DateTime(1990,5,15));
        Assert.AreEqual("Juan", usuario.name);
        Assert.AreEqual("Perez", usuario.lastname);
    }
    


    [TestMethod]
    public void UserCanBeInitializedWithAllData()
    {
        var birthDate = new DateTime(1990, 5, 15);
        User usuario = new User(1,"Juan","Perez","Juan@gmail.com","pass123",birthDate);
        Assert.AreEqual("Juan", usuario.name);
        Assert.AreEqual("Perez", usuario.lastname);
        Assert.AreEqual("Juan@gmail.com", usuario.email);
        Assert.AreEqual("pass123", usuario.password);
        Assert.AreEqual(birthDate, usuario.dateOfBirth);

    }

    [TestMethod]
    public void UserEmailNotNulls()
    {
        User usuario = new User(1,"Juan","Perez",string.Empty,"pass123",new DateTime(1990,5,15));
        Assert.IsNotNull(usuario.email);
    }
    
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void UserCannotHaveEmptyEmail()
    {
        new User(1, "Juan", "Perez", "", "Pass123@", DateTime.Now);
    }
    
    
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void UserCannotHaveInvalidEmailFormat()
    {
        new User(1, "Juan", "Perez", "mailinvalido", "Pass123@", DateTime.Now);
    }


    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void PasswordMustHaveAtLeast8Characters()
    {
        new User(1, "Juan", "Perez", "juan@mail.com", "Pa1@", DateTime.Now);
    }

    
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void PasswordMustHaveUppercaseLetter()
    {
        new User(1, "Juan", "Perez", "juan@mail.com", "pass123@", DateTime.Now);
    }

    
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void PasswordMustHaveNumber()
    {
        new User(1, "Juan", "Perez", "juan@mail.com", "Passsss@", DateTime.Now);
    }

    
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void PasswordMustHaveSpecialCharacter()
    {
        new User(1, "Juan", "Perez", "juan@mail.com", "Pass1234", DateTime.Now);
    }

    
    [TestMethod]
    public void PasswordIsNotStoredInPlainText()
    {
        string originalPassword = "Pass123@";

        User usuario = new User(1, "Juan", "Perez", "juan@mail.com", originalPassword, new DateTime(1990, 5, 15));

        Assert.AreNotEqual(originalPassword, usuario.password);
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

        Assert.IsTrue(usuario.Roles.Contains(UserRoles.Admin));
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

    
}
