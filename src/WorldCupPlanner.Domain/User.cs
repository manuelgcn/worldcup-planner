using System.Globalization;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text;

namespace WorldCupPlanner.Domain;

public class User
{
    private const string EmailRequiredMessage = "Email es obligatorio";
    private const string EmailInvalidMessage = "Email invalido";
    private const string PasswordTooShortMessage = "La contrasena debe tener al menos 8 caracteres";
    private const string PasswordTooLongMessage = "La contrasena no puede superar los 128 caracteres";
    private const string PasswordMissingUppercaseMessage = "La contrasena debe contener al menos una mayuscula";
    private const string PasswordMissingLowercaseMessage = "La contrasena debe contener al menos una minuscula";
    private const string PasswordMissingDigitMessage = "La contrasena debe contener al menos un numero";
    private const string PasswordMissingSpecialCharMessage = "La contrasena debe contener al menos un caracter especial";
    private const string FutureDateOfBirthMessage = "La fecha de nacimiento no puede ser futura";
    private const string UnderageUserMessage = "El usuario debe tener al menos 18 anos";
    private const string PasswordMismatchMessage = "La contrasena no coincide";
    private const string NameRequiredMessage = "El nombre es obligatorio";
    private const string NameTooLongMessage = "El nombre no puede superar los 100 caracteres";
    private const string LastnameTooLongMessage = "El apellido no puede superar los 100 caracteres";
    private const string EmailTooLongMessage = "El email no puede superar los 254 caracteres";
    // Mismos límites que UserConfiguration: validarlos acá evita que fallen recién al guardar.
    private const int NameMaxLength = 100;
    private const int LastnameMaxLength = 100;
    private const int EmailMaxLength = 254;
    // Acota lo que una contrasena nueva le da de trabajo a las regex y a PBKDF2.
    private const int PasswordMaxLength = 128;
    private const int PasswordSaltSize = 16;
    private const int PasswordHashSize = 32;
    private const int PasswordHashIterations = 600_000;
    // Los "sal:hash" de antes de guardar la version se derivaron con esta cantidad.
    private const int UnversionedHashIterations = 100_000;
    // Tope al leer un hash guardado: con una cantidad disparatada el login quedaria derivando durante minutos.
    private const int MaxPasswordHashIterations = 10_000_000;
    private const string PasswordHashVersion = "v2";
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
        ValidateNames(name, lastname);
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

   
    private static void ValidateNames(string name, string? lastname)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(NameRequiredMessage);

        if (name.Length > NameMaxLength)
            throw new ArgumentException(NameTooLongMessage);

        if (lastname?.Length > LastnameMaxLength)
            throw new ArgumentException(LastnameTooLongMessage);
    }

    private void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException(EmailRequiredMessage);

        if (email.Length > EmailMaxLength)
            throw new ArgumentException(EmailTooLongMessage);

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

        if (password.Length > PasswordMaxLength)
            throw new ArgumentException(PasswordTooLongMessage);

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


    
    // Se guarda "v2:iteraciones:sal:hash", con la sal y el hash en base64; las iteraciones van en el valor
    // para poder subirlas sin invalidar los hashes ya guardados. La sal es aleatoria a proposito (no es un
    // sorteo): dos usuarios con la misma contrasena no comparten hash y no sirven tablas precalculadas.
    private static string HashPassword(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(PasswordSaltSize);
        return FormatHash(salt, DeriveHash(password, salt, PasswordHashIterations));
    }

    private static string FormatHash(byte[] salt, byte[] hash)
    {
        return string.Join(PasswordSaltSeparator, PasswordHashVersion, PasswordHashIterations, Convert.ToBase64String(salt), Convert.ToBase64String(hash));
    }

    private static byte[] DeriveHash(string password, byte[] salt, int iterations)
    {
        return Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, PasswordHashSize);
    }

    // Legado es todo hash mas debil que el actual: el SHA-256 sin sal de la primera version (nunca contiene ':'),
    // el "sal:hash" de 100.000 iteraciones y un v2 con menos iteraciones que las de hoy. Se siguen aceptando para
    // no dejar afuera a las bases existentes; UserServices los reemplaza en el siguiente login correcto.
    public bool HasLegacyPasswordHash =>
        !TryReadSaltedHash(out int iterations, out _, out _) || iterations < PasswordHashIterations;

    public void UpgradeLegacyPasswordHash(string plainTextPassword)
    {
        if (!HasLegacyPasswordHash)
            return;

        if (!PasswordMatches(plainTextPassword))
            throw new ArgumentException(PasswordMismatchMessage);

        password = HashPassword(plainTextPassword);
    }

    public void ChangePassword(string currentPassword, string newPassword)
    {
        if (!PasswordMatches(currentPassword))
            throw new ArgumentException(PasswordMismatchMessage);

        password = HashPassword(newPassword);
    }

    public bool PasswordMatches(string plainTextPassword)
    {
        if (!password.Contains(PasswordSaltSeparator))
            return UnsaltedPasswordMatches(plainTextPassword);

        return TryReadSaltedHash(out int iterations, out byte[] salt, out byte[] expectedHash)
               && CryptographicOperations.FixedTimeEquals(expectedHash, DeriveHash(plainTextPassword, salt, iterations));
    }

    // Lee "v2:iteraciones:sal:hash" o el "sal:hash" anterior. Un valor que no se puede leer no es de nadie:
    // devuelve false en vez de lanzar, para que el login responda credenciales invalidas.
    private bool TryReadSaltedHash(out int iterations, out byte[] salt, out byte[] hash)
    {
        iterations = UnversionedHashIterations;
        salt = new byte[PasswordSaltSize];
        hash = new byte[PasswordHashSize];
        string[] parts = password.Split(PasswordSaltSeparator);

        if (parts.Length == 4)
        {
            if (parts[0] != PasswordHashVersion
                || !int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out iterations)
                || iterations < 1
                || iterations > MaxPasswordHashIterations)
                return false;
        }
        else if (parts.Length != 2)
        {
            return false;
        }

        return TryDecodeExactly(parts[^2], salt) && TryDecodeExactly(parts[^1], hash);
    }

    // Exige el largo exacto: con un hash vacio o mas corto la comparacion no puede darle la razon a nadie.
    private static bool TryDecodeExactly(string base64, byte[] bytes)
    {
        return Convert.TryFromBase64String(base64, bytes, out int written) && written == bytes.Length;
    }

    // Cuenta que no existe: un hash con el formato y las iteraciones de hoy, en cero, que ninguna contrasena produce.
    private static readonly User NoAccount = new User { password = FormatHash(new byte[PasswordSaltSize], new byte[PasswordHashSize]) };

    // Cuesta lo mismo que verificar la contrasena de una cuenta real y nunca coincide: es lo que el login
    // verifica cuando el email no existe.
    public static bool PasswordMatchesNoAccount(string plainTextPassword)
    {
        return NoAccount.PasswordMatches(plainTextPassword);
    }

    private bool UnsaltedPasswordMatches(string plainTextPassword)
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
        ValidateNames(name, lastname);
        ValidateEmail(email);

        this.name = name;
        this.lastname = lastname;
        this.email = email;
    }
}
