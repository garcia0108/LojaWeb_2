namespace LojaWeb_2.Models
{
    public class DashboardFinanceiroViewModel
    {
        // =========================================
        // RESUMO FINANCEIRO
        // =========================================

        public decimal TotalReceitas { get; set; }

        public decimal TotalDespesas { get; set; }

        public decimal Saldo { get; set; }

        public decimal Lucro { get; set; }


        // =========================================
        // CONTAS A RECEBER
        // =========================================

        public decimal ContasReceber { get; set; }

        public decimal ContasReceberVencidas { get; set; }

        public int QuantidadeContasReceber { get; set; }


        // =========================================
        // CONTAS A PAGAR
        // =========================================

        public decimal ContasPagar { get; set; }

        public decimal ContasPagarVencidas { get; set; }

        public int QuantidadeContasPagar { get; set; }


        // =========================================
        // VENDAS
        // =========================================

        public decimal TotalVendas { get; set; }

        public int QuantidadeVendas { get; set; }


        // =========================================
        // PERÍODO
        // =========================================

        public DateTime? DataInicio { get; set; }

        public DateTime? DataFim { get; set; }


        // =========================================
        // ÚLTIMOS LANÇAMENTOS
        // =========================================

        public List<LancamentoFinanceiro> UltimosLancamentos { get; set; }
            = new List<LancamentoFinanceiro>();

        // =========================================
        // CAIXA ATUAL
        // =========================================

        public FechamentoCaixa? CaixaAberto { get; set; }

        public FechamentoCaixa? UltimoCaixaFechado { get; set; }
    }
}