using DataAccess.Interfaces;
using ProjectCopa;
using Services.DTOs;
using Services.Exceptions;
using Services.Interfaces;

namespace Services;

public sealed class ImportService : IImportService
{
    private const int MaxNameLength = 60;
    private const int MinRanking = 300;
    private const int MaxRanking = 2500;
    private const string InvalidTeamNameMessage = "Invalid team name.";
    private const string InvalidConfederationMessage = "Invalid confederation.";
    private const string InvalidRankingMessage = "Invalid ranking.";
    private const string DuplicateTeamMessage = "Duplicate team name.";

    private readonly ITeamRepository _teamRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITeamParser _teamParser;

    private ImportService(ITeamRepository teamRepository, IUnitOfWork unitOfWork, ITeamParser teamParser)
    {
        _teamRepository = teamRepository;
        _unitOfWork = unitOfWork;
        _teamParser = teamParser;
    }

    public static ImportService CreateInstance(ITeamRepository teamRepository, IUnitOfWork unitOfWork, ITeamParser teamParser)
    {
        return new ImportService(teamRepository, unitOfWork, teamParser);
    }

    public ImportResult ImportTeams(Stream fileStream)
    {
        ArgumentNullException.ThrowIfNull(fileStream);

        List<TeamImportDto> rows = _teamParser.Parse(fileStream);
        HashSet<string> importedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        List<Team> importedTeams = new List<Team>();
        int nextTeamId = GetNextTeamId();

        foreach (TeamImportDto row in rows)
        {
            string name = row.Name.Trim();
            string confederationText = row.Confederation.Trim();
            string rankingText = row.Ranking.Trim();

            ValidateName(name, row.RowNumber);
            Confederation confederation = ParseConfederation(confederationText, row.RowNumber);
            int ranking = ParseRanking(rankingText, row.RowNumber);
            if (IsDuplicate(name, importedNames))
                ThrowDuplicateTeam(row.RowNumber);

            Team team = new Team(nextTeamId, name, ranking, false, confederation);
            _teamRepository.Add(team);
            importedTeams.Add(team);
            importedNames.Add(name);
            nextTeamId++;
        }

        _unitOfWork.Commit();
        return new ImportResult(importedTeams);
    }

    private int GetNextTeamId()
    {
        IReadOnlyCollection<Team> existingTeams = _teamRepository.GetAll();
        if (existingTeams.Count == 0)
        {
            return 1;
        }

        return existingTeams.Max(team => team.Id) + 1;
    }

    private static void ValidateName(string name, int rowNumber)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > MaxNameLength)
            ThrowInvalidTeamName(rowNumber);
    }

    private static Confederation ParseConfederation(string confederationText, int rowNumber)
    {
        if (!Enum.TryParse(confederationText, ignoreCase: true, out Confederation confederation))
            ThrowInvalidConfederation(rowNumber);

        return confederation;
    }

    private static int ParseRanking(string rankingText, int rowNumber)
    {
        if (!int.TryParse(rankingText, out int ranking) || IsInvalidRanking(ranking))
            ThrowInvalidRanking(rowNumber);

        return ranking;
    }

    private static bool IsInvalidRanking(int ranking)
    {
        return ranking < MinRanking || ranking > MaxRanking;
    }

    private bool IsDuplicate(string name, HashSet<string> importedNames) =>
        importedNames.Contains(name) || _teamRepository.ExistsByName(name);

    private static void ThrowInvalidTeamName(int rowNumber) =>
        throw new ImportServiceException($"{InvalidTeamNameMessage} Row: {rowNumber}.");

    private static void ThrowInvalidConfederation(int rowNumber) =>
        throw new ImportServiceException($"{InvalidConfederationMessage} Row: {rowNumber}.");

    private static void ThrowInvalidRanking(int rowNumber) =>
        throw new ImportServiceException($"{InvalidRankingMessage} Row: {rowNumber}.");

    private static void ThrowDuplicateTeam(int rowNumber) =>
        throw new ImportServiceException($"{DuplicateTeamMessage} Row: {rowNumber}.");
}
