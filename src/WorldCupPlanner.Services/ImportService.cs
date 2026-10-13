using WorldCupPlanner.DataAccess.Interfaces;
using WorldCupPlanner.Domain;
using WorldCupPlanner.Services.DTOs;
using WorldCupPlanner.Services.Exceptions;
using WorldCupPlanner.Services.Interfaces;

namespace WorldCupPlanner.Services;

public sealed class ImportService : IImportService
{
    private const int MaxNameLength = 60;
    private const int MinRanking = 300;
    private const int MaxRanking = 2500;
    private const int MaxTeams = 48;

    private const string InvalidTeamNameMessage = "Nombre de equipo invalido.";
    private const string InvalidConfederationMessage = "Confederacion invalida.";
    private const string InvalidRankingMessage = "Ranking invalido.";
    private const string DuplicateTeamMessage = "Nombre de equipo duplicado.";
    private const string MaxTeamsWarningMessage = "No se agregaron todos los equipos, max:48";

    private readonly ITeamRepository _teamRepository;
    private readonly ITeamParser _teamParser;
    private readonly IAuditLogService _auditLogService;

    private ImportService(ITeamRepository teamRepository, ITeamParser teamParser, IAuditLogService auditLogService)
    {
        ArgumentNullException.ThrowIfNull(auditLogService);
        _teamRepository = teamRepository;
        _teamParser = teamParser;
        _auditLogService = auditLogService;
    }

    public static ImportService CreateInstance(ITeamRepository teamRepository, ITeamParser teamParser, IAuditLogService auditLogService)
    {
        return new ImportService(teamRepository, teamParser, auditLogService);
    }

    public ImportResult ImportTeams(Stream fileStream, string? performedByEmail = null)
    {
        ArgumentNullException.ThrowIfNull(fileStream);

        List<TeamImportDto> rows = _teamParser.Parse(fileStream);
        HashSet<string> importedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        List<Team> teamsToImport = new List<Team>();

        IReadOnlyCollection<Team> existingTeams = _teamRepository.GetAll();
        int currentTeamCount = existingTeams.Count;
        int nextTeamId = GetNextTeamId(existingTeams);

        string? warningMessage = null;

        // H4: primero se validan y construyen TODAS las filas; recien despues se persiste. Asi una
        // fila invalida hace abortar el import completo sin dejar escrituras parciales (atomicidad
        // del caso comun, sin depender de una transaccion real). El tope de 48 no es un error: corta
        // el import y devuelve una advertencia con lo que si entraba.
        foreach (TeamImportDto row in rows)
        {
            if (currentTeamCount >= MaxTeams)
            {
                warningMessage = MaxTeamsWarningMessage;
                break;
            }

            string name = row.Name.Trim();
            string confederationText = row.Confederation.Trim();
            string rankingText = row.Ranking.Trim();

            ValidateName(name, row.RowNumber);
            Confederation confederation = ParseConfederation(confederationText, row.RowNumber);
            int ranking = ParseRanking(rankingText, row.RowNumber);

            if (IsDuplicate(name, importedNames))
            {
                ThrowDuplicateTeam(row.RowNumber);
            }

            teamsToImport.Add(new Team(nextTeamId, name, ranking, false, confederation));
            importedNames.Add(name);

            nextTeamId++;
            currentTeamCount++;
        }

        foreach (Team team in teamsToImport)
        {
            _teamRepository.Add(team);
        }

        return new ImportResult(teamsToImport, warningMessage);
    }

    private static int GetNextTeamId(IReadOnlyCollection<Team> existingTeams)
    {
        if (existingTeams.Count == 0)
        {
            return 1;
        }

        return existingTeams.Max(team => team.Id) + 1;
    }

    private static void ValidateName(string name, int rowNumber)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > MaxNameLength)
        {
            ThrowInvalidTeamName(rowNumber);
        }
    }

    private static Confederation ParseConfederation(string confederationText, int rowNumber)
    {
        if (!Enum.TryParse(confederationText, ignoreCase: true, out Confederation confederation))
        {
            ThrowInvalidConfederation(rowNumber);
        }

        return confederation;
    }

    private static int ParseRanking(string rankingText, int rowNumber)
    {
        if (!int.TryParse(rankingText, out int ranking) || IsInvalidRanking(ranking))
        {
            ThrowInvalidRanking(rowNumber);
        }

        return ranking;
    }

    private static bool IsInvalidRanking(int ranking)
    {
        return ranking < MinRanking || ranking > MaxRanking;
    }

    private bool IsDuplicate(string name, HashSet<string> importedNames)
    {
        return importedNames.Contains(name) || _teamRepository.ExistsByName(name);
    }

    private static void ThrowInvalidTeamName(int rowNumber) =>
        throw new ImportServiceException($"{InvalidTeamNameMessage} Row: {rowNumber}.");

    private static void ThrowInvalidConfederation(int rowNumber) =>
        throw new ImportServiceException($"{InvalidConfederationMessage} Row: {rowNumber}.");

    private static void ThrowInvalidRanking(int rowNumber) =>
        throw new ImportServiceException($"{InvalidRankingMessage} Row: {rowNumber}.");

    private static void ThrowDuplicateTeam(int rowNumber) =>
        throw new ImportServiceException($"{DuplicateTeamMessage} Row: {rowNumber}.");
}
