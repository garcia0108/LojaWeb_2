using LojaWeb_2.Data;
using LojaWeb_2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace LojaWeb_2.Controllers
{
    [Authorize(Roles = "Administrador, Funcionario")]
    public class FornecedoresController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FornecedoresController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Fornecedores
        public async Task<IActionResult> Index(string? pesquisa)
        {
            var query = _context.Fornecedores
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(pesquisa))
            {
                pesquisa = pesquisa.Trim();

                query = query.Where(f =>
                    f.Nome.Contains(pesquisa) ||
                    (f.NomeFantasia != null &&
                     f.NomeFantasia.Contains(pesquisa)) ||
                    (f.Cnpj != null &&
                     f.Cnpj.Contains(pesquisa)) ||
                    (f.Email != null &&
                     f.Email.Contains(pesquisa)) ||
                    (f.Cidade != null &&
                     f.Cidade.Contains(pesquisa)));
            }

            var fornecedores = await query
                .OrderBy(f => f.Nome)
                .ToListAsync();

            return View(fornecedores);
        }


        // GET: Fornecedores/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fornecedor = await _context.Fornecedores
                .FirstOrDefaultAsync(f => f.Id == id);

            if (fornecedor == null)
            {
                return NotFound();
            }

            return View(fornecedor);
        }

        [Authorize(Roles = "Administrador")]
        // GET: Fornecedores/Create
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "Administrador")]
        // POST: Fornecedores/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Fornecedor fornecedor)
        {
            if (ModelState.IsValid)
            {
                _context.Fornecedores.Add(fornecedor);

                await _context.SaveChangesAsync();

                TempData["Sucesso"] =
                    "Fornecedor cadastrado com sucesso!";

                return RedirectToAction(nameof(Index));
            }

            return View(fornecedor);
        }

        [Authorize(Roles = "Administrador")]
        // GET: Fornecedores/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fornecedor = await _context.Fornecedores
                .FindAsync(id);

            if (fornecedor == null)
            {
                return NotFound();
            }

            return View(fornecedor);
        }

        [Authorize(Roles = "Administrador")]
        // POST: Fornecedores/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Fornecedor fornecedor)
        {
            if (id != fornecedor.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(fornecedor);

                    await _context.SaveChangesAsync();

                    TempData["Sucesso"] =
                        "Fornecedor alterado com sucesso!";

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FornecedorExists(fornecedor.Id))
                    {
                        return NotFound();
                    }

                    throw;
                }
            }

            return View(fornecedor);
        }

        [Authorize(Roles = "Administrador")]
        // GET: Fornecedores/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fornecedor = await _context.Fornecedores
                .FirstOrDefaultAsync(f => f.Id == id);

            if (fornecedor == null)
            {
                return NotFound();
            }

            return View(fornecedor);
        }

        [Authorize(Roles = "Administrador")]
        // POST: Fornecedores/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var fornecedor = await _context.Fornecedores
                .FindAsync(id);

            if (fornecedor != null)
            {
                _context.Fornecedores.Remove(fornecedor);

                await _context.SaveChangesAsync();

                TempData["Sucesso"] =
                    "Fornecedor excluído com sucesso!";
            }

            return RedirectToAction(nameof(Index));
        }


        private bool FornecedorExists(int id)
        {
            return _context.Fornecedores
                .Any(f => f.Id == id);
        }
    }
}