using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text;


namespace ProjectCopa;


public class User
{
    public int idUser { get; set; }
    public string name { get; set; }
    public string? lastname { get; set; }
    public string email { get; set; }
    public string password { get; private set; }
    public DateTime? dateOfBirth { get; set; } 
    
    private List<UserRoles> roles = new List<UserRoles>();
    public IReadOnlyCollection<UserRoles> Roles => roles.AsReadOnly();
    
    public User()
    {
        this.name = "";
        this.lastname = "";
        this.email = "";
        this.password = "";
        
        
    }
    
    public User(int id, string name, string lastname, string email, string password, DateTime dateOfBirth)
    { 
        ValidateEmail(email);
        ValidatePassword(password);
        
        this.idUser = id;
        this.name = name;
        this.lastname = lastname;
        this.email = email;
        this.password = EncryptPassword(password);
        this.dateOfBirth = dateOfBirth;
    }
    
    
    private void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email es un campo obligatorio");

        if (!email.Contains("@") || !email.Contains("."))
            throw new ArgumentException("Email invalido, debe contener '@' y '.' ");
    }

    
    private void ValidatePassword(string password)
    {
        if (password == null || password.Length < 8)
            throw new ArgumentException("Password must have at least 8 characters");
        
        
        if (!Regex.IsMatch(password, "[A-Z]"))
            throw new ArgumentException("Password must contain an uppercase letter");

        
        if (!Regex.IsMatch(password, "[0-9]"))
            throw new ArgumentException("Password must contain a number");

        
        if (!Regex.IsMatch(password, "[^a-zA-Z0-9]"))
            throw new ArgumentException("Password must contain a special character");

    }
    private string EncryptPassword(string password)
    {
        using SHA256 sha = SHA256.Create();
        byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

    
    public void AddRole(UserRoles role)
    {
        if (!roles.Contains(role))
        {
            roles.Add(role);
        }
    }

    
}
