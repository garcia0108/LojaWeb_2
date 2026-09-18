using LojaWeb_2.Data;
using LojaWeb_2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LojaWeb_2.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class ContasReceberController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ContasReceberController(
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
            StatusContaReceber? status)
        {
            var query = _context.ContasReceber
                .Include(c => c.Venda)
                .AsQueryable();


            // PESQUISA

            if (!string.IsNullOrWhiteSpace(pesquisa))
            {
                pesquisa = pesquisa.Trim();

                query = query.Where(c =>
                    c.Descricao.Contains(pesquisa) ||
                    c.FormaPagamento.Contains(pesquisa) ||
                    (c.Venda != null &&
                     c.Venda.Id.ToString()
                        .Contains(pesquisa)));
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


            var conta = await _context.ContasReceber
                .Include(c => c.Venda)
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
        public IActionResult Create()
        {
            return View(new ContaReceber());
        }


        // =========================================
        // CREATE - POST
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            ContaReceber conta)
        {
            if (!ModelState.IsValid)
            {
                return View(conta);
            }


            _context.ContasReceber.Add(conta);

            await _context.SaveChangesAsync();


            TempData["Sucesso"] =
                "Conta a receber cadastrada com sucesso!";


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


            var conta = await _context.ContasReceber
                .FindAsync(id);


            if (conta == null)
            {
                return NotFound();
            }


            return View(conta);
        }


        // =========================================
        // EDIT - POST
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            ContaReceber conta)
        {
            if (id != conta.Id)
            {
                return NotFound();
            }


            if (!ModelState.IsValid)
            {
                return View(conta);
            }


            try
            {
                _context.Update(conta);

                await _context.SaveChangesAsync();


                TempData["Sucesso"] =
                    "Conta a receber atualizada com sucesso!";


                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ContaReceberExists(conta.Id))
                {
                    return NotFound();
                }

                throw;
            }
        }


        // =========================================
        // RECEBER CONTA
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Receber(int id)
        {
            var conta = await _context.ContasReceber
                .FirstOrDefaultAsync(c => c.Id == id);

            if (conta == null)
            {
                return NotFound();
            }

            if (conta.Status == StatusContaReceber.Recebido)
            {
                TempData["Erro"] =
                    "Esta conta já foi recebida.";

                return RedirectToAction(nameof(Index));
            }

            if (conta.Status == StatusContaReceber.Cancelado)
            {
                TempData["Erro"] =
                    "Uma conta cancelada não pode ser recebida.";

                return RedirectToAction(nameof(Index));
            }


            // =========================================
            // DATA DO RECEBIMENTO
            // =========================================

            conta.Status =
                StatusContaReceber.Recebido;

            conta.DataRecebimento =
                DateTime.Now;


            // =========================================
            // LANÇAMENTO FINANCEIRO
            // =========================================

            var lancamento = new LancamentoFinanceiro
            {
                Data = conta.DataRecebimento.Value,

                Descricao =
                    $"Recebimento - {conta.Descricao}",

                Tipo =
                    TipoLancamentoFinanceiro.Receita,

                Categoria =
                    "Recebimento de vendas",

                Valor =
                    conta.Valor,

                FormaPagamento =
                    conta.FormaPagamento,

                Status =
                    StatusLancamentoFinanceiro.Pago,

                DataPagamento =
                    conta.DataRecebimento.Value,

                Observacao =
                    $"Recebimento da conta #{conta.Id}"
            };


            _context.LancamentosFinanceiros.Add(lancamento);


            // =========================================
            // SALVAR
            // =========================================

            await _context.SaveChangesAsync();


            TempData["Sucesso"] =
                "Conta recebida e lançamento financeiro registrado com sucesso!";


            return RedirectToAction(nameof(Index));
        }


        // =========================================
        // CANCELAR
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancelar(int id)
        {
            var conta = await _context.ContasReceber
                .FirstOrDefaultAsync(c =>
                    c.Id == id);


            if (conta == null)
            {
                return NotFound();
            }


            if (conta.Status ==
                StatusContaReceber.Recebido)
            {
                TempData["Erro"] =
                    "Uma conta já recebida não pode ser cancelada.";

                return RedirectToAction(nameof(Index));
            }


            conta.Status =
                StatusContaReceber.Cancelado;


            await _context.SaveChangesAsync();


            TempData["Sucesso"] =
                "Conta a receber cancelada.";


            return RedirectToAction(nameof(Index));
        }


        // =========================================
        // DELETE
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var conta = await _context.ContasReceber
                .FindAsync(id);


            if (conta == null)
            {
                return NotFound();
            }


            if (conta.Status ==
                StatusContaReceber.Recebido)
            {
                TempData["Erro"] =
                    "Contas já recebidas não podem ser excluídas.";

                return RedirectToAction(nameof(Index));
            }


            _context.ContasReceber.Remove(conta);

            await _context.SaveChangesAsync();


            TempData["Sucesso"] =
                "Conta a receber excluída com sucesso!";


            return RedirectToAction(nameof(Index));
        }


        private bool ContaReceberExists(int id)
        {
            return _context.ContasReceber
                .Any(c => c.Id == id);
        }
    }
}