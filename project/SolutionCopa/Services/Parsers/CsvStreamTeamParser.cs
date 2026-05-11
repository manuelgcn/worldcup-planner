using Services.DTOs;
using Services.Interfaces;

namespace Services.Parsers;

public sealed class CsvStreamTeamParser : ITeamParser
{
    public List<TeamImportDto> Parse(Stream fileStream)
    {
        ArgumentNullException.ThrowIfNull(fileStream);

        List<TeamImportDto> teams = new();
        using StreamReader reader = new(fileStream, leaveOpen: true);

        int rowNumber = 0;
        while (!reader.EndOfStream)
        {
            string? line = reader.ReadLine();
            rowNumber++;

            if (rowNumber == 1)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            string[] columns = line.Split(',');
            string name = string.Empty;
            string confederation = string.Empty;
            string ranking = string.Empty;

            if (columns.Length > 0)
            {
                string nameColumn = columns[0];
                name = nameColumn.Trim();
            }

            if (columns.Length > 1)
            {
                string confederationColumn = columns[1];
                confederation = confederationColumn.Trim();
            }

            if (columns.Length > 2)
            {
                string rankingColumn = columns[2];
                ranking = rankingColumn.Trim();
            }

            TeamImportDto team = new()
            {
                RowNumber = rowNumber,
                Name = name,
                Confederation = confederation,
                Ranking = ranking
            };

            teams.Add(team);
        }

        return teams;
    }
}
