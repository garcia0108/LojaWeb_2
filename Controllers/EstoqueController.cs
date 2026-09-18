using LojaWeb_2.Data;
using LojaWeb_2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.AspNetCore.Authorization;

namespace LojaWeb_2.Controllers
{
    [Authorize(Roles = "Administrador, Funcionario")]
    public class EstoqueController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EstoqueController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Estoque
        public async Task<IActionResult> Index(
                    string? pesquisa,
                    int? produtoId,
                    int? marcaId,
                    string? tamanho,
                    string? cor,
                    string? situacao)
        {
            var query = _context.Estoques
                .Include(e => e.Produto)
                    .ThenInclude(p => p!.Categoria)
                .Include(e => e.Marca)
                .Include(e => e.Produto)
                    .ThenInclude(p => p!.Fornecedor)
                .AsQueryable();


            // PESQUISA
            if (!string.IsNullOrWhiteSpace(pesquisa))
            {
                pesquisa = pesquisa.Trim();

                // Tamanhos válidos
                var tamanhos = new[] { "PP", "P", "M", "G", "GG", "XG" };

                // Verifica se a pesquisa é exatamente um tamanho
                if (tamanhos.Contains(
                    pesquisa,
                    StringComparer.OrdinalIgnoreCase))
                {
                    query = query.Where(e =>
                        e.Tamanho == pesquisa);
                }
                else
                {
                    // Pesquisa geral
                    query = query.Where(e =>
                        (e.Produto != null &&
                         e.Produto.Nome.Contains(pesquisa))

                        ||

                        (e.Marca != null &&
                         e.Marca.Nome.Contains(pesquisa))

                        ||

                        (e.Produto != null &&
                         e.Produto.Fornecedor != null &&
                         e.Produto.Fornecedor.Nome.Contains(pesquisa))

                        ||

                        e.Cor.Contains(pesquisa));
                }
            }

            // =========================================
            // FILTRO POR PRODUTO
            // =========================================

            if (produtoId.HasValue)
            {
                query = query.Where(e =>
                    e.ProdutoId == produtoId.Value);
            }


            // =========================================
            // FILTRO POR MARCA
            // =========================================

            if (marcaId.HasValue)
            {
                query = query.Where(e =>
                    e.MarcaId == marcaId.Value);
            }


            // =========================================
            // FILTRO POR TAMANHO
            // =========================================

            if (!string.IsNullOrWhiteSpace(tamanho))
            {
                query = query.Where(e =>
                    e.Tamanho == tamanho);
            }


            // =========================================
            // FILTRO POR COR
            // =========================================

            if (!string.IsNullOrWhiteSpace(cor))
            {
                query = query.Where(e =>
                    e.Cor == cor);
            }


            // =========================================
            // FILTRO POR SITUAÇÃO
            // =========================================

            if (!string.IsNullOrWhiteSpace(situacao))
            {
                switch (situacao)
                {
                    case "EmEstoque":

                        query = query.Where(e =>
                            e.Quantidade > 5);

                        break;

                    case "EstoqueBaixo":

                        query = query.Where(e =>
                            e.Quantidade > 0 &&
                            e.Quantidade <= 5);

                        break;

                    case "Esgotado":

                        query = query.Where(e =>
                            e.Quantidade == 0);

                        break;
                }
            }

            var estoque = await query
                .OrderBy(e => e.Produto!.Nome)
                .ThenBy(e => e.Marca!.Nome)
                .ThenBy(e => e.Tamanho)
                .ThenBy(e => e.Cor)
                .ToListAsync();


            // INDICADORES

            ViewBag.TotalProdutos = await _context.Produtos
                .CountAsync();

            ViewBag.TotalUnidades = await _context.Estoques
                .SumAsync(e => e.Quantidade);

            ViewBag.EstoqueBaixo = await _context.Estoques
                .CountAsync(e =>
                    e.Quantidade > 0 &&
                    e.Quantidade <= 5);

            ViewBag.Esgotados = await _context.Estoques
                .CountAsync(e =>
                    e.Quantidade == 0);


            // ALERTAS - ESTOQUE BAIXO

            ViewBag.ProdutosEstoqueBaixo =
                await _context.Estoques
                    .Include(e => e.Produto)
                    .Include(e => e.Marca)
                    .Where(e =>
                        e.Quantidade > 0 &&
                        e.Quantidade <= 5)
                    .OrderBy(e => e.Quantidade)
                    .ThenBy(e => e.Produto!.Nome)
                    .ToListAsync();


            // ALERTAS - ESGOTADOS

            ViewBag.ProdutosEsgotados =
                await _context.Estoques
                    .Include(e => e.Produto)
                    .Include(e => e.Marca)
                    .Where(e =>
                        e.Quantidade == 0)
                    .OrderBy(e => e.Produto!.Nome)
                    .ToListAsync();


            // ÚLTIMAS MOVIMENTAÇÕES

            ViewBag.UltimasMovimentacoes =
                await _context.MovimentacoesEstoque
                    .Include(m => m.Produto)
                    .Include(m => m.Marca)
                    .Include(m => m.Fornecedor)
                    .OrderByDescending(m => m.Data)
                    .Take(5)
                    .ToListAsync();

            // =========================================
            // DADOS DOS FILTROS
            // =========================================

            ViewBag.Produtos = await _context.Produtos
                .Where(p => p.Ativo)
                .OrderBy(p => p.Nome)
                .ToListAsync();

            ViewBag.Marcas = await _context.Marcas
                .OrderBy(m => m.Nome)
                .ToListAsync();

            ViewBag.Tamanhos = await _context.Estoques
                .Select(e => e.Tamanho)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();

            ViewBag.Cores = await _context.Estoques
                .Select(e => e.Cor)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();


            // =========================================
            // MANTER FILTROS NA VIEW
            // =========================================

            ViewBag.Pesquisa = pesquisa;
            ViewBag.ProdutoId = produtoId;
            ViewBag.MarcaId = marcaId;
            ViewBag.Tamanho = tamanho;
            ViewBag.Cor = cor;
            ViewBag.Situacao = situacao;

            return View(estoque);
        }

