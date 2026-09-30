using ClosedXML.Excel;
using LojaWeb_2.ViewModels;

namespace LojaWeb_2.Services.Importacao
{
    public class ImportacaoExcelService
    {
        private readonly DetectorFormatoService _detector;
        private readonly TradutorMedidasService _tradutor;

        public ImportacaoExcelService(
            DetectorFormatoService detector,
            TradutorMedidasService tradutor)
        {
            _detector = detector;
            _tradutor = tradutor;
        }

        public List<LinhaImportacaoViewModel> LerPlanilha(Stream stream)
        {
            using var workbook = new XLWorkbook(stream);

            var planilha = workbook.Worksheet(1);

            var formato = _detector.Detectar(planilha);

            return formato switch
            {
                TipoFormatoExcel.LojaWebPadrao => LerPadrao(planilha),
                TipoFormatoExcel.Horizontal => LerHorizontal(planilha),
                TipoFormatoExcel.LetrasAF => LerLetrasAF(planilha),
                _ => LerPadrao(planilha)
            };
        }

        //Implementar o formato padrão
        private List<LinhaImportacaoViewModel> LerPadrao(IXLWorksheet planilha)
        {
            var linhas = new List<LinhaImportacaoViewModel>();

            var ultimaLinha = planilha.LastRowUsed()?.RowNumber() ?? 1;

            int linhaCabecalho = EncontrarLinhaCabecalhoPadrao(planilha);

            for (int linha = linhaCabecalho + 1; linha <= ultimaLinha; linha++)
            {
                var tamanho = planilha.Cell(linha, 1)
                    .GetValue<string>()
                    .Trim()
                    .ToUpper();

                if (string.IsNullOrWhiteSpace(tamanho))
                    continue;

                linhas.Add(new LinhaImportacaoViewModel
                {
                    Tamanho = tamanho,
                    Torax = LerDecimal(planilha.Cell(linha, 2)),
                    Comprimento = LerDecimal(planilha.Cell(linha, 3)),
                    Barra = LerDecimal(planilha.Cell(linha, 4)),
                    Ombro = LerDecimal(planilha.Cell(linha, 5)),
                    ComprimentoManga = LerDecimal(planilha.Cell(linha, 6)),
                    AberturaManga = LerDecimal(planilha.Cell(linha, 7))
                });
            }

            return linhas;
        }

        //Método para encontrar a linha do cabeçalho no formato padrão
        private int EncontrarLinhaCabecalhoPadrao(IXLWorksheet planilha)
        {
            var ultimaLinha = planilha.LastRowUsed()?.RowNumber() ?? 1;

            for (int linha = 1; linha <= ultimaLinha; linha++)
            {
                var primeiraColuna = planilha.Cell(linha, 1)
                    .GetValue<string>()
                    .Trim()
                    .ToUpper();

                if (primeiraColuna == "TAMANHO")
                    return linha;
            }

            return 1;
        }

        //Preparar o formato horizontal
        private List<LinhaImportacaoViewModel> LerHorizontal(IXLWorksheet planilha)
        {
            var resultado = new Dictionary<string, LinhaImportacaoViewModel>();

            // Na nossa planilha o cabeçalho está na linha 3
            int linhaCabecalho = 3;
            int primeiraLinhaDados = 4;

            var ultimaColuna = planilha.LastColumnUsed()?.ColumnNumber() ?? 1;
            var ultimaLinha = planilha.LastRowUsed()?.RowNumber() ?? primeiraLinhaDados;

            // Cria um objeto para cada tamanho (PP, P, M...)
            for (int coluna = 2; coluna <= ultimaColuna; coluna++)
            {
                var tamanho = planilha.Cell(linhaCabecalho, coluna)
                    .GetValue<string>()
                    .Trim()
                    .ToUpper();

                if (string.IsNullOrWhiteSpace(tamanho))
                    continue;

                resultado[tamanho] = new LinhaImportacaoViewModel
                {
                    Tamanho = tamanho
                };
            }

            // Lê as medidas (A, B, C, D, E, F)
            for (int linha = primeiraLinhaDados; linha <= ultimaLinha; linha++)
            {
                var codigo = planilha.Cell(linha, 1)
                    .GetValue<string>()
                    .Trim()
                    .ToUpper();

                var medida = _tradutor.Traduzir(codigo);

                for (int coluna = 2; coluna <= ultimaColuna; coluna++)
                {
                    var tamanho = planilha.Cell(linhaCabecalho, coluna)
                        .GetValue<string>()
                        .Trim()
                        .ToUpper();

                    if (!resultado.ContainsKey(tamanho))
                        continue;

                    var valor = LerDecimal(planilha.Cell(linha, coluna));

                    var item = resultado[tamanho];

                    switch (medida)
                    {
                        case "Torax":
                            item.Torax = valor;
                            break;
                        case "Comprimento":
                            item.Comprimento = valor;
                            break;
                        case "Barra":
                            item.Barra = valor;
                            break;
                        case "Ombro":
                            item.Ombro = valor;
                            break;
                        case "ComprimentoManga":
                            item.ComprimentoManga = valor;
                            break;
                        case "AberturaManga":
                            item.AberturaManga = valor;
                            break;
                    }
                }
            }

            return resultado.Values.ToList();
        }

        //Preparar o formato Letras A-F
        private List<LinhaImportacaoViewModel> LerLetrasAF(IXLWorksheet planilha)
        {
            return LerHorizontal(planilha);
        }

        //Método LerDecimal
        private decimal LerDecimal(IXLCell celula)
        {
            if (celula.IsEmpty())
                return 0;

            var texto = celula.GetString().Trim();

            if (string.IsNullOrWhiteSpace(texto))
                return 0;

            texto = texto.Replace(",", ".");

            if (decimal.TryParse(
                texto,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out decimal valor))
                return valor;

            try
            {
                return celula.GetValue<decimal>();
            }
            catch
            {
                return 0;
            }
        }
    }
}