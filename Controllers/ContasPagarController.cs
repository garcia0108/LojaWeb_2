using LojaWeb_2.Data;
using LojaWeb_2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LojaWeb_2.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class ContasPagarController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ContasPagarController(
            ApplicationDbContext context)
        {
            _context = context;
        }


        // =========================================
        // LISTAGEM
        // =========================================

        [HttpGet]
        public async Task<IActionResult> Index(
            string? pesquisa,
            StatusContaPagar? status)
        {
            var query = _context.ContasPagar
                .Include(c => c.Fornecedor)
                .AsQueryable();


            // PESQUISA

            if (!string.IsNullOrWhiteSpace(pesquisa))
            {
                pesquisa = pesquisa.Trim();

                query = query.Where(c =>
                    c.Descricao.Contains(pesquisa) ||
                    (c.FormaPagamento != null &&
                     c.FormaPagamento.Contains(pesquisa)) ||
                    c.Categoria.Contains(pesquisa) ||
                    (c.Fornecedor != null &&
                     c.Fornecedor.Nome.Contains(pesquisa)));
            }


            // STATUS

            if (status.HasValue)
            {
                query = query.Where(c =>
                    c.Status == status.Value);
            }


            var contas = await query
                .OrderBy(c => c.DataVencimento)
                .ToListAsync();


            ViewBag.Pesquisa = pesquisa;
            ViewBag.Status = status;


            return View(contas);
        }


        // =========================================
        // DETAILS
        // =========================================

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }


            var conta = await _context.ContasPagar
                .Include(c => c.Fornecedor)
                .FirstOrDefaultAsync(c =>
                    c.Id == id);


            if (conta == null)
            {
                return NotFound();
            }


            return View(conta);
        }


        // =========================================
        // CREATE - GET
        // =========================================

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Fornecedores =
                await _context.Fornecedores
                    .OrderBy(f => f.Nome)
                    .ToListAsync();

            var conta = new ContaPagar
            {
                DataVencimento = DateTime.Today
            };

            return View(conta);
        }


        // =========================================
        // CREATE - POST
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            ContaPagar conta)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Fornecedores =
                    await _context.Fornecedores
                        .OrderBy(f => f.Nome)
                        .ToListAsync();

                return View(conta);
            }


            _context.ContasPagar.Add(conta);

            await _context.SaveChangesAsync();


            TempData["Sucesso"] =
                "Conta a pagar cadastrada com sucesso!";


            return RedirectToAction(nameof(Index));
        }


        // =========================================
        // EDIT - GET
        // =========================================

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }


            var conta = await _context.ContasPagar
                .FindAsync(id);


            if (conta == null)
            {
                return NotFound();
            }


            ViewBag.Fornecedores =
                await _context.Fornecedores
                    .OrderBy(f => f.Nome)
                    .ToListAsync();


            return View(conta);
        }


        // =========================================
        // EDIT - POST
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            ContaPagar conta)
        {
            if (id != conta.Id)
            {
                return NotFound();
            }


            if (!ModelState.IsValid)
            {
                ViewBag.Fornecedores =
                    await _context.Fornecedores
                        .OrderBy(f => f.Nome)
                        .ToListAsync();

                return View(conta);
            }


            try
            {
                _context.Update(conta);

                await _context.SaveChangesAsync();


                TempData["Sucesso"] =
                    "Conta a pagar atualizada com sucesso!";


                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ContaPagarExists(conta.Id))
                {
                    return NotFound();
                }

                throw;
            }
        }


        // =========================================
        // PAGAR CONTA
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pagar(int id)
        {
            var conta = await _context.ContasPagar
                .FirstOrDefaultAsync(c =>
                    c.Id == id);


            if (conta == null)
            {
                return NotFound();
            }


            if (conta.Status ==
                StatusContaPagar.Pago)
            {
                TempData["Erro"] =
                    "Esta conta já foi paga.";

                return RedirectToAction(nameof(Index));
            }


            if (conta.Status ==
                StatusContaPagar.Cancelado)
            {
                TempData["Erro"] =
                    "Uma conta cancelada não pode ser paga.";

                return RedirectToAction(nameof(Index));
            }


            // =====================================
            // REGISTRAR PAGAMENTO
            // =====================================

            conta.Status =
                StatusContaPagar.Pago;

            conta.DataPagamento =
                DateTime.Now;


            // =====================================
            // LANÇAMENTO FINANCEIRO
            // =====================================

            var lancamento = new LancamentoFinanceiro
            {
                Data = conta.DataPagamento.Value,

                Descricao =
                    $"Pagamento - {conta.Descricao}",

                Tipo =
                    TipoLancamentoFinanceiro.Despesa,

                Categoria =
                    conta.Categoria,

                Valor =
                    conta.Valor,

                FormaPagamento =
                    conta.FormaPagamento,

                Status =
                    StatusLancamentoFinanceiro.Pago,

                DataPagamento =
                    conta.DataPagamento.Value,

                Observacao =
                    $"Pagamento da conta #{conta.Id}"
            };


            _context.LancamentosFinanceiros
                .Add(lancamento);


            // =====================================
            // SALVAR
            // =====================================

            await _context.SaveChangesAsync();


            TempData["Sucesso"] =
                "Conta paga e lançamento financeiro registrado com sucesso!";


            return RedirectToAction(nameof(Index));
        }


        // =========================================
        // CANCELAR
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancelar(int id)
        {
            var conta = await _context.ContasPagar
                .FirstOrDefaultAsync(c =>
                    c.Id == id);


            if (conta == null)
            {
                return NotFound();
            }


            if (conta.Status ==
                StatusContaPagar.Pago)
            {
                TempData["Erro"] =
                    "Uma conta já paga não pode ser cancelada.";

                return RedirectToAction(nameof(Index));
            }


            conta.Status =
                StatusContaPagar.Cancelado;


            await _context.SaveChangesAsync();


            TempData["Sucesso"] =
                "Conta a pagar cancelada.";


            return RedirectToAction(nameof(Index));
        }


        // =========================================
        // DELETE
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var conta = await _context.ContasPagar
                .FindAsync(id);


            if (conta == null)
            {
                return NotFound();
            }


            if (conta.Status ==
                StatusContaPagar.Pago)
            {
                TempData["Erro"] =
                    "Contas já pagas não podem ser excluídas.";

                return RedirectToAction(nameof(Index));
            }


            _context.ContasPagar.Remove(conta);

            await _context.SaveChangesAsync();


            TempData["Sucesso"] =
                "Conta a pagar excluída com sucesso!";


            return RedirectToAction(nameof(Index));
        }


        private bool ContaPagarExists(int id)
        {
            return _context.ContasPagar
                .Any(c => c.Id == id);
        }
    }
}