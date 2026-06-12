namespace Services.Exceptions;

public sealed class ExportServiceException(string message) : Exception(message);
