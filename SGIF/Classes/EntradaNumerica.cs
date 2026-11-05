using System.Globalization;

namespace SGIF.Classes
{
    // Leitura e exibição de números digitados em formulários (aceita vírgula ou ponto).
    public static class EntradaNumerica
    {
        private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

        // Vazio = sem valor (válido, devolve null). Respeita o limite do DECIMAL(10,2) do banco.
        public static bool TentarLerDecimal(string? texto, out decimal? valor)
        {
            valor = null;

            if (string.IsNullOrWhiteSpace(texto))
            {
                return true;
            }

            string normalizado = texto.Trim().Replace(',', '.');

            if (decimal.TryParse(normalizado, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign,
                                 CultureInfo.InvariantCulture, out decimal d)
                && d >= -99999999.99m && d <= 99999999.99m)
            {
                valor = Math.Round(d, 2);
                return true;
            }

            return false;
        }

        // 12.5 vira "12,5"; null vira texto vazio
        public static string Formatar(decimal? valor)
        {
            return valor.HasValue ? valor.Value.ToString("0.##", PtBr) : "";
        }
    }
}
