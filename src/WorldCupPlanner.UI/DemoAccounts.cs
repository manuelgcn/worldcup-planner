using WorldCupPlanner.Domain;

namespace WorldCupPlanner.UI;

// Lo que Program revisa al arrancar con Demo:Enabled en false, cuando no crea las cuentas de demo.
public static class DemoAccounts
{
    private const string NoAdminMessage =
        "Demo:Enabled is false and no user has the Admin role, so nobody could administer the app. " +
        "Start once with Demo:Enabled=true, or load the demo with WorldCupPlanner.DataSeeder, to create an Admin.";

    // Los mismos emails que Program crea y /login muestra.
    private static readonly string[] Emails = { "admin@copa.com", "editor@copa.com", "periodista@copa.com" };

    // Solo un Admin puede crear otro, y en una base vacía el primer registro público tomaría el id 1, que
    // UserServices protege: sin ningún Admin no se arranca. Apagar la opción tampoco borra las cuentas de
    // demo de un arranque anterior, que siguen entrando con la contraseña publicada.
    public static void VerifyDisabled(IEnumerable<User> users, ILogger logger)
    {
        List<User> existing = users.ToList();

        if (!existing.Any(user => user.HasRole(UserRoles.Admin)))
        {
            throw new InvalidOperationException(NoAdminMessage);
        }

        foreach (User user in existing.Where(user => Emails.Contains(user.email, StringComparer.OrdinalIgnoreCase)))
        {
            logger.LogWarning("Demo:Enabled is false but the demo account {Email} still exists: turning the setting off does not remove it.", user.email);
        }
    }
}
