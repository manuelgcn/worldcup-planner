using System.Globalization;
using System.Text;

namespace WorldCupPlanner.Domain;

// Normalizacion canonica de strings del dominio (regla de negocio de CLAUDE.md §7):
// minusculas, sin tildes, no-alfanumericos -> espacio, colapsar espacios y trim.
// Se usa para comparar nombres de forma insensible a mayusculas/acentos
// (unicidad de equipos y estadios) y como concepto de dominio reutilizable.
public static class TextNormalizer
{
    public static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        string lowered = value.ToLowerInvariant();
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

        return string.Join(' ',
            builder.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    public static bool AreEquivalent(string? left, string? right)
    {
        return Normalize(left) == Normalize(right);
    }
}
