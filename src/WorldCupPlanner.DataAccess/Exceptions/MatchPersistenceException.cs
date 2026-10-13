namespace WorldCupPlanner.DataAccess.Exceptions;

// Excepcion de la capa DataAccess: se lanza al intentar persistir un partido cuyas referencias
// obligatorias ya persistidas (p. ej. los partidos fuente de un partido de eliminacion) no existen
// en la base. Falla rapido y con un mensaje claro -incluyendo el MatchId ofensor- en lugar de dejar
// que el motor relacional reviente con una violacion de FK criptica.
public class MatchPersistenceException(string message) : Exception(message);
