namespace LojaWeb_2.Services.Importacao
{
    public class TradutorMedidasService
    {
        private readonly Dictionary<string, string> _mapa =
            new(StringComparer.OrdinalIgnoreCase)
            {
                { "A", "Torax" },
                { "B", "Comprimento" },
                { "C", "Barra" },
                { "D", "Ombro" },
                { "E", "ComprimentoManga" },
                { "F", "AberturaManga" },

                { "TORAX", "Torax" },
                { "TÓRAX", "Torax" },
                { "PEITO", "Torax" },
                { "CHEST", "Torax" },

                { "COMPRIMENTO", "Comprimento" },
                { "COMPRIMENTO TOTAL", "Comprimento" },
                { "LENGTH", "Comprimento" },

                { "BARRA", "Barra" },
                { "HEM", "Barra" },

                { "OMBRO", "Ombro" },
                { "OMBROS", "Ombro" },
                { "SHOULDER", "Ombro" },

                { "MANGA", "ComprimentoManga" },
                { "SLEEVE", "ComprimentoManga" },

                { "ABERTURA", "AberturaManga" },
                { "ABERTURA MANGA", "AberturaManga" },
                { "PUNHO", "AberturaManga" },
                { "CUFF", "AberturaManga" },
            };

        public string Traduzir(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return texto;

            texto = texto.Trim().ToUpper();

            return _mapa.TryGetValue(texto, out var campo)
                ? campo
                : texto;
        }
    }
}