        // GET: Entrada
        public IActionResult Entrada()
        {
            ViewBag.Produtos = new SelectList(
                _context.Produtos.OrderBy(p => p.Nome),
                "Id",
                "Nome");

            ViewBag.Marcas = new SelectList(
                _context.Marcas.OrderBy(m => m.Nome),
                "Id",
                "Nome");

            ViewBag.Fornecedores = new SelectList(
                _context.Fornecedores
                    .Where(f => f.Ativo)
                    .OrderBy(f => f.Nome),
                "Id",
                "Nome");

            return View();
        }

        // POST: Entrada
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Entrada(
            Estoque estoque,
            int fornecedorId,
            string precoCompraUnitario)
        {
            decimal precoCompra = 0;

            var valorInformado = precoCompraUnitario?.Trim();

            if (string.IsNullOrWhiteSpace(valorInformado))
            {
                ModelState.AddModelError(
                    "precoCompraUnitario",
                    "Informe o preço de compra.");
            }
            else
            {
                bool convertido = false;

                // Exemplo: 69,90
                if (valorInformado.Contains(","))
                {
                    valorInformado = valorInformado
                        .Replace(".", "")
                        .Replace(",", ".");

                    convertido = decimal.TryParse(
                        valorInformado,
                        System.Globalization.NumberStyles.Number,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out precoCompra);
                }
                else
                {
                    // Exemplo: 69.90
                    convertido = decimal.TryParse(
                        valorInformado,
                        System.Globalization.NumberStyles.Number,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out precoCompra);
                }

                if (!convertido || precoCompra <= 0)
                {
                    ModelState.AddModelError(
                        "precoCompraUnitario",
                        "Informe um preço de compra válido.");
                }
            }

            if (estoque.ProdutoId <= 0)
            {
                ModelState.AddModelError(
                    "ProdutoId",
                    "Selecione um produto.");
            }

            if (fornecedorId <= 0)
            {
                ModelState.AddModelError(
                    "fornecedorId",
                    "Selecione um fornecedor.");
            }

            if (estoque.Quantidade <= 0)
            {
                ModelState.AddModelError(
                    "Quantidade",
                    "A quantidade deve ser maior que zero.");
            }

            if (estoque.MarcaId <= 0)
            {
                ModelState.AddModelError(
                    "MarcaId",
                    "Selecione uma marca.");
            }

            if (precoCompra <= 0)
            {
                ModelState.AddModelError(
                    "precoCompraUnitario",
                    "O preço de compra deve ser maior que zero.");
            }

            if (ModelState.IsValid)
            {
                using var transaction =
                    await _context.Database.BeginTransactionAsync();

                try
                {
                    var itemExistente = await _context.Estoques
                        .FirstOrDefaultAsync(e =>
                            e.ProdutoId == estoque.ProdutoId &&
                            e.MarcaId == estoque.MarcaId &&
                            e.Tamanho == estoque.Tamanho &&
                            e.Cor == estoque.Cor);

                    if (itemExistente != null)
                    {
                        // Se o estoque antigo ainda não possui custo médio,
                        // não podemos calcular uma média confiável.
                        if (itemExistente.CustoMedio <= 0)
                        {
                            itemExistente.Quantidade += estoque.Quantidade;

                            // A nova entrada passa a ser o custo de referência
                            // para as próximas movimentações.
                            itemExistente.CustoMedio =
                                Math.Round(precoCompra, 2);
                        }
                        else
                        {
                            // Quantidade antes da entrada
                            int quantidadeAnterior =
                                itemExistente.Quantidade;

                            // Custo médio anterior
                            decimal custoMedioAnterior =
                                itemExistente.CustoMedio;

                            // Quantidade da nova entrada
                            int quantidadeEntrada =
                                estoque.Quantidade;

                            // Valor da nova entrada
                            decimal custoEntrada =
                                quantidadeEntrada * precoCompra;

                            // Valor do estoque existente
                            decimal valorEstoqueAnterior =
                                quantidadeAnterior * custoMedioAnterior;

                            // Nova quantidade total
                            int quantidadeTotal =
                                quantidadeAnterior + quantidadeEntrada;

                            // Novo custo médio
                            decimal novoCustoMedio =
                                (valorEstoqueAnterior + custoEntrada)
                                / quantidadeTotal;

                            itemExistente.Quantidade =
                                quantidadeTotal;

                            itemExistente.CustoMedio =
                                Math.Round(novoCustoMedio, 2);
                        }
                    }
                    else
                    {
                        // Primeiro estoque desse produto/variação
                        estoque.CustoMedio =
                            Math.Round(precoCompra, 2);

                        _context.Estoques.Add(estoque);
                    }

                    var movimentacao = new MovimentacaoEstoque
                    {
                        ProdutoId = estoque.ProdutoId,
                        MarcaId = estoque.MarcaId,
                        FornecedorId = fornecedorId,
                        Tamanho = estoque.Tamanho,
                        Cor = estoque.Cor,
                        Quantidade = estoque.Quantidade,
                        PrecoCompraUnitario = precoCompra,
                        Tipo = TipoMovimentacao.Entrada,
                        Data = DateTime.Now,
                        Observacao = "Entrada de estoque"
                    };

                    _context.MovimentacoesEstoque.Add(movimentacao);

                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    TempData["Sucesso"] =
                        "Entrada registrada com sucesso!";

                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    await transaction.RollbackAsync();

                    ModelState.AddModelError(
                        "",
                        "Não foi possível registrar a entrada.");
                }
            }

            ViewBag.Produtos = new SelectList(
                _context.Produtos.OrderBy(p => p.Nome),
                "Id",
                "Nome",
                estoque.ProdutoId);

            ViewBag.Marcas = new SelectList(
                    _context.Marcas
                     .OrderBy(m => m.Nome),
                     "Id",
                     "Nome",
                     estoque.MarcaId);

            ViewBag.Fornecedores = new SelectList(
                _context.Fornecedores
                    .Where(f => f.Ativo)
                    .OrderBy(f => f.Nome),
                "Id",
                "Nome",
                fornecedorId);

            return View(estoque);
        }

