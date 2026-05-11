using Services.DTOs;
using Services.Parsers;

namespace ServiceTest;

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

    private static MemoryStream CreateStream(string csv)
    {
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(csv);
        return new MemoryStream(bytes);
    }
}
