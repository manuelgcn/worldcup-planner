namespace WorldCupPlanner.Services.Exceptions;

public class FixtureOrchestratorException(string message, Exception? innerException = null)
    : Exception(message, innerException);
