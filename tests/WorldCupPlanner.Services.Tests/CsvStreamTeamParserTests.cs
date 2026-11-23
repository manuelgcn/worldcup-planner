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
        Assert.ThrowsException<ArgumentNullException>(() => parser.Parse(null!));
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

        ImportServiceException ex = Assert.ThrowsException<ImportServiceException>(() => parser.Parse(new MemoryStream(bytes)));

        Assert.AreEqual("El archivo debe estar guardado en UTF-8.", ex.Message);
    }

    [TestMethod]
    public void ParseRejectsInvalidBytesAfterAUtf8ByteOrderMark()
    {
        // StreamReader, al detectar la marca, cambia a un UTF-8 que reemplaza los bytes invalidos en vez de fallar.
        byte[] bytes = Utf8ByteOrderMark.Concat(Windows1252Bytes("Name,Confederation,Ranking\nMéxico,CONCACAF,1660")).ToArray();
        CsvStreamTeamParser parser = new CsvStreamTeamParser();

        Assert.ThrowsException<ImportServiceException>(() => parser.Parse(new MemoryStream(bytes)));
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

    private static readonly byte[] Utf8ByteOrderMark = { 0xEF, 0xBB, 0xBF };

    // Latin1 coincide con Windows-1252 en las letras acentuadas del español.
    private static byte[] Windows1252Bytes(string text) => System.Text.Encoding.Latin1.GetBytes(text);

    private static MemoryStream CreateStream(string csv)
    {
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(csv);
        return new MemoryStream(bytes);
    }
}
