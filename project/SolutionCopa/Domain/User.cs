using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text;

namespace ProjectCopa;

public class User
{
    private const string EmailRequiredMessage = "Email es obligatorio";
    private const string EmailInvalidMessage = "Email invalido";
    private const string PasswordTooShortMessage = "La contrasena debe tener al menos 8 caracteres";
    private const string PasswordMissingUppercaseMessage = "La contrasena debe contener al menos una mayuscula";
    private const string PasswordMissingDigitMessage = "La contrasena debe contener al menos un numero";
    private const string PasswordMissingSpecialCharMessage = "La contrasena debe contener al menos un caracter especial";
    private const string FutureDateOfBirthMessage = "La fecha de nacimiento no puede ser futura";
    private const string UnderageUserMessage = "El usuario debe tener al menos 18 anos";
    public int idUser { get; set; }

    public string name { get; set; }
    public string? lastname { get; set; }
    public string email { get; set; }
    public string password { get; private set; }
    public DateTime? dateOfBirth { get; set; }

    public ICollection<UserRole> Roles { get; set; } = new List<UserRole>();

    public User()
    {
        name = "";
        lastname = "";
        email = "";
        password = "";
    }

    public User(int id, string name, string lastname, string email, string password, DateTime dateOfBirth)
    {
        ValidateEmail(email);
        ValidatePassword(password);
        ValidateDateOfBirth(dateOfBirth);

        idUser = id;
        this.name = name;
        this.lastname = lastname;
        this.email = email;
        this.password = EncryptPassword(password);
        this.dateOfBirth = dateOfBirth;
    }

    private void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException(EmailRequiredMessage);

        if (!email.Contains("@") || !email.Contains("."))
            throw new ArgumentException(EmailInvalidMessage);
    }

    private void ValidatePassword(string password)
    {
        if (password == null || password.Length < 8)
            throw new ArgumentException(PasswordTooShortMessage);

        if (!Regex.IsMatch(password, "[A-Z]"))
            throw new ArgumentException(PasswordMissingUppercaseMessage);

        if (!Regex.IsMatch(password, "[0-9]"))
            throw new ArgumentException(PasswordMissingDigitMessage);

        if (!Regex.IsMatch(password, "[^a-zA-Z0-9]"))
            throw new ArgumentException(PasswordMissingSpecialCharMessage);
    }

    
   
    private void ValidateDateOfBirth(DateTime dateOfBirth)
    {
        if (dateOfBirth.Date > DateTime.Today)
            throw new ArgumentException(FutureDateOfBirthMessage);

        int age = DateTime.Today.Year - dateOfBirth.Year;

        if (dateOfBirth.Date > DateTime.Today.AddYears(-age))
            age--;

        if (age < 18)
            throw new ArgumentException(UnderageUserMessage);
    }


    
    private string EncryptPassword(string password)
    {
        using SHA256 sha = SHA256.Create();
        byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

    public bool PasswordMatches(string plainTextPassword)
    {
        return password == EncryptPassword(plainTextPassword);
    }

    public void AddRole(UserRoles role)
    {
        if (!Roles.Any(r => r.Role == role))
        {
            Roles.Add(new UserRole
            {
                Role = role,
                User = this
            });
        }
    }

    public void RemoveRoles(UserRoles role)
    {
        var existing = Roles.FirstOrDefault(r => r.Role == role);

        if (existing != null && Roles.Count > 1)
        {
            Roles.Remove(existing);
        }
    }

    public bool HasRole(UserRoles role)
    {
        return Roles.Any(r => r.Role == role);
    }

    public void UpdatePersonalData(string name, string lastname, string email)
    {
        ValidateEmail(email);

        this.name = name;
        this.lastname = lastname;
        this.email = email;
    }
}
