using System.Globalization;
using System.Text;
using DataAccess.Interfaces;
using ProjectCopa;
using Services.DTOs;
using Services.Interfaces;

namespace Services;

public class FlagAssignmentService(
    ITeamRepository teamRepository,
    IUnitOfWork unitOfWork,
    string flagsDirectoryPath,
    IAuditLogService? auditLogService = null) : IFlagAssignmentService
{
    private const string FlagFileExtension = ".png";
    private const string AuditDetailsTemplate = "Assigned {0} team flags ({1} already had one, {2} without mapping).";

    private static readonly IReadOnlyDictionary<string, string> IsoCodesByNormalizedName =
        new Dictionary<string, string>
        {
            ["alemania"] = "DE",
            ["arabia saudita"] = "SA",
            ["argelia"] = "DZ",
            ["argentina"] = "AR",
            ["australia"] = "AU",
            ["austria"] = "AT",
            ["belgica"] = "BE",
            ["bosnia y herzegovina"] = "BA",
            ["brasil"] = "BR",
            ["cabo verde"] = "CV",
            ["canada"] = "CA",
            ["catar"] = "QA",
            ["colombia"] = "CO",
            ["corea del sur"] = "KR",
            ["costa de marfil"] = "CI",
            ["croacia"] = "HR",
            ["curazao"] = "CW",
            ["ecuador"] = "EC",
            ["egipto"] = "EG",
            ["escocia"] = "GB-SCT",
            ["espana"] = "ES",
            ["estados unidos"] = "US",
            ["francia"] = "FR",
            ["ghana"] = "GH",
            ["haiti"] = "HT",
            ["inglaterra"] = "GB-ENG",
            ["irak"] = "IQ",
            ["iran"] = "IR",
            ["japon"] = "JP",
            ["jordania"] = "JO",
            ["marruecos"] = "MA",
            ["mexico"] = "MX",
            ["noruega"] = "NO",
            ["nueva zelanda"] = "NZ",
            ["paises bajos"] = "NL",
            ["panama"] = "PA",
            ["paraguay"] = "PY",
            ["portugal"] = "PT",
            ["rd congo"] = "CD",
            ["republica checa"] = "CZ",
            ["senegal"] = "SN",
            ["sudafrica"] = "ZA",
            ["suecia"] = "SE",
            ["suiza"] = "CH",
            ["tunez"] = "TN",
            ["turquia"] = "TR",
            ["uruguay"] = "UY",
            ["uzbekistan"] = "UZ"
        };

    public static IReadOnlyCollection<string> KnownIsoCodes =>
        IsoCodesByNormalizedName.Values.Distinct().ToList();

    public FlagAssignmentResult AssignFlags(bool overwriteExisting = false, string? performedByEmail = null)
    {
        int assignedCount = 0;
        int skippedExistingCount = 0;
        List<string> unmappedTeamNames = new List<string>();

        foreach (Team team in teamRepository.GetAll())
        {
            if (team.IsFictitious)
            {
                continue;
            }

            if (!overwriteExisting && !string.IsNullOrWhiteSpace(team.FlagBase64))
            {
                skippedExistingCount++;
                continue;
            }

            string? flagBase64 = LoadFlagFor(team.Name);

            if (flagBase64 is null)
            {
                unmappedTeamNames.Add(team.Name);
                continue;
            }

            team.UpdateFlag(flagBase64);
            teamRepository.Update(team);
            assignedCount++;
        }

        if (assignedCount > 0)
        {
            unitOfWork.Commit();
        }

        auditLogService?.LogPerformedBy(
            AuditActions.AssignTeamFlags,
            string.Format(AuditDetailsTemplate, assignedCount, skippedExistingCount, unmappedTeamNames.Count),
            performedByEmail,
            nameof(Team));

        return new FlagAssignmentResult(assignedCount, skippedExistingCount, unmappedTeamNames);
    }

    private string? LoadFlagFor(string teamName)
    {
        string normalizedName = NormalizeName(teamName);

        if (!IsoCodesByNormalizedName.TryGetValue(normalizedName, out string? isoCode))
        {
            return null;
        }

        string flagPath = Path.Combine(flagsDirectoryPath, isoCode + FlagFileExtension);

        if (!File.Exists(flagPath))
        {
            return null;
        }

        return Convert.ToBase64String(File.ReadAllBytes(flagPath));
    }

    private static string NormalizeName(string name)
    {
        string lowered = name.ToLowerInvariant();
        string decomposed = lowered.Normalize(NormalizationForm.FormD);

        StringBuilder builder = new StringBuilder(decomposed.Length);

        foreach (char character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            builder.Append(char.IsLetterOrDigit(character) ? character : ' ');
        }

        string withSingleSpaces = string.Join(' ',
            builder.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries));

        return withSingleSpaces;
    }
}
