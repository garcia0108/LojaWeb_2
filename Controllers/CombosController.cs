using LojaWeb_2.Data;
using LojaWeb_2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LojaWeb_2.Controllers
{
    [Authorize(Roles = "Administrador, Funcionario")]
    public class CombosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CombosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Combos
        public async Task<IActionResult> Index()
        {
            var combos = await _context.Combos
                .Include(c => c.Itens)
                .ThenInclude(i => i.Produto)
                .OrderByDescending(c => c.Id)
                .ToListAsync();

            return View(combos);
        }

        // GET: Combos/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Produtos = new SelectList(
                await _context.Produtos
                    .Where(p => p.Ativo)
                    .OrderBy(p => p.Nome)
                    .ToListAsync(),
                "Id",
                "Nome"
            );

            return View();
        }

        // POST: Combos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Combo combo,
            List<int> produtoIds,
            List<string?> cores,
            List<string?> tamanhos,
            List<int> quantidades,
            IFormFile? imagemArquivo)
        {
            if (produtoIds == null ||
                 quantidades == null ||
                 cores == null ||
                 tamanhos == null ||
                 produtoIds.Count == 0)
            {
                ModelState.AddModelError(
                    "",
                    "Adicione pelo menos um produto ao combo."
                );
            }
            else if (produtoIds.Count != quantidades.Count ||
                     produtoIds.Count != cores.Count ||
                     produtoIds.Count != tamanhos.Count)
            {
                ModelState.AddModelError(
                    "",
                    "Erro nos dados dos produtos do combo."
                );
            }

            if (ModelState.IsValid)
            {
                for (int i = 0; i < produtoIds.Count; i++)
                {
                    combo.Itens.Add(new ComboItem
                    {
                        ProdutoId = produtoIds[i],
                        Cor = cores[i],
                        Tamanho = tamanhos[i],
                        Quantidade = quantidades[i]
                    });
                }

                // =========================================
                // UPLOAD DA IMAGEM DO COMBO
                // =========================================

                if (imagemArquivo != null &&
                    imagemArquivo.Length > 0)
                {
                    var pastaUploads = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        "uploads"
                    );

                    if (!Directory.Exists(pastaUploads))
                    {
                        Directory.CreateDirectory(pastaUploads);
                    }

                    var nomeArquivo =
                        Guid.NewGuid().ToString()
                        + Path.GetExtension(imagemArquivo.FileName);

                    var caminhoArquivo = Path.Combine(
                        pastaUploads,
                        nomeArquivo
                    );

                    using (var stream = new FileStream(
                        caminhoArquivo,
                        FileMode.Create))
                    {
                        await imagemArquivo.CopyToAsync(stream);
                    }

                    combo.Imagem = "/uploads/" + nomeArquivo;
                }

                _context.Combos.Add(combo);

                await _context.SaveChangesAsync();

                TempData["Sucesso"] =
                    "Combo cadastrado com sucesso!";

                return RedirectToAction(nameof(Index));
            }


            ViewBag.Produtos = new SelectList(
                await _context.Produtos
                    .Where(p => p.Ativo)
                    .OrderBy(p => p.Nome)
                    .ToListAsync(),
                "Id",
                "Nome"
            );

            return View(combo);
        }

        // =========================================
        // GET: VARIAÇÕES DO PRODUTO
        // =========================================

        [HttpGet]
        public async Task<IActionResult> VariacoesProduto(int produtoId)
        {
            var variacoes = await _context.Estoques
                .Where(e =>
                    e.ProdutoId == produtoId &&
                    e.Quantidade > 0)
                .Select(e => new
                {
                    e.Cor,
                    e.Tamanho
                })
                .ToListAsync();

            var cores = variacoes
                .Select(v => v.Cor)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            var tamanhos = variacoes
                .Select(v => v.Tamanho)
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Distinct()
                .OrderBy(t => t)
                .ToList();

            return Json(new
            {
                cores,
                tamanhos
            });
        }

        // GET: Combos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var combo = await _context.Combos
                .Include(c => c.Itens)
                .ThenInclude(i => i.Produto)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (combo == null)
            {
                return NotFound();
            }

            return View(combo);
        }

        // GET: Combos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var combo = await _context.Combos
                .Include(c => c.Itens)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (combo == null)
            {
                return NotFound();
            }

            ViewBag.Produtos = new SelectList(
                await _context.Produtos
                    .Where(p => p.Ativo)
                    .OrderBy(p => p.Nome)
                    .ToListAsync(),
                "Id",
                "Nome"
            );

            return View(combo);
        }

        // POST: Combos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Combo combo,
            List<int> produtoIds,
            List<string?> cores,
            List<string?> tamanhos,
            List<int> quantidades,
            IFormFile? imagemArquivo)
        {
            if (id != combo.Id)
            {
                return NotFound();
            }

            // =========================================
            // VALIDAR PRODUTOS
            // =========================================

            if (produtoIds == null ||
                quantidades == null ||
                cores == null ||
                tamanhos == null ||
                produtoIds.Count == 0)
            {
                ModelState.AddModelError(
                    "",
                    "Adicione pelo menos um produto válido ao combo.");
            }
            else if (produtoIds.Count != quantidades.Count ||
                     produtoIds.Count != cores.Count ||
                     produtoIds.Count != tamanhos.Count)
            {
                ModelState.AddModelError(
                    "",
                    "Erro nos dados dos produtos do combo.");
            }

            // =========================================
            // SALVAR
            // =========================================

            if (ModelState.IsValid)
            {
                var comboExistente = await _context.Combos
                    .Include(c => c.Itens)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (comboExistente == null)
                {
                    return NotFound();
                }

                // =========================================
                // ATUALIZAR DADOS DO COMBO
                // =========================================

                comboExistente.Nome = combo.Nome;
                comboExistente.Descricao = combo.Descricao;
                comboExistente.Preco = combo.Preco;
                comboExistente.Ativo = combo.Ativo;
                comboExistente.EmPromocao = combo.EmPromocao;

                // =========================================
                // NOVA IMAGEM
                // =========================================

                if (imagemArquivo != null &&
                    imagemArquivo.Length > 0)
                {
                    string pastaUploads = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        "uploads");

                    if (!Directory.Exists(pastaUploads))
                    {
                        Directory.CreateDirectory(pastaUploads);
                    }

                    // -----------------------------------------
                    // APAGAR IMAGEM ANTIGA
                    // -----------------------------------------

                    if (!string.IsNullOrEmpty(comboExistente.Imagem))
                    {
                        string caminhoImagemAntiga =
                            Path.Combine(
                                Directory.GetCurrentDirectory(),
                                "wwwroot",
                                comboExistente.Imagem.TrimStart('/'));

                        if (System.IO.File.Exists(caminhoImagemAntiga))
                        {
                            System.IO.File.Delete(caminhoImagemAntiga);
                        }
                    }

                    // -----------------------------------------
                    // GERAR NOVO NOME
                    // -----------------------------------------

                    string extensao = Path
                        .GetExtension(imagemArquivo.FileName)
                        .ToLowerInvariant();

                    string nomeArquivo =
                        Guid.NewGuid().ToString() + extensao;

                    string caminhoArquivo =
                        Path.Combine(
                            pastaUploads,
                            nomeArquivo);

                    // -----------------------------------------
                    // SALVAR ARQUIVO
                    // -----------------------------------------

                    using (var stream = new FileStream(
                        caminhoArquivo,
                        FileMode.Create))
                    {
                        await imagemArquivo.CopyToAsync(stream);
                    }

                    // -----------------------------------------
                    // SALVAR CAMINHO NO BANCO
                    // -----------------------------------------

                    comboExistente.Imagem =
                        "/uploads/" + nomeArquivo;
                }

                // =========================================
                // REMOVER ITENS ANTIGOS
                // =========================================

                _context.ComboItens.RemoveRange(
                    comboExistente.Itens);

                // =========================================
                // ADICIONAR NOVOS ITENS
                // =========================================

                for (int i = 0; i < produtoIds.Count; i++)
                {
                    comboExistente.Itens.Add(
                        new ComboItem
                        {
                            ProdutoId = produtoIds[i],
                            Cor = cores[i],
                            Tamanho = tamanhos[i],
                            Quantidade = quantidades[i]
                        });
                }

                // =========================================
                // SALVAR NO BANCO
                // =========================================

                await _context.SaveChangesAsync();

                TempData["Sucesso"] =
                    "Combo atualizado com sucesso!";

                return RedirectToAction(
                    nameof(Index));
            }

            // =========================================
            // RECARREGAR PRODUTOS
            // =========================================

            ViewBag.Produtos = new SelectList(
                await _context.Produtos
                    .Where(p => p.Ativo)
                    .OrderBy(p => p.Nome)
                    .ToListAsync(),
                "Id",
                "Nome"
            );

            return View(combo);
        }

        // GET: Combos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var combo = await _context.Combos
                .Include(c => c.Itens)
                .ThenInclude(i => i.Produto)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (combo == null)
            {
                return NotFound();
            }

            return View(combo);
        }

        // POST: Combos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var combo = await _context.Combos
                .Include(c => c.Itens)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (combo != null)
            {
                // Remove primeiro os itens do combo
                _context.ComboItens.RemoveRange(combo.Itens);

                // Remove o combo
                _context.Combos.Remove(combo);

                await _context.SaveChangesAsync();

                TempData["Sucesso"] =
                    "Combo excluído com sucesso!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}