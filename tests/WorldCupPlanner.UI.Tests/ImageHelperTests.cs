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
    public void IsSupportedImage_SvgWithXmlDeclaration_IsAccepted()
    {
        byte[] svg = Encoding.UTF8.GetBytes(
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<!-- exportado -->\n<svg xmlns=\"http://www.w3.org/2000/svg\"></svg>");

        Assert.IsTrue(ImageHelper.IsSupportedImage(svg));
    }

    [TestMethod]
    public void IsSupportedImage_XmlThatIsNotSvg_IsRejected()
    {
        byte[] sitemap = Encoding.UTF8.GetBytes(
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\"></urlset>");

        Assert.IsFalse(ImageHelper.IsSupportedImage(sitemap));
    }

    // Al mostrar solo se lee el comienzo, y un SVG exportado puede traer comentarios antes de su <svg.
    [TestMethod]
    public void GuessImageMimeType_SvgWhoseRootComesAfterThePrefix_IsStillSvg()
    {
        string svg = "<?xml version=\"1.0\"?><!--" + new string('-', 300) + "--><svg xmlns=\"http://www.w3.org/2000/svg\"></svg>";
        string base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(svg));

        Assert.IsTrue(ImageHelper.IsSupportedImage(Encoding.UTF8.GetBytes(svg)));
        Assert.AreEqual("image/svg+xml", ImageHelper.GuessImageMimeType(base64));
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
