namespace WorldCupPlanner.UI;

// Las excepciones propias (dominio y servicios) traen mensajes en español pensados para el usuario.
// Las de infraestructura (EF, SQL, IO) pueden mostrar detalles internos: se loguean y se ve uno genérico.
public static class UserFacingError
{
    private const string OwnNamespacePrefix = "WorldCupPlanner";
    private const string UnexpectedErrorMessage = "Ocurrió un error inesperado. Intentá nuevamente.";
    private const string ParameterSuffixFormat = " (Parameter '{0}')";

    public static string MessageFor(Exception exception, ILogger logger)
    {
        bool isOwnException = exception.GetType().Namespace?.StartsWith(OwnNamespacePrefix, StringComparison.Ordinal) == true;

        if (isOwnException)
        {
            return exception.Message;
        }

        if (exception is ArgumentException argumentException && WasThrownByOwnCode(argumentException))
        {
            return WithoutParameterSuffix(argumentException);
        }

        logger.LogError(exception, "Unexpected error replaced by a generic message for the user.");
        return UnexpectedErrorMessage;
    }

    // El dominio valida con ArgumentException y mensajes en español. Las que lanza el framework, aun
    // llamado desde el dominio (ArgumentNullException.ThrowIfNull, Enum.Parse), traen texto en inglés.
    private static bool WasThrownByOwnCode(Exception exception)
    {
        string? throwingAssembly = exception.TargetSite?.DeclaringType?.Assembly.GetName().Name;
        return throwingAssembly?.StartsWith(OwnNamespacePrefix, StringComparison.Ordinal) == true;
    }

    // .NET agrega el nombre del parámetro al mensaje y, si lo hay, el valor recibido en otra línea.
    private static string WithoutParameterSuffix(ArgumentException exception)
    {
        if (exception.ParamName is null)
        {
            return exception.Message;
        }

        int suffixStart = exception.Message.LastIndexOf(
            string.Format(ParameterSuffixFormat, exception.ParamName),
            StringComparison.Ordinal);

        return suffixStart < 0 ? exception.Message : exception.Message[..suffixStart];
    }
}
