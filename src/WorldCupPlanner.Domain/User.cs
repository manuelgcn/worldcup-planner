using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text;

namespace WorldCupPlanner.Domain;

public class User
{
    private const string EmailRequiredMessage = "Email es obligatorio";
    private const string EmailInvalidMessage = "Email invalido";
    private const string PasswordTooShortMessage = "La contrasena debe tener al menos 8 caracteres";
    private const string PasswordMissingUppercaseMessage = "La contrasena debe contener al menos una mayuscula";
    private const string PasswordMissingLowercaseMessage = "La contrasena debe contener al menos una minuscula";
    private const string PasswordMissingDigitMessage = "La contrasena debe contener al menos un numero";
    private const string PasswordMissingSpecialCharMessage = "La contrasena debe contener al menos un caracter especial";
    private const string FutureDateOfBirthMessage = "La fecha de nacimiento no puede ser futura";
    private const string UnderageUserMessage = "El usuario debe tener al menos 18 anos";
    private const string PasswordMismatchMessage = "La contrasena no coincide";
    private const int PasswordSaltSize = 16;
    private const int PasswordHashSize = 32;
    private const int PasswordHashIterations = 100_000;
    private const char PasswordSaltSeparator = ':';
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
        this.password = HashPassword(password);
        this.dateOfBirth = dateOfBirth;
    }

   
    private void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException(EmailRequiredMessage);

        if (HasWhitespace(email) || !HasExactlyOneAt(email))
            throw new ArgumentException(EmailInvalidMessage);

        string localPart = GetLocalPart(email);
        string domainPart = GetDomainPart(email);

        if (IsInvalidLocalPart(localPart) || IsInvalidDomainPart(domainPart))
            throw new ArgumentException(EmailInvalidMessage);
    }

    private static bool HasWhitespace(string email)
    {
        return email.Any(char.IsWhiteSpace);
    }

    private static bool HasExactlyOneAt(string email)
    {
        return email.Count(c => c == '@') == 1;
    }

    private static string GetLocalPart(string email)
    {
        int atIndex = email.IndexOf('@');
        return email[..atIndex];
    }

    private static string GetDomainPart(string email)
    {
        int atIndex = email.IndexOf('@');
        return email[(atIndex + 1)..];
    }

    private static bool IsInvalidLocalPart(string localPart)
    {
        return string.IsNullOrWhiteSpace(localPart)
               || localPart.StartsWith('.')
               || localPart.EndsWith('.')
               || localPart.Contains("..");
    }

    private static bool IsInvalidDomainPart(string domainPart)
    {
        if (string.IsNullOrWhiteSpace(domainPart))
            return true;

        if (!domainPart.Contains('.'))
            return true;

        if (domainPart.StartsWith('.') || domainPart.EndsWith('.'))
            return true;

        if (domainPart.Contains(".."))
            return true;

        string[] labels = domainPart.Split('.');
        return labels.Any(string.IsNullOrWhiteSpace);
    }

    private void ValidatePassword(string password)
    {
        if (password == null || password.Length < 8)
            throw new ArgumentException(PasswordTooShortMessage);

        if (!Regex.IsMatch(password, "[A-Z]"))
            throw new ArgumentException(PasswordMissingUppercaseMessage);

        if (!Regex.IsMatch(password, "[a-z]"))
            throw new ArgumentException(PasswordMissingLowercaseMessage);

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


    
    // Se guarda "sal:hash" en base64. La sal es aleatoria a proposito (no es un sorteo): dos
    // usuarios con la misma contrasena no comparten hash y no sirven tablas precalculadas.
    private static string HashPassword(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(PasswordSaltSize);
        return Convert.ToBase64String(salt) + PasswordSaltSeparator + Convert.ToBase64String(DeriveHash(password, salt));
    }

    private static byte[] DeriveHash(string password, byte[] salt)
    {
        return Rfc2898DeriveBytes.Pbkdf2(password, salt, PasswordHashIterations, HashAlgorithmName.SHA256, PasswordHashSize);
    }

    // Antes se guardaba un SHA-256 sin sal (nunca contiene ':'). Se sigue aceptando para no dejar
    // afuera a las bases existentes; UserServices lo reemplaza por PBKDF2 en el primer login.
    public bool HasLegacyPasswordHash => !password.Contains(PasswordSaltSeparator);

    public void UpgradeLegacyPasswordHash(string plainTextPassword)
    {
        if (!HasLegacyPasswordHash)
            return;

        if (!LegacyPasswordMatches(plainTextPassword))
            throw new ArgumentException(PasswordMismatchMessage);

        password = HashPassword(plainTextPassword);
    }

    public bool PasswordMatches(string plainTextPassword)
    {
        if (HasLegacyPasswordHash)
            return LegacyPasswordMatches(plainTextPassword);

        string[] parts = password.Split(PasswordSaltSeparator);
        if (parts.Length != 2)
            return false;

        byte[] salt = Convert.FromBase64String(parts[0]);
        byte[] expectedHash = Convert.FromBase64String(parts[1]);
        return CryptographicOperations.FixedTimeEquals(expectedHash, DeriveHash(plainTextPassword, salt));
    }

    private bool LegacyPasswordMatches(string plainTextPassword)
    {
        string legacyHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(plainTextPassword)));
        return CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(password), Encoding.UTF8.GetBytes(legacyHash));
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
