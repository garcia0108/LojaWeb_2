namespace LojaWeb_2.Models
{
    public class RelatorioFinanceiroViewModel
    {
        public DateTime DataInicio { get; set; }

        public DateTime DataFim { get; set; }

        public decimal TotalReceitas { get; set; }

        public decimal TotalDespesas { get; set; }

        public decimal Saldo { get; set; }

        public int QuantidadeReceitas { get; set; }

        public int QuantidadeDespesas { get; set; }

        public List<LancamentoFinanceiro> Lancamentos { get; set; }
            = new List<LancamentoFinanceiro>();

        // =========================================
        // FILTROS
        // =========================================

        public TipoLancamentoFinanceiro? Tipo { get; set; }

        public StatusLancamentoFinanceiro? Status { get; set; }

        public string? FormaPagamento { get; set; }

        public List<string> FormasPagamento { get; set; }
             = new List<string>();
    }
}