namespace ProjectCopa;

public class Usuario
{
    public int idUser { get; set; }
    public string name { get; set; }
    public string lastname { get; set; }
    public string email { get; set; }
    public string password { get; set; }
    public DateTime dateOfBirth { get; set; } 
    
    public Usuario()
    {
        this.name = "";
        this.lastname = "";
        this.email = "";
        this.password = "";
        
    }
    
    public Usuario(string name)
    {
        this.name = name;
        this.lastname = "";
        this.email = "";
        this.password = "";
    }
}
