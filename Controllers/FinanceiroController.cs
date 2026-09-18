using LojaWeb_2.Data;
using LojaWeb_2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LojaWeb_2.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class FinanceiroController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FinanceiroController(
            ApplicationDbContext context)
        {
            _context = context;
        }


        // =========================================
        // DASHBOARD FINANCEIRO
        // =========================================

        [HttpGet]
        public async Task<IActionResult> Index(
            DateTime? dataInicio,
            DateTime? dataFim)
        {
            // Período padrão:
            // mês atual

            var inicio = dataInicio ??
                new DateTime(
                    DateTime.Today.Year,
                    DateTime.Today.Month,
                    1);

            var fim = dataFim ??
                inicio.AddMonths(1).AddDays(-1);


            // =========================================
            // LANÇAMENTOS FINANCEIROS
            // =========================================

            var lancamentos = await _context
                .LancamentosFinanceiros
                .Where(l =>
                    l.Data >= inicio &&
                    l.Data < fim.Date.AddDays(1))
                .ToListAsync();


            var totalReceitas = lancamentos
                .Where(l =>
                    l.Tipo ==
                    TipoLancamentoFinanceiro.Receita &&
                    l.Status ==
                    StatusLancamentoFinanceiro.Pago)
                .Sum(l => l.Valor);


            var totalDespesas = lancamentos
                .Where(l =>
                    l.Tipo ==
                    TipoLancamentoFinanceiro.Despesa &&
                    l.Status ==
                    StatusLancamentoFinanceiro.Pago)
                .Sum(l => l.Valor);


            // =========================================
            // SALDO
            // =========================================

            var saldo =
                totalReceitas -
                totalDespesas;


            // =========================================
            // LUCRO
            // =========================================

            var lucro = saldo;


            // =========================================
            // CONTAS A RECEBER
            // =========================================

            var contasReceber = await _context
                .ContasReceber
                .Where(c =>
                    c.Status ==
                    StatusContaReceber.Pendente)
                .ToListAsync();


            var totalContasReceber =
                contasReceber.Sum(c => c.Valor);


            var contasReceberVencidas =
                contasReceber
                    .Where(c =>
                        c.DataVencimento.Date <
                        DateTime.Today)
                    .Sum(c => c.Valor);


            // =========================================
            // CONTAS A PAGAR
            // =========================================

            var contasPagar = await _context
                .ContasPagar
                .Where(c =>
                    c.Status ==
                    StatusContaPagar.Pendente)
                .ToListAsync();


            var totalContasPagar =
                contasPagar.Sum(c => c.Valor);


            var contasPagarVencidas =
                contasPagar
                    .Where(c =>
                        c.DataVencimento.Date <
                        DateTime.Today)
                    .Sum(c => c.Valor);


            // =========================================
            // VENDAS
            // =========================================

            var vendas = await _context
                .Vendas
                .Where(v =>
                    v.DataVenda >= inicio &&
                    v.DataVenda < fim.Date.AddDays(1) &&
                    v.Status == "Concluída")
                .ToListAsync();


            var totalVendas =
                vendas.Sum(v => v.Total);


            var quantidadeVendas =
                vendas.Count;


            // =========================================
            // ÚLTIMOS LANÇAMENTOS
            // =========================================

            var ultimosLancamentos =
                await _context
                    .LancamentosFinanceiros
                    .OrderByDescending(l => l.Data)
                    .Take(10)
                    .ToListAsync();

            // =========================================
            // CAIXA ATUAL
            // =========================================

            var caixaAberto =
                await _context.FechamentosCaixa
                    .FirstOrDefaultAsync(c =>
                        c.Status == StatusCaixa.Aberto);


            var ultimoCaixaFechado =
                await _context.FechamentosCaixa
                    .Where(c =>
                        c.Status == StatusCaixa.Fechado)
                    .OrderByDescending(c =>
                        c.DataFechamento)
                    .FirstOrDefaultAsync();

            // =========================================
            // ATUALIZAR CAIXA ABERTO
            // =========================================

            if (caixaAberto != null)
            {
                var lancamentosCaixa =
                    await _context
                        .LancamentosFinanceiros
                        .Where(l =>
                            l.Status ==
                                StatusLancamentoFinanceiro.Pago
                            &&
                            l.Data >= caixaAberto.DataAbertura
                            &&
                            l.Data <= DateTime.Now)
                        .ToListAsync();


                caixaAberto.TotalEntradas =
                    lancamentosCaixa
                        .Where(l =>
                            l.Tipo ==
                            TipoLancamentoFinanceiro.Receita)
                        .Sum(l => l.Valor);


                caixaAberto.TotalSaidas =
                    lancamentosCaixa
                        .Where(l =>
                            l.Tipo ==
                            TipoLancamentoFinanceiro.Despesa)
                        .Sum(l => l.Valor);


                caixaAberto.SaldoEsperado =
                    caixaAberto.SaldoInicial
                    + caixaAberto.TotalEntradas
                    - caixaAberto.TotalSaidas;
            }

            // =========================================
            // VIEWMODEL
            // =========================================

            var model =
                new DashboardFinanceiroViewModel
                {
                    TotalReceitas = totalReceitas,

                    TotalDespesas = totalDespesas,

                    Saldo = saldo,

                    Lucro = lucro,

                    ContasReceber =
                        totalContasReceber,

                    ContasReceberVencidas =
                        contasReceberVencidas,

                    QuantidadeContasReceber =
                        contasReceber.Count,

                    ContasPagar =
                        totalContasPagar,

                    ContasPagarVencidas =
                        contasPagarVencidas,

                    QuantidadeContasPagar =
                        contasPagar.Count,

                    TotalVendas =
                        totalVendas,

                    QuantidadeVendas =
                        quantidadeVendas,

                    DataInicio =
                        inicio,

                    DataFim =
                        fim,

                    UltimosLancamentos =
                        ultimosLancamentos,

                    CaixaAberto =
                         caixaAberto,

                    UltimoCaixaFechado =
                         ultimoCaixaFechado,
                };


            return View(model);
        }
    }
}