        //GET: Saída
        public IActionResult Saida()
        {
            ViewBag.Produtos = new SelectList(
                _context.Produtos,
                "Id",
                "Nome");

            ViewBag.Marcas = new SelectList(
                _context.Marcas,
                "Id",
                "Nome");

            return View();
        }

        // POST: Saida
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Saida(Estoque estoque)
        {
            if (estoque.ProdutoId <= 0)
            {
                ModelState.AddModelError(
                    "ProdutoId",
                    "Selecione um produto.");
            }

            if (estoque.Quantidade <= 0)
            {
                ModelState.AddModelError(
                    "Quantidade",
                    "A quantidade deve ser maior que zero.");
            }

            if (estoque.MarcaId <= 0)
            {
                ModelState.AddModelError(
                    "MarcaId",
                    "Selecione uma marca.");
            }

            if (ModelState.IsValid)
            {
                var item = await _context.Estoques.FirstOrDefaultAsync(
                    e => e.ProdutoId == estoque.ProdutoId &&
                    e.MarcaId == estoque.MarcaId &&
                    e.Tamanho == estoque.Tamanho &&
                    e.Cor == estoque.Cor);

                if (item == null)
                {
                    ModelState.AddModelError("", "Produto não encontrado no estoque.");
                }
                else if (item.Quantidade < estoque.Quantidade)
                {
                    ModelState.AddModelError("Quantidade", $"Quantidade insuficiente. Estoque disponível: {item.Quantidade}");

                }
                else
                {
                    using var transaction = await _context.Database.BeginTransactionAsync();

                    try
                    {
                        item.Quantidade -= estoque.Quantidade;

                        var movimentacao = new MovimentacaoEstoque
                        {
                            ProdutoId = estoque.ProdutoId,
                            MarcaId = estoque.MarcaId,
                            Tamanho = estoque.Tamanho,
                            Cor = estoque.Cor,
                            Quantidade = estoque.Quantidade,
                            PrecoCompraUnitario = item.CustoMedio,
                            Tipo = TipoMovimentacao.Saida,
                            Data = DateTime.Now,
                            Observacao = "Saída de estoque."
                        };

                        _context.MovimentacoesEstoque.Add(movimentacao);

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        TempData["Sucesso"] = "Saída registrada com sucesso!";

                        return RedirectToAction(nameof(Index));
                    }
                    catch
                    {
                        await transaction.RollbackAsync();

                        ModelState.AddModelError(
                            "", "Não foi possível registrar a saída.");
                    }
                }
            }

            ViewBag.Produtos = new SelectList(
                _context.Produtos.OrderBy(p => p.Nome),
                "Id",
                "Nome",
                estoque.ProdutoId);

            if (estoque.MarcaId <= 0)
            {
                ModelState.AddModelError(
                    "MarcaId",
                    "Selecione uma marca.");
            }

            return View(estoque);
        }

