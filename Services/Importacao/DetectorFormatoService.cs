using ClosedXML.Excel;

namespace LojaWeb_2.Services.Importacao
{
    public enum TipoFormatoExcel
    {
        LojaWebPadrao,
        Horizontal,
        LetrasAF
    }

    public class DetectorFormatoService
    {
        public TipoFormatoExcel Detectar(IXLWorksheet planilha)
        {
            for (int linha = 1; linha <= 5; linha++)
            {
                var cabecalho = planilha.Row(linha).CellsUsed()
                    .Select(c => c.GetString().Trim().ToUpper())
                    .ToList();

                if (cabecalho.Contains("TAMANHO"))
                    return TipoFormatoExcel.LojaWebPadrao;

                if (cabecalho.Any(x => new[] { "PP", "P", "M", "G", "GG", "XGG" }.Contains(x)))
                    return TipoFormatoExcel.Horizontal;
            }

            return TipoFormatoExcel.LojaWebPadrao;
        }
    }
}