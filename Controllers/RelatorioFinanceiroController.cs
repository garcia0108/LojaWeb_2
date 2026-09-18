using LojaWeb_2.Data;
using LojaWeb_2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LojaWeb_2.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class RelatorioFinanceiroController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RelatorioFinanceiroController(
            ApplicationDbContext context)
        {
            _context = context;
        }


        // =========================================
        // GET: RELATÓRIO FINANCEIRO
        // =========================================

        [HttpGet]
        public async Task<IActionResult> Index(
       DateTime? dataInicio,
       DateTime? dataFim,
       TipoLancamentoFinanceiro? tipo,
       StatusLancamentoFinanceiro? status,
       string? formaPagamento)
        {
            // =========================================
            // PERÍODO PADRÃO
            // MÊS ATUAL
            // =========================================

            var inicio = dataInicio ??
                new DateTime(
                    DateTime.Today.Year,
                    DateTime.Today.Month,
                    1);

            var fim = dataFim ??
                inicio.AddMonths(1).AddDays(-1);


            // =========================================
            // LANÇAMENTOS
            // =========================================

            var query =
                _context
                    .LancamentosFinanceiros
                    .Where(l =>
                        l.Data >= inicio &&
                        l.Data < fim.Date.AddDays(1));


            // =========================================
            // FILTRO POR TIPO
            // =========================================

            if (tipo.HasValue)
            {
                query = query.Where(l =>
                    l.Tipo == tipo.Value);
            }


            // =========================================
            // FILTRO POR STATUS
            // =========================================

            if (status.HasValue)
            {
                query = query.Where(l =>
                    l.Status == status.Value);
            }


            // =========================================
            // FILTRO POR FORMA DE PAGAMENTO
            // =========================================

            if (!string.IsNullOrWhiteSpace(formaPagamento))
            {
                query = query.Where(l =>
                    l.FormaPagamento == formaPagamento);
            }


            // =========================================
            // BUSCAR LANÇAMENTOS
            // =========================================

            var lancamentos =
                await query
                    .OrderByDescending(l => l.Data)
                    .ToListAsync();


            // =========================================
            // RECEITAS
            // =========================================

            var receitas =
                lancamentos
                    .Where(l =>
                        l.Tipo ==
                        TipoLancamentoFinanceiro.Receita &&
                        l.Status ==
                        StatusLancamentoFinanceiro.Pago)
                    .ToList();


            // =========================================
            // DESPESAS
            // =========================================

            var despesas =
                lancamentos
                    .Where(l =>
                        l.Tipo ==
                        TipoLancamentoFinanceiro.Despesa &&
                        l.Status ==
                        StatusLancamentoFinanceiro.Pago)
                    .ToList();


            // =========================================
            // TOTAIS
            // =========================================

            var totalReceitas =
                receitas.Sum(l => l.Valor);

            var totalDespesas =
                despesas.Sum(l => l.Valor);

            var saldo =
                totalReceitas -
                totalDespesas;

            var formasPagamento =
                await _context
                    .LancamentosFinanceiros
                    .Where(l =>
                        !string.IsNullOrWhiteSpace(l.FormaPagamento))
                    .Select(l => l.FormaPagamento!)
                    .Distinct()
                    .OrderBy(f => f)
                    .ToListAsync();

            // =========================================
            // VIEWMODEL
            // =========================================

            var model =
                new RelatorioFinanceiroViewModel
                {
                    DataInicio = inicio,

                    DataFim = fim,

                    Tipo = tipo,

                    Status = status,

                    FormaPagamento = formaPagamento,

                    FormasPagamento =
                        formasPagamento,

                    TotalReceitas =
                        totalReceitas,

                    TotalDespesas =
                        totalDespesas,

                    Saldo =
                        saldo,

                    QuantidadeReceitas =
                        receitas.Count,

                    QuantidadeDespesas =
                        despesas.Count,

                    Lancamentos =
                        lancamentos
                };


            return View(model);
        }
    }
}