        // GET: Histórico de Movimentação
        public async Task<IActionResult> Historico(
            DateTime? dataInicial,
            DateTime? dataFinal,
            int? produtoId,
            int? marcaId,
            TipoMovimentacao? tipo)
        {
            var query = _context.MovimentacoesEstoque
                .Include(m => m.Produto)
                .ThenInclude(p => p!.Categoria)
                .Include(m => m.Marca)
                .Include(m => m.Fornecedor)
                .AsQueryable();


            // =========================================
            // FILTRO - DATA INICIAL
            // =========================================

            if (dataInicial.HasValue)
            {
                query = query.Where(m =>
                    m.Data.Date >= dataInicial.Value.Date);
            }


            // =========================================
            // FILTRO - DATA FINAL
            // =========================================

            if (dataFinal.HasValue)
            {
                query = query.Where(m =>
                    m.Data.Date <= dataFinal.Value.Date);
            }


            // =========================================
            // FILTRO - PRODUTO
            // =========================================

            if (produtoId.HasValue)
            {
                query = query.Where(m =>
                    m.ProdutoId == produtoId.Value);
            }


            // =========================================
            // FILTRO - MARCA
            // =========================================

            if (marcaId.HasValue)
            {
                query = query.Where(m =>
                    m.MarcaId == marcaId.Value);
            }


            // =========================================
            // FILTRO - TIPO
            // =========================================

            if (tipo.HasValue)
            {
                query = query.Where(m =>
                    m.Tipo == tipo.Value);
            }


            // =========================================
            // BUSCAR MOVIMENTAÇÕES
            // =========================================

            var movimentacoes = await query
                .OrderByDescending(m => m.Data)
                .ToListAsync();


            // =========================================
            // DADOS PARA OS FILTROS
            // =========================================

            ViewBag.Produtos = await _context.Produtos
                .Where(p => p.Ativo)
                .OrderBy(p => p.Nome)
                .ToListAsync();

            ViewBag.Marcas = await _context.Marcas
                .OrderBy(m => m.Nome)
                .ToListAsync();


            // =========================================
            // MANTER FILTROS NA VIEW
            // =========================================

            ViewBag.DataInicial =
                dataInicial?.ToString("yyyy-MM-dd");

            ViewBag.DataFinal =
                dataFinal?.ToString("yyyy-MM-dd");

            ViewBag.ProdutoId =
                produtoId;

            ViewBag.MarcaId =
                marcaId;

            ViewBag.Tipo =
                tipo;


            return View(movimentacoes);
        }
    }
}
