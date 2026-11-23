using Microsoft.Extensions.Logging;
using Moq;
using WorldCupPlanner.Domain;
using WorldCupPlanner.Services;
using WorldCupPlanner.Services.Exceptions;

namespace WorldCupPlanner.UI.Tests;

[TestClass]
public class UserFacingErrorTests
{
    private const string GenericMessage = "Ocurrió un error inesperado. Intentá nuevamente.";
    private const string InvalidTeamIdMessage = "El id del equipo debe ser mayor a cero.";

    private readonly Mock<ILogger> logger = new();

    // Quién lanzó una excepción solo se sabe después de lanzarla: construirla no alcanza.
    private static Exception Catch(Action action)
    {
        try
        {
            action();
        }
        catch (Exception exception)
        {
            return exception;
        }

        throw new AssertFailedException("Se esperaba una excepción.");
    }

    private void AssertLogged(Exception exception, Times times)
    {
        logger.Verify(
            log => log.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            times);
    }

    [TestMethod]
    public void MessageFor_OwnException_ShowsItsMessage()
    {
        Exception exception = Catch(() => throw new UserServicesException("Credenciales invalidas"));

        string message = UserFacingError.MessageFor(exception, logger.Object);

        Assert.AreEqual("Credenciales invalidas", message);
        AssertLogged(exception, Times.Never());
    }

    [TestMethod]
    public void MessageFor_InfrastructureException_ShowsTheGenericMessageAndLogsIt()
    {
        Exception exception = Catch(() => throw new InvalidOperationException("Login failed for user 'sa'."));

        string message = UserFacingError.MessageFor(exception, logger.Object);

        Assert.AreEqual(GenericMessage, message);
        AssertLogged(exception, Times.Once());
    }

    [TestMethod]
    public void MessageFor_ArgumentExceptionThrownByTheDomain_ShowsItsMessage()
    {
        Exception exception = Catch(() => new User(1, "Ana", "Pérez", "", "Admin@123", new DateTime(1990, 1, 1)));

        string message = UserFacingError.MessageFor(exception, logger.Object);

        Assert.AreEqual("Email es obligatorio", message);
        AssertLogged(exception, Times.Never());
    }

    [TestMethod]
    public void MessageFor_ArgumentExceptionThrownByTheDomainWithAParameterName_DropsTheParameterSuffix()
    {
        Exception exception = Catch(() => new Team(0, "Uruguay", 1500, false, Confederation.CONMEBOL));

        string message = UserFacingError.MessageFor(exception, logger.Object);

        StringAssert.EndsWith(exception.Message, "(Parameter 'id')");
        Assert.AreEqual(InvalidTeamIdMessage, message);
        AssertLogged(exception, Times.Never());
    }

    [TestMethod]
    public void MessageFor_ArgumentOutOfRangeExceptionThrownByAService_DropsTheParameterSuffix()
    {
        Exception exception = Catch(() => new Exporter().Export(new[] { "fila" }, (ExportFormat)99, "Hoja"));

        string message = UserFacingError.MessageFor(exception, logger.Object);

        Assert.AreEqual("Formato de exportacion no soportado.", message);
    }

    // Con el valor recibido .NET agrega otra línea después del sufijo. Este ensamblado también es WorldCupPlanner.
    [TestMethod]
    public void MessageFor_ArgumentOutOfRangeExceptionWithTheActualValue_KeepsOnlyTheOwnMessage()
    {
        Exception exception = Catch(() => throw new ArgumentOutOfRangeException("phase", 5, "No hay una fase siguiente definida."));

        string message = UserFacingError.MessageFor(exception, logger.Object);

        Assert.AreEqual("No hay una fase siguiente definida.", message);
    }

    [TestMethod]
    public async Task MessageFor_ArgumentExceptionThrownByTheDomainInsideTaskRun_ShowsItsMessage()
    {
        Exception? exception = null;
        try
        {
            await Task.Run(() => new Team(0, "Uruguay", 1500, false, Confederation.CONMEBOL));
        }
        catch (ArgumentException thrown)
        {
            exception = thrown;
        }

        string message = UserFacingError.MessageFor(exception!, logger.Object);

        Assert.AreEqual(InvalidTeamIdMessage, message);
    }

    [TestMethod]
    public void MessageFor_FrameworkNullGuardReachedThroughTheDomain_ShowsTheGenericMessageAndLogsIt()
    {
        Exception exception = Catch(() => new Notification(1, null!, "Resultado cargado"));

        string message = UserFacingError.MessageFor(exception, logger.Object);

        Assert.IsInstanceOfType<ArgumentNullException>(exception);
        Assert.AreEqual(GenericMessage, message);
        AssertLogged(exception, Times.Once());
    }

    [TestMethod]
    public void MessageFor_ArgumentExceptionThrownByTheFramework_ShowsTheGenericMessageAndLogsIt()
    {
        Exception exception = Catch(() => Enum.Parse<UserRoles>("Arquero"));

        string message = UserFacingError.MessageFor(exception, logger.Object);

        Assert.IsInstanceOfType<ArgumentException>(exception);
        Assert.AreEqual(GenericMessage, message);
        AssertLogged(exception, Times.Once());
    }
}
