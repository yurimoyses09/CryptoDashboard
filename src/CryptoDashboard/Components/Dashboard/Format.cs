using System.Globalization;

namespace CryptoDashboard.Components.Dashboard
{
    // Formatação de valores para exibição, no padrão brasileiro.
    public static class Format
    {
        private static readonly CultureInfo Pt = CultureInfo.GetCultureInfo("pt-BR");

        public static string Price(decimal? value)
        {
            if (value is null) return "—";
            // Moedas de centavos precisam de mais casas decimais para não aparecerem como zero
            var format = value.Value >= 1m ? "N2" : "N6";
            return value.Value.ToString(format, Pt);
        }

        public static string Pct(decimal? value)
        {
            if (value is null) return "—";
            return value.Value.ToString("+0.00;-0.00", Pt) + "%";
        }

        public static string Score(decimal? value)
        {
            if (value is null) return "—";
            return value.Value.ToString("+0.00;-0.00", Pt);
        }

        public static string VariationClass(decimal? value)
        {
            if (value is null) return string.Empty;
            return value.Value >= 0m ? "text-success" : "text-danger";
        }

        public static string MarketCap(decimal? value)
        {
            if (value is null) return "—";
            var v = value.Value;
            if (v >= 1_000_000_000m) return "US$ " + (v / 1_000_000_000m).ToString("N2", Pt) + " bi";
            if (v >= 1_000_000m) return "US$ " + (v / 1_000_000m).ToString("N2", Pt) + " mi";
            return "US$ " + v.ToString("N0", Pt);
        }
    }
}
