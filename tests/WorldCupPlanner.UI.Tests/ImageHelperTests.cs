using System.Text;

namespace WorldCupPlanner.UI.Tests;

[TestClass]
public class ImageHelperTests
{
    private static readonly byte[] Png = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
    private static readonly byte[] Jpeg = { 0xFF, 0xD8, 0xFF, 0xE0 };
    private static readonly byte[] Webp = Encoding.ASCII.GetBytes("RIFF\0\0\0\0WEBP");
    private static readonly byte[] Svg = Encoding.UTF8.GetBytes("<svg xmlns=\"http://www.w3.org/2000/svg\"></svg>");

    [TestMethod]
    public void IsSupportedImage_Png_IsAccepted()
    {
        Assert.IsTrue(ImageHelper.IsSupportedImage(Png));
    }

    [TestMethod]
    public void IsSupportedImage_Jpeg_IsAccepted()
    {
        Assert.IsTrue(ImageHelper.IsSupportedImage(Jpeg));
    }

    [TestMethod]
    public void IsSupportedImage_Webp_IsAccepted()
    {
        Assert.IsTrue(ImageHelper.IsSupportedImage(Webp));
    }

    [TestMethod]
    public void IsSupportedImage_Svg_IsAccepted()
    {
        Assert.IsTrue(ImageHelper.IsSupportedImage(Svg));
    }

    [TestMethod]
    public void IsSupportedImage_PlainText_IsRejected()
    {
        Assert.IsFalse(ImageHelper.IsSupportedImage(Encoding.UTF8.GetBytes("Nombre,Confederacion,RankingFIFA")));
    }

    [TestMethod]
    public void IsSupportedImage_Executable_IsRejected()
    {
        Assert.IsFalse(ImageHelper.IsSupportedImage(new byte[] { 0x4D, 0x5A, 0x90, 0x00, 0x03, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00 }));
    }

    [TestMethod]
    public void IsSupportedImage_EmptyFile_IsRejected()
    {
        Assert.IsFalse(ImageHelper.IsSupportedImage(Array.Empty<byte>()));
    }

    // Una bandera llega a 200 KB y se adivina su tipo en cada render: alcanza con leer el comienzo.
    // El final inválido lo prueba: decodificar todo el texto falla y cae en el tipo por defecto.
    [TestMethod]
    public void GuessImageMimeType_LongImage_ReadsOnlyItsFirst256Characters()
    {
        byte[] image = new byte[300];
        Jpeg.CopyTo(image, 0);
        string base64 = Convert.ToBase64String(image)[..^4] + "****";

        string mimeType = ImageHelper.GuessImageMimeType(base64);

        Assert.AreEqual("image/jpeg", mimeType);
    }

    [TestMethod]
    public void GuessImageMimeType_ImageShorterThanThePrefix_IsStillRecognised()
    {
        string base64 = Convert.ToBase64String(Jpeg);

        string mimeType = ImageHelper.GuessImageMimeType(base64);

        Assert.AreEqual("image/jpeg", mimeType);
    }
}
