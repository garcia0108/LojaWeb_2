using LojaWeb_2.Data;
using LojaWeb_2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LojaWeb_2.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class FluxoCaixaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FluxoCaixaController(ApplicationDbContext context)
        {
            _context = context;
        }


        // =========================================
        // FLUXO DE CAIXA
        // =========================================

        [HttpGet]
        public async Task<IActionResult> Index(
            DateTime? dataInicial,
            DateTime? dataFinal)
        {
            // Se nenhuma data for informada,
            // usamos o mês atual.

            var inicio = dataInicial
                ?? new DateTime(
                    DateTime.Today.Year,
                    DateTime.Today.Month,
                    1);

            var fim = dataFinal
                ?? inicio.AddMonths(1).AddDays(-1);


            // Ajusta o final para incluir
            // todo o último dia.

            var fimComHora =
                fim.Date.AddDays(1).AddTicks(-1);


            // =====================================
            // LANÇAMENTOS PAGOS
            // =====================================

            var lancamentos = await _context
                .LancamentosFinanceiros
                .Where(l =>
                    l.Status ==
                    StatusLancamentoFinanceiro.Pago
                    &&
                    l.Data >= inicio.Date
                    &&
                    l.Data <= fimComHora)
                .OrderByDescending(l => l.Data)
                .ToListAsync();


            // =====================================
            // ENTRADAS
            // =====================================

            var entradas = lancamentos
                .Where(l =>
                    l.Tipo ==
                    TipoLancamentoFinanceiro.Receita)
                .Sum(l => l.Valor);


            // =====================================
            // SAÍDAS
            // =====================================

            var saidas = lancamentos
                .Where(l =>
                    l.Tipo ==
                    TipoLancamentoFinanceiro.Despesa)
                .Sum(l => l.Valor);


            // =====================================
            // SALDO
            // =====================================

            var saldo = entradas - saidas;


            ViewBag.DataInicial = inicio;
            ViewBag.DataFinal = fim;

            ViewBag.Entradas = entradas;
            ViewBag.Saidas = saidas;
            ViewBag.Saldo = saldo;


            return View(lancamentos);
        }
    }
}