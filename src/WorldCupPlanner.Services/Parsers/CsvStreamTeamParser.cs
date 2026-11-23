using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Delegates;
using WorldCupPlanner.Domain;
using WorldCupPlanner.Services.DTOs;
using WorldCupPlanner.Services.Exceptions;
using WorldCupPlanner.Services.Interfaces;

namespace WorldCupPlanner.Services.Parsers;

public sealed class CsvStreamTeamParser : ITeamParser
{
    private const string InvalidEncodingMessage = "El archivo debe estar guardado en UTF-8.";
    private const string MissingHeaderMessage = "La primera fila del archivo debe ser el encabezado (Nombre, Confederación, Ranking), no un equipo.";
    private const string MalformedRowMessage = "El archivo CSV está mal formado.";
    private const int NameColumn = 0;
    private const int ConfederationColumn = 1;
    private const int RankingColumn = 2;

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
        CsvConfiguration configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            DetectDelimiter = true,
            GetDelimiter = DelimiterOfHeader,
            TrimOptions = TrimOptions.Trim
        };
        using CsvParser csv = new CsvParser(reader, configuration, leaveOpen: true);

        List<TeamImportDto> teams = new List<TeamImportDto>();
        bool headerRead = false;

        try
        {
            while (csv.Read())
            {
                string[] columns = csv.Record!;

                if (columns.All(string.IsNullOrWhiteSpace))
                {
                    continue;
                }

                if (!headerRead)
                {
                    EnsureIsHeader(columns);
                    headerRead = true;
                    continue;
                }

                teams.Add(new TeamImportDto
                {
                    // Linea fisica del archivo: es la que el usuario ve en su editor.
                    RowNumber = csv.RawRow,
                    Name = columns.ElementAtOrDefault(NameColumn) ?? string.Empty,
                    Confederation = columns.ElementAtOrDefault(ConfederationColumn) ?? string.Empty,
                    Ranking = columns.ElementAtOrDefault(RankingColumn) ?? string.Empty
                });
            }
        }
        catch (CsvHelperException)
        {
            throw new ImportServiceException($"{MalformedRowMessage} Row: {csv.RawRow}.");
        }

        return teams;
    }

    // El detector de CsvHelper exige el separador en todas las lineas: una sola fila sin ninguno hacia leer el
    // archivo entero con ',' y el error senalaba la fila 2. Alcanza con el encabezado; si empata o no hay, es ','.
    private static string DelimiterOfHeader(GetDelimiterArgs args)
    {
        string header = args.Text.Split('\r', '\n').FirstOrDefault(line => !string.IsNullOrWhiteSpace(line)) ?? string.Empty;
        return args.Configuration.DetectDelimiterValues.MaxBy(delimiter => header.Split(delimiter).Length)!;
    }

    // Los titulos son libres ("Nombre,Confederación,RankingFIFA", "Name,Confederation,Ranking"), asi que se mira
    // la forma: una primera fila con una confederacion o con algun digito en el ranking es un equipo, y saltearla
    // lo perderia.
    // shortcut: un titulo con un digito en la tercera columna ("Ranking 2026") se rechaza, y un equipo sin encabezado,
    // con la confederacion mal escrita y un ranking sin digitos, se sigue salteando; si molesta, exigir titulos conocidos.
    private static void EnsureIsHeader(string[] columns)
    {
        bool looksLikeATeam =
            Enum.GetNames<Confederation>().Contains(columns.ElementAtOrDefault(ConfederationColumn), StringComparer.OrdinalIgnoreCase)
            || columns.ElementAtOrDefault(RankingColumn)?.Any(char.IsDigit) == true;

        if (looksLikeATeam)
        {
            throw new ImportServiceException(MissingHeaderMessage);
        }
    }
}
