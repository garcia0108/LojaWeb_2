using LojaWeb_2.Data;
using LojaWeb_2.Models;
using LojaWeb_2.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;


namespace LojaWeb_2.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(
                ILogger<HomeController> logger,
                ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // =========================================
            // PRODUTOS EM DESTAQUE
            // =========================================

            var produtosDestaque = await _context.Produtos
                .Include(p => p.Categoria)
                .Include(p => p.Marca)
                .Where(p => p.Ativo && p.Destaque)
                .OrderBy(p => p.Nome)
                .Take(4)
                .ToListAsync();


            // =========================================
            // PRODUTOS MAIS VENDIDOS
            // =========================================

            var maisVendidos = await _context.ItensVenda
                .Include(i => i.Produto)
                    .ThenInclude(p => p!.Categoria)
                .Where(i =>
                    i.Produto != null &&
                    i.Produto.Ativo &&
                    i.Venda != null &&
                    i.Venda.Status == "Concluída")
                .GroupBy(i => i.ProdutoId)
                .Select(g => new
                {
                    ProdutoId = g.Key,
                    QuantidadeVendida = g.Sum(x => x.Quantidade)
                })
                .OrderByDescending(x => x.QuantidadeVendida)
                .Take(4)
                .Join(
                    _context.Produtos
                        .Include(p => p.Categoria)
                        .Include(p => p.Marca)
                        .Include(p => p.Imagens),
                    venda => venda.ProdutoId,
                    produto => produto.Id,
                    (venda, produto) => produto
                )
                .ToListAsync();


            // =========================================
            // ENVIAR DADOS PARA A HOME
            // =========================================

            var viewModel = new HomeViewModel
            {
                ProdutosDestaque = produtosDestaque,
                MaisVendidos = maisVendidos
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Sobre()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // =========================================
        // PÁGINA NÃO ENCONTRADA - 404
        // =========================================

        [HttpGet]
        public IActionResult NotFoundPage()
        {
            return View();
        }

        // =========================================
        // ACESSO NEGADO
        // =========================================

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // =========================================
        // NEWSLETTER
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Newsletter(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                TempData["NewsletterErro"] = "Informe um e-mail válido.";
                return RedirectToAction(nameof(Index));
            }

            email = email.Trim().ToLower();

            var emailJaExiste = await _context.Newsletters
                .AnyAsync(n => n.Email == email);

            if (emailJaExiste)
            {
                TempData["NewsletterErro"] =
                    "Este e-mail já está cadastrado.";

                return RedirectToAction(nameof(Index));
            }

            var newsletter = new Newsletter
            {
                Email = email,
                DataCadastro = DateTime.Now,
                Ativo = true
            };

            _context.Newsletters.Add(newsletter);

            await _context.SaveChangesAsync();

            TempData["NewsletterSucesso"] =
                "Cadastro realizado com sucesso!";

            return RedirectToAction(nameof(Index));
        }
    }
}
