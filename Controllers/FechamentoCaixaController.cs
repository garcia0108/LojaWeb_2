using LojaWeb_2.Data;
using LojaWeb_2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LojaWeb_2.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class FechamentoCaixaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FechamentoCaixaController(
            ApplicationDbContext context)
        {
            _context = context;
        }


        // =========================================
        // HISTÓRICO DE FECHAMENTOS
        // =========================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var fechamentos =
                await _context.FechamentosCaixa
                    .OrderByDescending(f => f.DataAbertura)
                    .ToListAsync();

            return View(fechamentos);
        }


        // =========================================
        // ABRIR CAIXA - GET
        // =========================================

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // Verifica se já existe um caixa aberto

            var caixaAberto =
                await _context.FechamentosCaixa
                    .AnyAsync(c =>
                        c.Status ==
                        StatusCaixa.Aberto);

            if (caixaAberto)
            {
                TempData["Erro"] =
                    "Já existe um caixa aberto.";

                return RedirectToAction(nameof(Index));
            }


            // Procura o último caixa fechado

            var ultimoCaixa =
                await _context.FechamentosCaixa
                    .Where(c =>
                        c.Status ==
                        StatusCaixa.Fechado)
                    .OrderByDescending(c =>
                        c.DataFechamento)
                    .FirstOrDefaultAsync();


            var saldoInicial =
                ultimoCaixa?.SaldoInformado
                ?? ultimoCaixa?.SaldoEsperado
                ?? 0m;


            var caixa = new FechamentoCaixa
            {
                DataAbertura = DateTime.Now,

                SaldoInicial = saldoInicial,

                TotalEntradas = 0m,

                TotalSaidas = 0m,

                SaldoEsperado = saldoInicial,

                Status = StatusCaixa.Aberto
            };


            return View(caixa);
        }


        // =========================================
        // ABRIR CAIXA - POST
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            FechamentoCaixa caixa)
        {
            // Confirma novamente se existe caixa aberto

            var caixaAberto =
                await _context.FechamentosCaixa
                    .AnyAsync(c =>
                        c.Status ==
                        StatusCaixa.Aberto);

            if (caixaAberto)
            {
                TempData["Erro"] =
                    "Já existe um caixa aberto.";

                return RedirectToAction(nameof(Index));
            }


            if (!ModelState.IsValid)
            {
                return View(caixa);
            }


            caixa.DataAbertura =
                DateTime.Now;

            caixa.Status =
                StatusCaixa.Aberto;


            caixa.TotalEntradas = 0m;

            caixa.TotalSaidas = 0m;


            caixa.SaldoEsperado =
                caixa.SaldoInicial;


            caixa.SaldoInformado = null;

            caixa.Diferenca = null;

            caixa.DataFechamento = null;


            _context.FechamentosCaixa.Add(caixa);

            await _context.SaveChangesAsync();


            TempData["Sucesso"] =
                "Caixa aberto com sucesso!";


            return RedirectToAction(nameof(Index));
        }


        // =========================================
        // DETALHES
        // =========================================

        [HttpGet]
        public async Task<IActionResult> Details(
       int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var caixa =
                await _context.FechamentosCaixa
                    .FirstOrDefaultAsync(c =>
                        c.Id == id);

            if (caixa == null)
            {
                return NotFound();
            }


            // =========================================
            // ATUALIZAR VALORES DO CAIXA ABERTO
            // =========================================

            if (caixa.Status == StatusCaixa.Aberto)
            {
                var inicio =
                    caixa.DataAbertura;

                var fim =
                    DateTime.Now;


                var lancamentos =
                    await _context
                        .LancamentosFinanceiros
                        .Where(l =>
                            l.Status ==
                                StatusLancamentoFinanceiro.Pago
                            &&
                            l.Data >= inicio
                            &&
                            l.Data <= fim)
                        .ToListAsync();


                // =====================================
                // ENTRADAS
                // =====================================

                caixa.TotalEntradas =
                    lancamentos
                        .Where(l =>
                            l.Tipo ==
                            TipoLancamentoFinanceiro.Receita)
                        .Sum(l => l.Valor);


                // =====================================
                // SAÍDAS
                // =====================================

                caixa.TotalSaidas =
                    lancamentos
                        .Where(l =>
                            l.Tipo ==
                            TipoLancamentoFinanceiro.Despesa)
                        .Sum(l => l.Valor);


                // =====================================
                // SALDO ESPERADO
                // =====================================

                caixa.SaldoEsperado =
                    caixa.SaldoInicial
                    + caixa.TotalEntradas
                    - caixa.TotalSaidas;
            }


            return View(caixa);
        }


        // =========================================
        // FECHAR CAIXA - GET
        // =========================================

        [HttpGet]
        public async Task<IActionResult> Fechar(
            int? id)
        {
            if (id == null)
            {
                return NotFound();
            }


            var caixa =
                await _context.FechamentosCaixa
                    .FirstOrDefaultAsync(c =>
                        c.Id == id);


            if (caixa == null)
            {
                return NotFound();
            }


            if (caixa.Status ==
                StatusCaixa.Fechado)
            {
                TempData["Erro"] =
                    "Este caixa já está fechado.";

                return RedirectToAction(nameof(Index));
            }


            // =====================================
            // BUSCAR LANÇAMENTOS
            // =====================================

            var inicio =
                caixa.DataAbertura;

            var fim =
                DateTime.Now;


            var lancamentos =
                await _context
                    .LancamentosFinanceiros
                    .Where(l =>
                        l.Status ==
                        StatusLancamentoFinanceiro.Pago
                        &&
                        l.Data >= inicio
                        &&
                        l.Data <= fim)
                    .ToListAsync();


            caixa.TotalEntradas =
                lancamentos
                    .Where(l =>
                        l.Tipo ==
                        TipoLancamentoFinanceiro.Receita)
                    .Sum(l => l.Valor);


            caixa.TotalSaidas =
                lancamentos
                    .Where(l =>
                        l.Tipo ==
                        TipoLancamentoFinanceiro.Despesa)
                    .Sum(l => l.Valor);


            caixa.SaldoEsperado =
                caixa.SaldoInicial
                + caixa.TotalEntradas
                - caixa.TotalSaidas;


            return View(caixa);
        }


        // =========================================
        // FECHAR CAIXA - POST
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Fechar(
            int id,
            decimal saldoInformado,
            string? observacao)
        {
            var caixa =
                await _context.FechamentosCaixa
                    .FirstOrDefaultAsync(c =>
                        c.Id == id);


            if (caixa == null)
            {
                return NotFound();
            }


            if (caixa.Status ==
                StatusCaixa.Fechado)
            {
                TempData["Erro"] =
                    "Este caixa já está fechado.";

                return RedirectToAction(nameof(Index));
            }


            // =====================================
            // RECALCULAR LANÇAMENTOS
            // =====================================

            var inicio =
                caixa.DataAbertura;

            var fim =
                DateTime.Now;


            var lancamentos =
                await _context
                    .LancamentosFinanceiros
                    .Where(l =>
                        l.Status ==
                        StatusLancamentoFinanceiro.Pago
                        &&
                        l.Data >= inicio
                        &&
                        l.Data <= fim)
                    .ToListAsync();


            caixa.TotalEntradas =
                lancamentos
                    .Where(l =>
                        l.Tipo ==
                        TipoLancamentoFinanceiro.Receita)
                    .Sum(l => l.Valor);


            caixa.TotalSaidas =
                lancamentos
                    .Where(l =>
                        l.Tipo ==
                        TipoLancamentoFinanceiro.Despesa)
                    .Sum(l => l.Valor);


            caixa.SaldoEsperado =
                caixa.SaldoInicial
                + caixa.TotalEntradas
                - caixa.TotalSaidas;


            // =====================================
            // SALDO INFORMADO
            // =====================================

            caixa.SaldoInformado =
                saldoInformado;


            caixa.Diferenca =
                saldoInformado
                - caixa.SaldoEsperado;


            caixa.Observacao =
                observacao;


            caixa.DataFechamento =
                DateTime.Now;


            caixa.Status =
                StatusCaixa.Fechado;


            await _context.SaveChangesAsync();


            TempData["Sucesso"] =
                "Caixa fechado com sucesso!";


            return RedirectToAction(nameof(Index));
        }
    }
}