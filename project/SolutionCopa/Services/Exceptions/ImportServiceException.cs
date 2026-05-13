namespace Services.Exceptions;

public sealed class ImportServiceException(string message) : Exception(message);
