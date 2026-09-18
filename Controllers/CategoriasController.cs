using LojaWeb_2.Data;
using LojaWeb_2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace LojaWeb_2.Controllers
{
    [Authorize(Roles = "Administrador, Funcionario")]
    public class CategoriasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriasController(ApplicationDbContext context)
        {
            _context = context;
        }


        // =========================================
        // GET: Categorias
        // =========================================

        public async Task<IActionResult> Index(string? pesquisa)
        {
            var query = _context.Categorias.AsQueryable();

            // PESQUISA
            if (!string.IsNullOrWhiteSpace(pesquisa))
            {
                pesquisa = pesquisa.Trim();

                query = query.Where(c =>
                    c.Nome.Contains(pesquisa));
            }

            var categorias = await query
                .OrderBy(c => c.Nome)
                .ToListAsync();

            ViewBag.Pesquisa = pesquisa;

            return View(categorias);
        }


        // =========================================
        // GET: Categorias/Create
        // =========================================

        [Authorize(Roles = "Administrador")]
        public IActionResult Create()
        {
            return View();
        }


        // =========================================
        // POST: Categorias/Create
        // =========================================

        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Categoria categoria,
                IFormFile? imagemArquivo,
                IFormFile? imagemMasculinaArquivo,
                IFormFile? imagemFemininaArquivo)
        {
            if (ModelState.IsValid)
            {
                // =========================================
                // UPLOAD DA IMAGEM
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

                    var extensao = Path
                        .GetExtension(imagemArquivo.FileName)
                        .ToLowerInvariant();

                    var nomeArquivo =
                        Guid.NewGuid().ToString() + extensao;

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

                    categoria.Imagem =
                        "/uploads/" + nomeArquivo;
                }

                // =========================================
                // IMAGEM MASCULINA
                // =========================================

                if (imagemMasculinaArquivo != null &&
                    imagemMasculinaArquivo.Length > 0)
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

                    var extensao = Path
                        .GetExtension(imagemMasculinaArquivo.FileName)
                        .ToLowerInvariant();

                    var nomeArquivo =
                        Guid.NewGuid().ToString() + extensao;

                    var caminhoArquivo = Path.Combine(
                        pastaUploads,
                        nomeArquivo
                    );

                    using (var stream = new FileStream(
                        caminhoArquivo,
                        FileMode.Create))
                    {
                        await imagemMasculinaArquivo.CopyToAsync(stream);
                    }

                    categoria.ImagemMasculina =
                        "/uploads/" + nomeArquivo;
                }


                // =========================================
                // IMAGEM FEMININA
                // =========================================

                if (imagemFemininaArquivo != null &&
                    imagemFemininaArquivo.Length > 0)
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

                    var extensao = Path
                        .GetExtension(imagemFemininaArquivo.FileName)
                        .ToLowerInvariant();

                    var nomeArquivo =
                        Guid.NewGuid().ToString() + extensao;

                    var caminhoArquivo = Path.Combine(
                        pastaUploads,
                        nomeArquivo
                    );

                    using (var stream = new FileStream(
                        caminhoArquivo,
                        FileMode.Create))
                    {
                        await imagemFemininaArquivo.CopyToAsync(stream);
                    }

                    categoria.ImagemFeminina =
                        "/uploads/" + nomeArquivo;
                }

                // =========================================
                // SALVAR CATEGORIA
                // =========================================

                _context.Add(categoria);

                await _context.SaveChangesAsync();

                TempData["Sucesso"] =
                    "Categoria cadastrada com sucesso!";

                return RedirectToAction(nameof(Index));
            }

            return View(categoria);
        }


        // =========================================
        // GET: Categorias/Details
        // =========================================

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var categoria =
                await _context.Categorias
                    .FirstOrDefaultAsync(c => c.Id == id);

            if (categoria == null)
                return NotFound();

            return View(categoria);
        }


        // =========================================
        // GET: Categorias/Edit
        // =========================================

        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var categoria =
                await _context.Categorias.FindAsync(id);

            if (categoria == null)
                return NotFound();

            return View(categoria);
        }


        // =========================================
        // POST: Categorias/Edit
        // =========================================

        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
                int id,
                Categoria categoria,
                IFormFile? imagemArquivo,
                IFormFile? imagemMasculinaArquivo,
                IFormFile? imagemFemininaArquivo)
        {
            if (id != categoria.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // =========================================
                    // BUSCAR CATEGORIA EXISTENTE
                    // =========================================

                    var categoriaExistente =
                        await _context.Categorias
                            .FirstOrDefaultAsync(c =>
                                c.Id == id);

                    if (categoriaExistente == null)
                        return NotFound();


                    // =========================================
                    // ATUALIZAR DADOS
                    // =========================================

                    categoriaExistente.Nome =
                        categoria.Nome;

                    categoriaExistente.Descricao =
                        categoria.Descricao;


                    // =========================================
                    // NOVA IMAGEM
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
                            Directory.CreateDirectory(
                                pastaUploads);
                        }


                        // =========================================
                        // APAGAR IMAGEM ANTIGA
                        // =========================================

                        if (!string.IsNullOrEmpty(
                            categoriaExistente.Imagem))
                        {
                            var caminhoImagemAntiga =
                                Path.Combine(
                                    Directory.GetCurrentDirectory(),
                                    "wwwroot",
                                    categoriaExistente.Imagem
                                        .TrimStart('/'));

                            if (System.IO.File.Exists(
                                caminhoImagemAntiga))
                            {
                                System.IO.File.Delete(
                                    caminhoImagemAntiga);
                            }
                        }


                        // =========================================
                        // GERAR NOVO NOME
                        // =========================================

                        var extensao = Path
                            .GetExtension(
                                imagemArquivo.FileName)
                            .ToLowerInvariant();

                        var nomeArquivo =
                            Guid.NewGuid().ToString()
                            + extensao;


                        var caminhoArquivo =
                            Path.Combine(
                                pastaUploads,
                                nomeArquivo);


                        // =========================================
                        // SALVAR NOVA IMAGEM
                        // =========================================

                        using (var stream =
                            new FileStream(
                                caminhoArquivo,
                                FileMode.Create))
                        {
                            await imagemArquivo
                                .CopyToAsync(stream);
                        }


                        // =========================================
                        // SALVAR CAMINHO
                        // =========================================

                        categoriaExistente.Imagem =
                            "/uploads/" + nomeArquivo;
                    }

                    // =========================================
                    // IMAGEM MASCULINA
                    // =========================================

                    if (imagemMasculinaArquivo != null &&
                        imagemMasculinaArquivo.Length > 0)
                    {
                        string pastaUploads = Path.Combine(
                            Directory.GetCurrentDirectory(),
                            "wwwroot",
                            "uploads");

                        if (!Directory.Exists(pastaUploads))
                        {
                            Directory.CreateDirectory(pastaUploads);
                        }


                        // APAGAR IMAGEM MASCULINA ANTIGA

                        if (!string.IsNullOrEmpty(
                            categoriaExistente.ImagemMasculina))
                        {
                            string caminhoImagemAntiga =
                                Path.Combine(
                                    Directory.GetCurrentDirectory(),
                                    "wwwroot",
                                    categoriaExistente.ImagemMasculina
                                        .TrimStart('/'));

                            if (System.IO.File.Exists(
                                caminhoImagemAntiga))
                            {
                                System.IO.File.Delete(
                                    caminhoImagemAntiga);
                            }
                        }


                        // NOVO NOME

                        string extensao =
                            Path.GetExtension(
                                imagemMasculinaArquivo.FileName)
                                .ToLowerInvariant();

                        string nomeArquivo =
                            Guid.NewGuid().ToString()
                            + extensao;


                        string caminhoArquivo =
                            Path.Combine(
                                pastaUploads,
                                nomeArquivo);


                        // SALVAR

                        using (var stream =
                            new FileStream(
                                caminhoArquivo,
                                FileMode.Create))
                        {
                            await imagemMasculinaArquivo
                                .CopyToAsync(stream);
                        }


                        categoriaExistente.ImagemMasculina =
                            "/uploads/" + nomeArquivo;
                    }


                    // =========================================
                    // IMAGEM FEMININA
                    // =========================================

                    if (imagemFemininaArquivo != null &&
                        imagemFemininaArquivo.Length > 0)
                    {
                        string pastaUploads = Path.Combine(
                            Directory.GetCurrentDirectory(),
                            "wwwroot",
                            "uploads");

                        if (!Directory.Exists(pastaUploads))
                        {
                            Directory.CreateDirectory(pastaUploads);
                        }


                        // APAGAR IMAGEM FEMININA ANTIGA

                        if (!string.IsNullOrEmpty(
                            categoriaExistente.ImagemFeminina))
                        {
                            string caminhoImagemAntiga =
                                Path.Combine(
                                    Directory.GetCurrentDirectory(),
                                    "wwwroot",
                                    categoriaExistente.ImagemFeminina
                                        .TrimStart('/'));

                            if (System.IO.File.Exists(
                                caminhoImagemAntiga))
                            {
                                System.IO.File.Delete(
                                    caminhoImagemAntiga);
                            }
                        }


                        // NOVO NOME

                        string extensao =
                            Path.GetExtension(
                                imagemFemininaArquivo.FileName)
                                .ToLowerInvariant();

                        string nomeArquivo =
                            Guid.NewGuid().ToString()
                            + extensao;


                        string caminhoArquivo =
                            Path.Combine(
                                pastaUploads,
                                nomeArquivo);


                        // SALVAR

                        using (var stream =
                            new FileStream(
                                caminhoArquivo,
                                FileMode.Create))
                        {
                            await imagemFemininaArquivo
                                .CopyToAsync(stream);
                        }


                        categoriaExistente.ImagemFeminina =
                            "/uploads/" + nomeArquivo;
                    }

                    // =========================================
                    // SALVAR ALTERAÇÕES
                    // =========================================

                    await _context.SaveChangesAsync();

                    TempData["Sucesso"] =
                        "Categoria atualizada com sucesso!";
                }
                catch
                {
                    TempData["Erro"] =
                        "Erro ao atualizar a categoria.";
                }

                return RedirectToAction(nameof(Index));
            }

            return View(categoria);
        }


        // =========================================
        // GET: Categorias/Delete
        // =========================================

        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var categoria =
                await _context.Categorias
                    .FirstOrDefaultAsync(c =>
                        c.Id == id);

            if (categoria == null)
                return NotFound();

            return View(categoria);
        }


        // =========================================
        // POST: Categorias/Delete
        // =========================================

        [Authorize(Roles = "Administrador")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
            int id)
        {
            var categoria =
                await _context.Categorias
                    .Include(c => c.Produtos)
                    .FirstOrDefaultAsync(c =>
                        c.Id == id);

            if (categoria == null)
                return NotFound();


            // =========================================
            // NÃO EXCLUIR SE POSSUI PRODUTOS
            // =========================================

            if (categoria.Produtos.Any())
            {
                TempData["Erro"] =
                    "Não é possível excluir uma categoria que possui produto.";

                return RedirectToAction(nameof(Index));
            }


            // =========================================
            // APAGAR IMAGEM DA CATEGORIA
            // =========================================

            if (!string.IsNullOrEmpty(categoria.Imagem))
            {
                var caminhoImagem =
                    Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        categoria.Imagem.TrimStart('/'));

                if (System.IO.File.Exists(caminhoImagem))
                {
                    System.IO.File.Delete(
                        caminhoImagem);
                }
            }


            // =========================================
            // EXCLUIR CATEGORIA
            // =========================================

            _context.Categorias.Remove(categoria);

            await _context.SaveChangesAsync();

            TempData["Sucesso"] =
                "Categoria excluída com sucesso!";

            return RedirectToAction(nameof(Index));
        }
    }
}