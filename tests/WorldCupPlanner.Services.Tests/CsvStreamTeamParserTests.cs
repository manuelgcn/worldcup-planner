using WorldCupPlanner.Services.DTOs;
using WorldCupPlanner.Services.Exceptions;
using WorldCupPlanner.Services.Parsers;

namespace WorldCupPlanner.Services.Tests;

[TestClass]
public class CsvStreamTeamParserTests
{
    [TestMethod]
    public void ParseReturnsRowsAfterHeader()
    {
        // Arrange
        string csv = "Name,Confederation,Ranking\nArgentina,CONMEBOL,500\nJapan,AFC,600";
        MemoryStream stream = CreateStream(csv);
        CsvStreamTeamParser parser = new CsvStreamTeamParser();

        // Act
        List<TeamImportDto> rows = parser.Parse(stream);

        // Assert
        Assert.AreEqual(2, rows.Count);
        Assert.AreEqual("Argentina", rows[0].Name);
    }

    [TestMethod]
    public void ParseThrowsWhenStreamIsNull()
    {
        // Arrange
        CsvStreamTeamParser parser = new CsvStreamTeamParser();

        // Act

        // Assert
        Assert.ThrowsExactly<ArgumentNullException>(() => parser.Parse(null!));
    }

    [TestMethod]
    public void ParseSkipsBlankLines()
    {
        // Arrange
        string csv = "Name,Confederation,Ranking\n\nArgentina,CONMEBOL,500\n";
        MemoryStream stream = CreateStream(csv);
        CsvStreamTeamParser parser = new CsvStreamTeamParser();

        // Act
        List<TeamImportDto> rows = parser.Parse(stream);

        // Assert
        Assert.AreEqual(1, rows.Count);
        Assert.AreEqual("CONMEBOL", rows[0].Confederation);
    }

    [TestMethod]
    public void ParseRejectsAFileThatIsNotUtf8()
    {
        // Excel guarda "CSV" en Windows-1252: ahi la é de México es un byte suelto, invalido en UTF-8.
        byte[] bytes = Windows1252Bytes("Name,Confederation,Ranking\nMéxico,CONCACAF,1660");
        CsvStreamTeamParser parser = new CsvStreamTeamParser();

        ImportServiceException ex = Assert.ThrowsExactly<ImportServiceException>(() => parser.Parse(new MemoryStream(bytes)));

        Assert.AreEqual("El archivo debe estar guardado en UTF-8.", ex.Message);
    }

    [TestMethod]
    public void ParseRejectsInvalidBytesAfterAUtf8ByteOrderMark()
    {
        // StreamReader, al detectar la marca, cambia a un UTF-8 que reemplaza los bytes invalidos en vez de fallar.
        byte[] bytes = Utf8ByteOrderMark.Concat(Windows1252Bytes("Name,Confederation,Ranking\nMéxico,CONCACAF,1660")).ToArray();
        CsvStreamTeamParser parser = new CsvStreamTeamParser();

        Assert.ThrowsExactly<ImportServiceException>(() => parser.Parse(new MemoryStream(bytes)));
    }

    [TestMethod]
    public void ParseReadsAUtf8FileWithByteOrderMark()
    {
        // "CSV UTF-8" de Excel empieza con la marca de orden de bytes.
        byte[] bytes = Utf8ByteOrderMark.Concat(System.Text.Encoding.UTF8.GetBytes("Name,Confederation,Ranking\nMéxico,CONCACAF,1660")).ToArray();
        CsvStreamTeamParser parser = new CsvStreamTeamParser();

        List<TeamImportDto> rows = parser.Parse(new MemoryStream(bytes));

        Assert.AreEqual("México", rows.Single().Name);
    }

    [TestMethod]
    public void ParseReadsAQuotedFieldThatContainsTheDelimiter()
    {
        MemoryStream stream = CreateStream("Name,Confederation,Ranking\n\"Congo, RD\",CAF,1380");

        TeamImportDto row = new CsvStreamTeamParser().Parse(stream).Single();

        Assert.AreEqual("Congo, RD", row.Name);
        Assert.AreEqual("CAF", row.Confederation);
        Assert.AreEqual("1380", row.Ranking);
    }

    [TestMethod]
    public void ParseDetectsASemicolonDelimiter()
    {
        // Excel en español separa con punto y coma.
        MemoryStream stream = CreateStream("Nombre;Confederación;Ranking\nMéxico;CONCACAF;1660\nBrasil;CONMEBOL;1760\n");

        List<TeamImportDto> rows = new CsvStreamTeamParser().Parse(stream);

        Assert.AreEqual(2, rows.Count);
        Assert.AreEqual("México", rows[0].Name);
        Assert.AreEqual("CONCACAF", rows[0].Confederation);
        Assert.AreEqual("1760", rows[1].Ranking);
    }

