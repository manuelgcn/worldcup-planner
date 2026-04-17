using System.Reflection.Metadata;
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
    public void UserCanSetPassword()
    {
        usuario.password = "pass123";
        Assert.AreEqual("pass123", usuario.password);
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

    
}
