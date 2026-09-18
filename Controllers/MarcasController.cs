
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LojaWeb_2.Models;
using LojaWeb_2.Data;
using Microsoft.AspNetCore.Authorization;

[Authorize(Roles = "Administrador, Funcionario")]
public class MarcasController : Controller
{
    private readonly ApplicationDbContext _context;

    public MarcasController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: MARCAS
    public async Task<IActionResult> Index(string? pesquisa)
    {
        var query = _context.Marcas.AsQueryable();

        // PESQUISA
        if (!string.IsNullOrWhiteSpace(pesquisa))
        {
            pesquisa = pesquisa.Trim();

            query = query.Where(m =>
                m.Nome.Contains(pesquisa));
        }

        var marcas = await query
            .OrderBy(m => m.Nome)
            .ToListAsync();

        ViewBag.Pesquisa = pesquisa;

        return View(marcas);
    }

    // GET: MARCAS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var marca = await _context.Marcas
            .FirstOrDefaultAsync(m => m.Id == id);
        if (marca == null)
        {
            return NotFound();
        }

        return View(marca);
    }

    // GET: MARCAS/Create
    [Authorize(Roles = "Administrador")]
    public IActionResult Create()
    {
        return View();
    }

    // POST: MARCAS/Create
    [Authorize(Roles = "Administrador")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Nome,Descricao")] Marca marca)
    {
        if (ModelState.IsValid)
        {
            _context.Add(marca);
            await _context.SaveChangesAsync();

            TempData["Mensagem"] = "Marca cadastrada com sucesso!";
            TempData["TipoMensagem"] = "success";

            return RedirectToAction(nameof(Index));
        }
        return View(marca);
    }

    // GET: MARCAS/Edit/5
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var marca = await _context.Marcas.FindAsync(id);
        if (marca == null)
        {
            return NotFound();
        }
        return View(marca);
    }

    // POST: MARCAS/Edit/5
    [Authorize(Roles = "Administrador")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,Nome,Descricao")] Marca marca)
    {
        if (id != marca.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(marca);
                await _context.SaveChangesAsync();

                TempData["Mensagem"] = "Marca alterada com sucesso!";
                TempData["TipoMensagem"] = "warning";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MarcaExists(marca.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(marca);
    }

    // GET: MARCAS/Delete/5
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var marca = await _context.Marcas
            .FirstOrDefaultAsync(m => m.Id == id);
        if (marca == null)
        {
            return NotFound();
        }

        return View(marca);
    }

    // POST: MARCAS/Delete/5
    [Authorize(Roles = "Administrador")]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var marca = await _context.Marcas.FindAsync(id);
        if (marca != null)
        {
            _context.Marcas.Remove(marca);
        }

        await _context.SaveChangesAsync();

        TempData["Mensagem"] = "Marca excluída com sucesso.";
        TempData["TipoMensagem"] = "danger";
        return RedirectToAction(nameof(Index));
    }

    private bool MarcaExists(int? id)
    {
        return _context.Marcas.Any(e => e.Id == id);
    }
}
