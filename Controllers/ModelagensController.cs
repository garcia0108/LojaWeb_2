using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LojaWeb_2.Data;
using LojaWeb_2.Models;

namespace LojaWeb_2.Controllers
{
    public class ModelagensController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ModelagensController(
                ApplicationDbContext context,
                IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: Modelagens
        public async Task<IActionResult> Index()
        {
            var modelagens = await _context.Modelagens
                .Include(m => m.Marca)
                .Include(m => m.Categoria)
                .Include(m => m.Medidas)
                .OrderBy(m => m.Marca.Nome)
                .ThenBy(m => m.Categoria.Nome)
                .ThenBy(m => m.Nome)
                .ToListAsync();

            return View(modelagens);
        }

        // GET: Modelagens/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var modelagem = await _context.Modelagens
                .Include(m => m.Marca)
                .Include(m => m.Categoria)
                .Include(m => m.Medidas)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (modelagem == null)
                return NotFound();

            return View(modelagem);
        }

        // GET: Modelagens/Create
        public IActionResult Create()
        {
            ViewData["MarcaId"] = new SelectList(_context.Marcas.OrderBy(x => x.Nome), "Id", "Nome");
            ViewData["CategoriaId"] = new SelectList(_context.Categorias.OrderBy(x => x.Nome), "Id", "Nome");

            return View();
        }

        // POST: Modelagens/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Modelagem modelagem, IFormFile? imagemArquivo)
        {
            if (!ModelState.IsValid)
            {
                foreach (var item in ModelState)
                {
                    foreach (var erro in item.Value.Errors)
                    {
                        Console.WriteLine($"Campo: {item.Key} | Erro: {erro.ErrorMessage}");
                    }
                }

                ViewData["MarcaId"] = new SelectList(_context.Marcas.OrderBy(x => x.Nome), "Id", "Nome", modelagem.MarcaId);
                ViewData["CategoriaId"] = new SelectList(_context.Categorias.OrderBy(x => x.Nome), "Id", "Nome", modelagem.CategoriaId);

                return View(modelagem);
            }

            if (imagemArquivo != null && imagemArquivo.Length > 0)
            {
                var uploads = Path.Combine(
                    _environment.WebRootPath,
                    "uploads",
                    "modelagens");

                Directory.CreateDirectory(uploads);

                var nomeArquivo = $"{Guid.NewGuid()}{Path.GetExtension(imagemArquivo.FileName)}";

                var caminho = Path.Combine(uploads, nomeArquivo);

                using var stream = new FileStream(caminho, FileMode.Create);

                await imagemArquivo.CopyToAsync(stream);

                modelagem.ImagemReferencia = $"/uploads/modelagens/{nomeArquivo}";
            }

            _context.Modelagens.Add(modelagem);
            await _context.SaveChangesAsync();

            var tamanhos = new[] { "PP", "P", "M", "G", "GG", "XGG" };

            foreach (var tamanho in tamanhos)
            {
                _context.MedidasModelagem.Add(new MedidaModelagem
                {
                    ModelagemId = modelagem.Id,
                    Tamanho = tamanho
                });
            }

            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Modelagem cadastrada com sucesso.";

            return RedirectToAction(nameof(Index));
        }

        // GET: Modelagens/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var modelagem = await _context.Modelagens
                .Include(m => m.Marca)
                .Include(m => m.Categoria)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (modelagem == null)
                return NotFound();

            ViewData["MarcaId"] = new SelectList(
                _context.Marcas.OrderBy(x => x.Nome),
                "Id",
                "Nome",
                modelagem.MarcaId);

            ViewData["CategoriaId"] = new SelectList(
                _context.Categorias.OrderBy(x => x.Nome),
                "Id",
                "Nome",
                modelagem.CategoriaId);

            return View(modelagem);
        }

        // POST: Modelagens/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Modelagem modelagem,
            IFormFile? imagemArquivo)
        {
            if (id != modelagem.Id)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ViewData["MarcaId"] = new SelectList(
                    _context.Marcas.OrderBy(x => x.Nome),
                    "Id",
                    "Nome",
                    modelagem.MarcaId);

                ViewData["CategoriaId"] = new SelectList(
                    _context.Categorias.OrderBy(x => x.Nome),
                    "Id",
                    "Nome",
                    modelagem.CategoriaId);

                return View(modelagem);
            }

            var modelagemBanco = await _context.Modelagens
                .FirstOrDefaultAsync(x => x.Id == id);

            if (modelagemBanco == null)
                return NotFound();

            // Atualiza os dados
            modelagemBanco.Nome = modelagem.Nome;
            modelagemBanco.Descricao = modelagem.Descricao;
            modelagemBanco.MarcaId = modelagem.MarcaId;
            modelagemBanco.CategoriaId = modelagem.CategoriaId;
            modelagemBanco.ModoIlustracao = modelagem.ModoIlustracao;

            // Nova imagem
            if (imagemArquivo != null && imagemArquivo.Length > 0)
            {
                var uploads = Path.Combine(
                    _environment.WebRootPath,
                    "uploads",
                    "modelagens");

                Directory.CreateDirectory(uploads);

                // Remove imagem antiga
                if (!string.IsNullOrEmpty(modelagemBanco.ImagemReferencia))
                {
                    var caminhoAntigo = Path.Combine(
                        _environment.WebRootPath,
                        modelagemBanco.ImagemReferencia.TrimStart('/'));

                    if (System.IO.File.Exists(caminhoAntigo))
                        System.IO.File.Delete(caminhoAntigo);
                }

                var nomeArquivo = $"{Guid.NewGuid()}{Path.GetExtension(imagemArquivo.FileName)}";

                var caminhoNovo = Path.Combine(uploads, nomeArquivo);

                using var stream = new FileStream(caminhoNovo, FileMode.Create);

                await imagemArquivo.CopyToAsync(stream);

                modelagemBanco.ImagemReferencia = $"/uploads/modelagens/{nomeArquivo}";
            }

            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Modelagem atualizada com sucesso.";

            return RedirectToAction(nameof(Index));
        }
        // POST: Modelagens/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var modelagem = await _context.Modelagens.FindAsync(id);

            if (modelagem == null)
                return NotFound();

            _context.Modelagens.Remove(modelagem);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Modelagem excluída com sucesso.";

            return RedirectToAction(nameof(Index));
        }

        // GET: Modelagens/Medidas/5
        public async Task<IActionResult> Medidas(int id)
        {
            var modelagem = await _context.Modelagens
                .Include(m => m.Marca)
                .Include(m => m.Categoria)
                .Include(m => m.Medidas)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (modelagem == null)
                return NotFound();

            modelagem.Medidas = modelagem.Medidas
                .OrderBy(x => x.Id)
                .ToList();

            return View(modelagem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SalvarMedidas(Modelagem modelagem)
        {
            foreach (var medida in modelagem.Medidas)
            {
                _context.Update(medida);
            }

            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Medidas atualizadas com sucesso.";

            return RedirectToAction(nameof(Index));
        }
    }
}