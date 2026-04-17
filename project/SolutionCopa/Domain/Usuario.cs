namespace ProjectCopa;

public class Usuario
{
    public int idUser { get; set; }
    public string name { get; set; }
    public string? lastname { get; set; }
    public string? email { get; set; }
    public string? password { get; set; }
    public DateTime? dateOfBirth { get; set; } 
    
    public Usuario()
    {
        this.name = "";
        this.lastname = "";
        this.email = "";
        this.password = "";
        
    }
    
    public Usuario(int id, string name)
    {
        this.idUser = id;
        this.name = name;
    }

    public Usuario(string name, string lastname)
    {
        this.name = name;
        this.lastname = lastname;
    }
    
    public Usuario(string name, string lastname, string email, string password, DateTime dateOfBirth)
    {
        this.name = name;
        this.lastname = lastname;
        this.email = email;
        this.password = password;
        this.dateOfBirth = dateOfBirth;
    }
    
    public Usuario(int id, string name, string lastname, string email, string password, DateTime dateOfBirth)
    {
        this.idUser = id;
        this.name = name;
        this.lastname = lastname;
        this.email = email;
        this.password = password;
        this.dateOfBirth = dateOfBirth;
    }
}
