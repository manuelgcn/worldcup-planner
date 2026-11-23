using System.Text;
using WorldCupPlanner.Services.DTOs;
using WorldCupPlanner.Services.Exceptions;
using WorldCupPlanner.Services.Interfaces;

namespace WorldCupPlanner.Services.Parsers;

public sealed class CsvStreamTeamParser : ITeamParser
{
    private const string InvalidEncodingMessage = "El archivo debe estar guardado en UTF-8.";

    // Estricto: un CSV en Windows-1252 falla en vez de guardar "M?xico". Lleva preambulo y la deteccion va apagada
    // porque, al detectar la marca de orden de bytes, StreamReader cambia a su UTF-8 por defecto, que no falla.
    private static readonly Encoding StrictUtf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true, throwOnInvalidBytes: true);

    public List<TeamImportDto> Parse(Stream fileStream)
    {
        ArgumentNullException.ThrowIfNull(fileStream);

        using StreamReader reader = new StreamReader(fileStream, StrictUtf8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);

        // La decodificacion ocurre a medida que se lee: el error puede saltar en cualquier fila.
        try
        {
            return ReadRows(reader);
        }
        catch (DecoderFallbackException)
        {
            throw new ImportServiceException(InvalidEncodingMessage);
        }
    }

    private static List<TeamImportDto> ReadRows(StreamReader reader)
    {
        List<TeamImportDto> teams = new List<TeamImportDto>();

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

            TeamImportDto team = new TeamImportDto
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
