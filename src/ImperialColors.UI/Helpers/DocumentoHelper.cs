using System.Text;

namespace ImperialColors.UI.Helpers;

public static class DocumentoHelper
{
    public static string SomenteDigitos(string? valor)
        => string.IsNullOrEmpty(valor) ? string.Empty : new string(valor.Where(char.IsDigit).ToArray());

    public static string AplicarMascaraCpf(string? valor)
    {
        var digits = SomenteDigitos(valor);
        if (digits.Length > 11) digits = digits[..11];

        var sb = new StringBuilder();
        for (var i = 0; i < digits.Length; i++)
        {
            if (i is 3 or 6) sb.Append('.');
            if (i is 9) sb.Append('-');
            sb.Append(digits[i]);
        }
        return sb.ToString();
    }

    public static string AplicarMascaraCnpj(string? valor)
    {
        var digits = SomenteDigitos(valor);
        if (digits.Length > 14) digits = digits[..14];

        var sb = new StringBuilder();
        for (var i = 0; i < digits.Length; i++)
        {
            if (i is 2 or 5) sb.Append('.');
            if (i is 8) sb.Append('/');
            if (i is 12) sb.Append('-');
            sb.Append(digits[i]);
        }
        return sb.ToString();
    }

    public static string AplicarMascaraCep(string? valor)
    {
        var digits = SomenteDigitos(valor);
        if (digits.Length > 8) digits = digits[..8];

        return digits.Length <= 5
            ? digits
            : $"{digits[..5]}-{digits[5..]}";
    }

    public static bool CpfCompleto(string? valor) => SomenteDigitos(valor).Length == 11;
    public static bool CnpjCompleto(string? valor) => SomenteDigitos(valor).Length == 14;
    public static bool CepCompleto(string? valor) => SomenteDigitos(valor).Length == 8;
}