    [TestMethod]
    public void ParseDetectsTheDelimiterInAFileWithBlankLines()
    {
        MemoryStream stream = CreateStream("Nombre;Confederación;Ranking\n\nMéxico;CONCACAF;1660\n\nBrasil;CONMEBOL;1760\n");

        List<TeamImportDto> rows = new CsvStreamTeamParser().Parse(stream);

        Assert.AreEqual(2, rows.Count);
        Assert.AreEqual("CONMEBOL", rows[1].Confederation);
    }

    [TestMethod]
    public void ParseDetectsTheDelimiterWhenANameContainsAnotherOne()
    {
        MemoryStream stream = CreateStream("Nombre;Confederación;Ranking\nCongo, RD;CAF;1380\nTrinidad, Tobago;CONCACAF;1300\n");

        List<TeamImportDto> rows = new CsvStreamTeamParser().Parse(stream);

        Assert.AreEqual("Congo, RD", rows[0].Name);
        Assert.AreEqual("CONCACAF", rows[1].Confederation);
    }

    [TestMethod]
    public void ParseKeepsTheDelimiterOfTheHeaderWhenARowHasNone()
    {
        // El detector de CsvHelper exige el separador en todas las lineas: con una sola fila sin ';'
        // leia el archivo entero con ',' y el error de importacion senalaba la fila 2, que estaba bien.
        MemoryStream stream = CreateStream("Nombre;Confederación;Ranking\nMéxico;CONCACAF;1660\nBrasil\nJapón;AFC;1648\n");

        List<TeamImportDto> rows = new CsvStreamTeamParser().Parse(stream);

        Assert.AreEqual("CONCACAF", rows[0].Confederation);
        Assert.AreEqual("Brasil", rows[1].Name);
        Assert.AreEqual(string.Empty, rows[1].Confederation);
        Assert.AreEqual(3, rows[1].RowNumber);
        Assert.AreEqual("1648", rows[2].Ranking);
    }

    [TestMethod]
    [DataRow("Name,Confederation,Ranking\n\nArgentina,CONMEBOL,500\nJapan,AFC,600")]
    [DataRow("Name,Confederation,Ranking\r\n\r\nArgentina,CONMEBOL,500\r\nJapan,AFC,600\r\n")]
    public void ParseNumbersRowsByPhysicalLine(string csv)
    {
        // El numero de fila es el que el usuario ve en su editor: cuenta el encabezado y las lineas en blanco.
        List<TeamImportDto> rows = new CsvStreamTeamParser().Parse(CreateStream(csv));

        CollectionAssert.AreEqual(new[] { 3, 4 }, rows.Select(row => row.RowNumber).ToArray());
    }

    [TestMethod]
    public void ParseSkipsLinesWithoutAnyValue()
    {
        MemoryStream stream = CreateStream("Name,Confederation,Ranking\n   \nArgentina,CONMEBOL,500\n,,\n");

        List<TeamImportDto> rows = new CsvStreamTeamParser().Parse(stream);

        Assert.AreEqual("Argentina", rows.Single().Name);
    }

    [TestMethod]
    [DataRow("Argentina,CONMEBOL,500\nJapan,AFC,600")]
    [DataRow("Argentina,conmebol,sin ranking\nJapan,AFC,600")]
    [DataRow("Argentina,Sudamerica,500\nJapan,AFC,600")]
    [DataRow("Uruguay,Conmebol.,18OO\nJapan,AFC,600")]
    public void ParseRejectsAFileWhoseFirstRowIsATeam(string csv)
    {
        // Sin encabezado, la primera seleccion se perdia sin aviso.
        ImportServiceException ex = Assert.ThrowsExactly<ImportServiceException>(
            () => new CsvStreamTeamParser().Parse(CreateStream(csv)));

        StringAssert.Contains(ex.Message, "encabezado");
    }

    [TestMethod]
    public void ParseAcceptsTheHeaderOfTheSampleFile()
    {
        MemoryStream stream = CreateStream("Nombre,Confederación,RankingFIFA\nMéxico,CONCACAF,1660");

        Assert.AreEqual("México", new CsvStreamTeamParser().Parse(stream).Single().Name);
    }

    [TestMethod]
    public void ParseReportsAMalformedRowWithItsNumber()
    {
        // Una comilla suelta en un campo sin comillas no tiene una lectura unica.
        MemoryStream stream = CreateStream("Name,Confederation,Ranking\nArgentina,CONMEBOL,500\nCosta \"Rica,CONCACAF,1500\n");

        ImportServiceException ex = Assert.ThrowsExactly<ImportServiceException>(
            () => new CsvStreamTeamParser().Parse(stream));

        StringAssert.Contains(ex.Message, "Row: 3.");
    }

    private static readonly byte[] Utf8ByteOrderMark = { 0xEF, 0xBB, 0xBF };

    // Latin1 coincide con Windows-1252 en las letras acentuadas del español.
    private static byte[] Windows1252Bytes(string text) => System.Text.Encoding.Latin1.GetBytes(text);

    private static MemoryStream CreateStream(string csv)
    {
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(csv);
        return new MemoryStream(bytes);
    }
}
