namespace WorldCupPlanner.UI;

// Las excepciones propias (dominio y servicios) traen mensajes en español pensados para el usuario.
// Las de infraestructura (EF, SQL, IO) pueden mostrar detalles internos: se loguean y se ve uno genérico.
public static class UserFacingError
{
    private const string OwnNamespacePrefix = "WorldCupPlanner";
    private const string UnexpectedErrorMessage = "Ocurrió un error inesperado. Intentá nuevamente.";

    public static string MessageFor(Exception exception, ILogger logger)
    {
        bool isOwnException = exception.GetType().Namespace?.StartsWith(OwnNamespacePrefix, StringComparison.Ordinal) == true;

        if (isOwnException || exception is ArgumentException)
        {
            return exception.Message;
        }

        logger.LogError(exception, "Unexpected error replaced by a generic message for the user.");
        return UnexpectedErrorMessage;
    }
}
