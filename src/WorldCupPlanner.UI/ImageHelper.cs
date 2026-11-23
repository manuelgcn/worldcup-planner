using System.Text;

namespace WorldCupPlanner.UI;

public static class ImageHelper
{
    private const string DefaultMimeType = "image/png";
    // La firma está en los primeros bytes y esto corre en cada render de una bandera. Múltiplo de 4:
    // un prefijo de ese largo sigue siendo base64 válido.
    private const int SignaturePrefixLength = 256;

    public static string GetDataUri(string? base64, string? contentType = null)
    {
        if (string.IsNullOrWhiteSpace(base64))
        {
            return "";
        }

        string mimeType = !string.IsNullOrWhiteSpace(contentType)
            ? contentType
            : GuessImageMimeType(base64);

        return $"data:{mimeType};base64,{base64}";
    }

    public static string GuessImageMimeType(string base64)
    {
        try
        {
            byte[] bytes = Convert.FromBase64String(base64[..Math.Min(SignaturePrefixLength, base64.Length)]);

            if (IsPng(bytes))
            {
                return "image/png";
            }

            if (IsJpeg(bytes))
            {
                return "image/jpeg";
            }

            if (IsWebp(bytes))
            {
                return "image/webp";
            }

            if (IsSvg(bytes))
            {
                return "image/svg+xml";
            }
        }
        catch (FormatException)
        {
        }

        return DefaultMimeType;
    }

    // Los mismos formatos que GuessImageMimeType reconoce: lo que se acepta al subir se puede mostrar después.
    // Con el archivo entero además se exige el <svg, porque cualquier XML empieza con "<?xml"; el prefijo
    // que lee GuessImageMimeType puede no alcanzarlo.
    // shortcut: no parsea el XML, un XHTML con un <svg> adentro pasa; parsearlo si una bandera deja de mostrarse solo en <img>.
    public static bool IsSupportedImage(byte[] bytes)
    {
        return IsPng(bytes) || IsJpeg(bytes) || IsWebp(bytes) ||
               (IsSvg(bytes) && Encoding.UTF8.GetString(bytes).Contains("<svg", StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsPng(byte[] bytes)
    {
        return bytes.Length >= 8 &&
               bytes[0] == 0x89 &&
               bytes[1] == 0x50 &&
               bytes[2] == 0x4E &&
               bytes[3] == 0x47;
    }

    private static bool IsJpeg(byte[] bytes)
    {
        return bytes.Length >= 3 &&
               bytes[0] == 0xFF &&
               bytes[1] == 0xD8 &&
               bytes[2] == 0xFF;
    }

    private static bool IsWebp(byte[] bytes)
    {
        return bytes.Length >= 12 &&
               bytes[0] == 0x52 &&
               bytes[1] == 0x49 &&
               bytes[2] == 0x46 &&
               bytes[3] == 0x46 &&
               bytes[8] == 0x57 &&
               bytes[9] == 0x45 &&
               bytes[10] == 0x42 &&
               bytes[11] == 0x50;
    }

    private static bool IsSvg(byte[] bytes)
    {
        string trimmed = Encoding.UTF8.GetString(bytes).TrimStart();

        return trimmed.StartsWith("<svg", StringComparison.OrdinalIgnoreCase) ||
               trimmed.StartsWith("<?xml", StringComparison.OrdinalIgnoreCase);
    }
}
