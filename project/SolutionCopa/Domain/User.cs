namespace ProjectCopa;

public class User
{
    public int idUser { get; set; }
    public string name { get; set; }
    public string? lastname { get; set; }
    public string email { get; set; }
    public string? password { get; set; }
    public DateTime? dateOfBirth { get; set; } 
    
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
        this.password = password;
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
    }

    
    
}
