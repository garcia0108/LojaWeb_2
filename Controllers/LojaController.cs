using LojaWeb_2.Data;
using LojaWeb_2.Models;
using LojaWeb_2.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LojaWeb_2.Controllers
{
    public class LojaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LojaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Loja
        public async Task<IActionResult> Index(
                string? publico,
                bool novidades = false)
        {
            var query = _context.Produtos
                .Include(p => p.Categoria)
                .Include(p => p.Marca)
                .Include(p => p.Imagens)
                .Where(p => p.Ativo)
                .AsQueryable();


            // =========================================
            // FILTRO POR PÚBLICO
            // =========================================

            if (!string.IsNullOrWhiteSpace(publico))
            {
                query = query.Where(p => p.Publico == publico);
            }


            // =========================================
            // FILTRO DE NOVIDADES
            // =========================================

            if (novidades)
            {
                query = query
                    .OrderByDescending(p => p.Id)
                    .Take(12);
            }
            else
            {
                query = query.OrderBy(p => p.Nome);
            }


            // =========================================
            // PRODUTOS
            // =========================================

            var produtos = await query.ToListAsync();


            // =========================================
            // ÚLTIMOS 5 COMBOS
            // =========================================

            var combos = await _context.Combos
                .Where(c => c.Ativo)
                .OrderByDescending(c => c.Id)
                .Take(5)
                .ToListAsync();


            // Envia os combos para a View Loja

            ViewBag.Combos = combos;

            // =========================================
            // CATEGORIAS MASCULINAS
            // =========================================

            var categoriasMasculinas = await _context.Categorias
                .Where(c => c.Produtos.Any(p =>
                    p.Ativo &&
                    p.Publico == "Masculino"))
                .OrderBy(c => c.Nome)
                .ToListAsync();

            ViewBag.CategoriasMasculinas = categoriasMasculinas;

            // =========================================
            // CATEGORIAS FEMININAS
            // =========================================

            var categoriasFemininas = await _context.Categorias
                .Where(c => c.Produtos.Any(p =>
                    p.Ativo &&
                    p.Publico == "Feminino"))
                .OrderBy(c => c.Nome)
                .ToListAsync();

            ViewBag.CategoriasFemininas = categoriasFemininas;

            // =========================================
            // PROMOÇÕES MASCULINAS - CARROSSEL
            // =========================================

            var promocoesMasculinas = await _context.Produtos
                .Include(p => p.Categoria)
                .Include(p => p.Marca)
                .Include(p => p.Imagens)
                .Where(p =>
                    p.Ativo &&
                    p.EmPromocao &&
                    p.PrecoPromocional != null &&
                    p.Publico == "Masculino")
                .OrderByDescending(p => p.Id)
                .ToListAsync();


            // Divide em páginas de 4 produtos
            var paginasPromocoesMasculinas = promocoesMasculinas
                .Chunk(4)
                .Select(p => p.ToList())
                .Take(4)
                .ToList();

            ViewBag.PaginasPromocoesMasculinas = paginasPromocoesMasculinas;


            // =========================================
            // PROMOÇÕES FEMININAS - CARROSSEL
            // =========================================

            var promocoesFemininas = await _context.Produtos
                .Include(p => p.Categoria)
                .Include(p => p.Marca)
                .Include(p => p.Imagens)
                .Where(p =>
                    p.Ativo &&
                    p.EmPromocao &&
                    p.PrecoPromocional != null &&
                    p.Publico == "Feminino")
                .OrderByDescending(p => p.Id)
                .ToListAsync();


            // Divide em páginas de 4 produtos
            var paginasPromocoesFemininas = promocoesFemininas
                .Chunk(4)
                .Select(p => p.ToList())
                .Take(4)
                .ToList();

            ViewBag.PaginasPromocoesFemininas = paginasPromocoesFemininas;

            return View(produtos);
        }

        //GET: Catalago/Produtos
        public async Task<IActionResult> Produtos(
                string? pesquisa,
                string? publico,
                int? categoriaId,
                string? cor,
                string? tamanho,
                string? ordenar,
                decimal? precoMin,
                decimal? precoMax,
                int pagina = 1)
        {
            var query = _context.Produtos
                .Include(p => p.Categoria)
                .Include(p => p.Marca)
                .Include(p => p.Imagens)
                .Include(p => p.Estoques)
                .Include(p => p.Cores)
                    .ThenInclude(c => c.Imagens)
                .Include(p => p.Cores)
                    .ThenInclude(c => c.ImagemPrincipal)
                .Where(p => p.Ativo)
                .AsQueryable();


            // =========================================
            // PESQUISA POR NOME
            // =========================================

            if (!string.IsNullOrWhiteSpace(pesquisa))
            {
                pesquisa = pesquisa.Trim();

                query = query.Where(p =>
                    p.Nome.Contains(pesquisa));
            }


            // =========================================
            // FILTRO POR PÚBLICO
            // =========================================

            if (!string.IsNullOrWhiteSpace(publico))
            {
                query = query.Where(p =>
                    p.Publico == publico);
            }


            // =========================================
            // FILTRO POR CATEGORIA
            // =========================================

            if (categoriaId.HasValue)
            {
                query = query.Where(p =>
                    p.CategoriaId == categoriaId.Value);
            }


            // =========================================
            // FILTRO POR COR
            // =========================================

            if (!string.IsNullOrWhiteSpace(cor))
            {
                cor = cor.Trim();

                query = query.Where(p =>
                    _context.Estoques.Any(e =>
                        e.ProdutoId == p.Id &&
                        e.Quantidade > 0 &&
                        e.Cor == cor));
            }

            // =========================================
            // FILTRO POR TAMANHO
            // =========================================

            if (!string.IsNullOrWhiteSpace(tamanho))
            {
                tamanho = tamanho.Trim();

                query = query.Where(p =>
                    _context.Estoques.Any(e =>
                        e.ProdutoId == p.Id &&
                        e.Quantidade > 0 &&
                        e.Tamanho == tamanho));
            }

            // =========================================
            // FILTRO POR PREÇO
            // =========================================

            if (precoMin.HasValue)
            {
                query = query.Where(p =>
                    (p.PrecoPromocional.HasValue &&
                     p.PrecoPromocional.Value > 0
                        ? p.PrecoPromocional.Value
                        : p.Preco) >= precoMin.Value);
            }

            if (precoMax.HasValue)
            {
                query = query.Where(p =>
                    (p.PrecoPromocional.HasValue &&
                     p.PrecoPromocional.Value > 0
                        ? p.PrecoPromocional.Value
                        : p.Preco) <= precoMax.Value);
            }

            // =========================================
            // ORDENAÇÃO
            // =========================================

            switch (ordenar)
            {
                case "menor-preco":
                    query = query
                        .OrderBy(p => p.PrecoPromocional.HasValue && p.PrecoPromocional.Value > 0
                            ? p.PrecoPromocional.Value
                            : p.Preco);
                    break;

                case "maior-preco":
                    query = query
                        .OrderByDescending(p => p.PrecoPromocional.HasValue && p.PrecoPromocional.Value > 0
                            ? p.PrecoPromocional.Value
                            : p.Preco);
                    break;

                case "mais-recentes":
                    query = query
                        .OrderByDescending(p => p.Id);
                    break;

                case "nome-az":
                    query = query
                        .OrderBy(p => p.Nome);
                    break;

                case "nome-za":
                    query = query
                        .OrderByDescending(p => p.Nome);
                    break;

                default:
                    query = query
                        .OrderBy(p => p.Nome);
                    break;
            }

            // =========================================
            // MATERIALIZA OS PRODUTOS FILTRADOS
            // =========================================

            var produtosFiltrados = await query.ToListAsync();

            // =========================================
            // CRIA OS ITENS DO CATÁLOGO POR COR
            // =========================================

            var itensCatalogo = new List<ProdutoCatalogoViewModel>();

            foreach (var produto in produtosFiltrados)
            {
                var cores = produto.Cores
                    .Where(c => !string.IsNullOrWhiteSpace(c.Nome))
                    .Where(c => string.IsNullOrWhiteSpace(cor) || c.Nome == cor)
                    .ToList();

                // Produto possui cores cadastradas
                if (cores.Any())
                {
                    foreach (var produtoCor in cores)
                    {
                        var imagem = produtoCor.ImagemPrincipal?.Caminho
                            ?? produtoCor.Imagens
                                .OrderBy(i => i.Id)
                                .Select(i => i.Caminho)
                                .FirstOrDefault();

                        itensCatalogo.Add(new ProdutoCatalogoViewModel
                        {
                            Produto = produto,
                            Cor = produtoCor,
                            Imagem = imagem ?? produto.Imagem
                        });
                    }
                }
                else
                {
                    // Mantém compatibilidade com produtos antigos
                    // que ainda não possuem ProdutoCor
                    itensCatalogo.Add(new ProdutoCatalogoViewModel
                    {
                        Produto = produto,
                        Cor = null,
                        Imagem = produto.Imagem
                    });
                }
            }

            // =========================================
            // PAGINAÇÃO
            // =========================================

            int produtosPorPagina = 12;

            int totalProdutos = itensCatalogo.Count;

            int totalPaginas = (int)Math.Ceiling(
                totalProdutos / (double)produtosPorPagina);

            if (pagina < 1)
            {
                pagina = 1;
            }

            if (totalPaginas > 0 && pagina > totalPaginas)
            {
                pagina = totalPaginas;
            }

            var itensCatalogoPaginados = itensCatalogo
                .Skip((pagina - 1) * produtosPorPagina)
                .Take(produtosPorPagina)
                .ToList();

            // =========================================
            // CATEGORIAS
            // =========================================

            ViewBag.Categorias = await _context.Categorias
                .OrderBy(c => c.Nome)
                .ToListAsync();


            // =========================================
            // CORES DISPONÍVEIS
            // =========================================

            ViewBag.Cores = await _context.Estoques
                .Where(e =>
                    e.Quantidade > 0 &&
                    e.Cor != null &&
                    e.Cor != "")
                .Select(e => e.Cor)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            // =========================================
            // TAMANHOS DISPONÍVEIS
            // =========================================

            ViewBag.Tamanhos = await _context.Estoques
                .Where(e =>
                    e.Quantidade > 0 &&
                    e.Tamanho != null &&
                    e.Tamanho != "")
                .Select(e => e.Tamanho)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();

            // =========================================
            // FILTRO POR PREÇO
            // =========================================

            if (precoMin.HasValue)
            {
                query = query.Where(p =>
                    (p.PrecoPromocional.HasValue &&
                     p.PrecoPromocional.Value > 0
                        ? p.PrecoPromocional.Value
                        : p.Preco) >= precoMin.Value);
            }

            if (precoMax.HasValue)
            {
                query = query.Where(p =>
                    (p.PrecoPromocional.HasValue &&
                     p.PrecoPromocional.Value > 0
                        ? p.PrecoPromocional.Value
                        : p.Preco) <= precoMax.Value);
            }

            // =========================================
            // MANTER FILTROS
            // =========================================

            ViewBag.Pesquisa = pesquisa;
            ViewBag.Publico = publico;
            ViewBag.CategoriaId = categoriaId;
            ViewBag.Cor = cor;
            ViewBag.Tamanho = tamanho;
            ViewBag.Ordenar = ordenar;
            ViewBag.PrecoMin = precoMin;
            ViewBag.PrecoMax = precoMax;
            ViewBag.PaginaAtual = pagina;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.TotalProdutos = totalProdutos;


            return View(itensCatalogoPaginados);
        }

        // GET: Loja/Detalhes/5
        public async Task<IActionResult> Detalhes(int? id, string? cor)
        {
            if (id == null)
                return NotFound();

            var produto = await _context.Produtos
                .Include(p => p.Categoria)
                .Include(p => p.Marca)
                .Include(p => p.Imagens)
                .Include(p => p.Cores)
                    .ThenInclude(c => c.Imagens)
                .Include(p => p.Cores)
                    .ThenInclude(c => c.ImagemPrincipal)
                .FirstOrDefaultAsync(p =>
                    p.Id == id &&
                    p.Ativo);

            if (produto == null)
                return NotFound();

            // =========================================
            // ESTOQUE DISPONÍVEL
            // =========================================

            var estoque = await _context.Estoques
                .Where(e =>
                    e.ProdutoId == id &&
                    e.Quantidade > 0)
                .OrderBy(e => e.Cor)
                .ThenBy(e => e.Tamanho)
                .ToListAsync();


            // =========================================
            // COR SELECIONADA
            // =========================================

            string? corSelecionada = null;

            if (!string.IsNullOrWhiteSpace(cor))
            {
                var corInformada = cor.Trim();

                var produtoCor =
                    produto.Cores.FirstOrDefault(c =>
                        string.Equals(
                            c.Nome,
                            corInformada,
                            StringComparison.OrdinalIgnoreCase));

                if (produtoCor != null)
                {
                    corSelecionada = produtoCor.Nome;
                }
            }


            // =========================================
            // ENVIAR DADOS PARA A VIEW
            // =========================================

            ViewBag.Estoque = estoque;

            ViewBag.CorSelecionada =
                corSelecionada;


            return View(produto);
        }

        // =========================================
        // PROMOÇÕES
        // PRODUTOS + COMBOS NO MESMO CATÁLOGO
        // =========================================

        public async Task<IActionResult> Promocoes(
            string? publico,
            int? categoriaId,
            string? cor,
            string? tipo,
            string? pesquisa,
            string? ordenar,
            int pagina = 1)
        {
            // =========================================
            // PRODUTOS EM PROMOÇÃO
            // =========================================

            var produtosQuery = _context.Produtos
                .Include(p => p.Categoria)
                .Include(p => p.Marca)
                .Include(p => p.Imagens)
                .Include(p => p.Estoques)
                .Include(p => p.Cores)
                    .ThenInclude(c => c.Imagens)
                .Include(p => p.Cores)
                    .ThenInclude(c => c.ImagemPrincipal)
                .Where(p =>
                    p.Ativo &&
                    p.EmPromocao &&
                    p.PrecoPromocional != null)
                .AsQueryable();


            // =========================================
            // PESQUISA - PRODUTOS
            // =========================================

            if (!string.IsNullOrWhiteSpace(pesquisa))
            {
                produtosQuery = produtosQuery.Where(p =>
                    p.Nome.Contains(pesquisa));
            }


            // =========================================
            // FILTRO POR PÚBLICO - PRODUTOS
            // =========================================

            if (!string.IsNullOrWhiteSpace(publico))
            {
                produtosQuery = produtosQuery.Where(p =>
                    p.Publico == publico);
            }


            // =========================================
            // FILTRO POR CATEGORIA - PRODUTOS
            // =========================================

            if (categoriaId.HasValue)
            {
                produtosQuery = produtosQuery.Where(p =>
                    p.CategoriaId == categoriaId.Value);
            }


            // =========================================
            // FILTRO POR COR - PRODUTOS
            // =========================================
            if (!string.IsNullOrWhiteSpace(cor))
            {
                cor = cor.Trim();

                produtosQuery = produtosQuery.Where(p =>
                    p.Cores.Any(c =>
                        c.Nome == cor));
            }


            // =========================================
            // FILTRO POR TIPO
            // =========================================

            if (!string.IsNullOrWhiteSpace(tipo) &&
                tipo != "Combo")
            {
                produtosQuery = produtosQuery.Where(p =>
                    p.TipoPromocao == tipo);
            }


            var produtos = await produtosQuery.ToListAsync();

            var produtosPromocao =
                new List<LojaWeb_2.ViewModels.ProdutoCatalogoViewModel>();

            foreach (var produto in produtos)
            {
                var cores = produto.Cores
                     .Where(c => !string.IsNullOrWhiteSpace(c.Nome))
                     .Where(c =>
                         string.IsNullOrWhiteSpace(cor) ||
                         string.Equals(
                             c.Nome,
                             cor,
                             StringComparison.OrdinalIgnoreCase))
                     .ToList();

                if (cores.Any())
                {
                    foreach (var produtoCor in cores)
                    {
                        var imagem =
                            produtoCor.ImagemPrincipal?.Caminho
                            ??
                            produtoCor.Imagens
                                .OrderBy(i => i.Id)
                                .Select(i => i.Caminho)
                                .FirstOrDefault();

                        produtosPromocao.Add(
                            new LojaWeb_2.ViewModels.ProdutoCatalogoViewModel
                            {
                                Produto = produto,
                                Cor = produtoCor,
                                Imagem = imagem ?? produto.Imagem
                            });
                    }
                }
                else
                {
                    produtosPromocao.Add(
                        new LojaWeb_2.ViewModels.ProdutoCatalogoViewModel
                        {
                            Produto = produto,
                            Cor = null,
                            Imagem = produto.Imagem
                        });
                }
            }

            // =========================================
            // COMBOS EM PROMOÇÃO
            // =========================================

            var combosQuery = _context.Combos
                .Include(c => c.Itens)
                    .ThenInclude(i => i.Produto)
                .Where(c =>
                    c.Ativo &&
                    c.EmPromocao)
                .AsQueryable();


            // =========================================
            // PESQUISA - COMBOS
            // =========================================

            if (!string.IsNullOrWhiteSpace(pesquisa))
            {
                combosQuery = combosQuery.Where(c =>
                    c.Nome.Contains(pesquisa) ||
                    (c.Descricao != null &&
                     c.Descricao.Contains(pesquisa)));
            }


            // =========================================
            // FILTRO POR PÚBLICO - COMBOS
            // Usa os produtos que fazem parte do combo
            // =========================================

            if (!string.IsNullOrWhiteSpace(publico))
            {
                combosQuery = combosQuery.Where(c =>
                    c.Itens.Any(i =>
                        i.Produto != null &&
                        i.Produto.Publico == publico));
            }


            // =========================================
            // FILTRO POR CATEGORIA - COMBOS
            // Usa os produtos que fazem parte do combo
            // =========================================

            if (categoriaId.HasValue)
            {
                combosQuery = combosQuery.Where(c =>
                    c.Itens.Any(i =>
                        i.Produto != null &&
                        i.Produto.CategoriaId == categoriaId.Value));
            }


            // =========================================
            // FILTRO POR COR - COMBOS
            // Usa os produtos que fazem parte do combo
            // =========================================

            if (!string.IsNullOrWhiteSpace(cor))
            {
                combosQuery = combosQuery.Where(c =>
                    c.Itens.Any(i =>
                        i.Produto != null &&
                        i.Produto.Cor == cor));
            }


            // =========================================
            // FILTRO TIPO = COMBO
            // =========================================

            if (tipo == "Combo")
            {
                produtos.Clear();
            }


            var combos = await combosQuery.ToListAsync();


            // =========================================
            // MONTA CATÁLOGO ÚNICO
            // =========================================

            var ofertas = new List<LojaWeb_2.Models.PromocaoItemViewModel>();


            // =========================================
            // ADICIONA PRODUTOS
            // =========================================

            foreach (var item in produtosPromocao)
            {
                ofertas.Add(new LojaWeb_2.Models.PromocaoItemViewModel
                {
                    Produto = item.Produto,
                    Combo = null,
                    Cor = item.Cor
                });
            }

            // =========================================
            // ADICIONA COMBOS
            // =========================================

            foreach (var combo in combos)
            {
                ofertas.Add(new LojaWeb_2.Models.PromocaoItemViewModel
                {
                    Produto = null,
                    Combo = combo
                });
            }


            // =========================================
            // ORDENAÇÃO
            // =========================================

            switch (ordenar)
            {
                case "az":

                    ofertas = ofertas
                        .OrderBy(x =>
                            x.EhCombo
                                ? x.Combo!.Nome
                                : x.Produto!.Nome)
                        .ToList();

                    break;


                case "za":

                    ofertas = ofertas
                        .OrderByDescending(x =>
                            x.EhCombo
                                ? x.Combo!.Nome
                                : x.Produto!.Nome)
                        .ToList();

                    break;


                case "menor-preco":

                    ofertas = ofertas
                        .OrderBy(x =>
                            x.EhCombo
                                ? x.Combo!.Preco
                                : x.Produto!.PrecoPromocional ?? 0)
                        .ToList();

                    break;


                case "maior-preco":

                    ofertas = ofertas
                        .OrderByDescending(x =>
                            x.EhCombo
                                ? x.Combo!.Preco
                                : x.Produto!.PrecoPromocional ?? 0)
                        .ToList();

                    break;


                case "recentes":

                    ofertas = ofertas
                        .OrderByDescending(x =>
                            x.EhCombo
                                ? x.Combo!.Id
                                : x.Produto!.Id)
                        .ToList();

                    break;


                default:

                    ofertas = ofertas
                        .OrderByDescending(x =>
                            x.EhCombo
                                ? x.Combo!.Id
                                : x.Produto!.Id)
                        .ToList();

                    break;
            }

            // =========================================
            // PAGINAÇÃO
            // =========================================

            const int ofertasPorPagina = 12;

            int totalOfertas = ofertas.Count;

            int totalPaginas = (int)Math.Ceiling(
                totalOfertas / (double)ofertasPorPagina);

            if (pagina < 1)
            {
                pagina = 1;
            }

            if (totalPaginas > 0 && pagina > totalPaginas)
            {
                pagina = totalPaginas;
            }

            var ofertasPaginadas = ofertas
                .Skip((pagina - 1) * ofertasPorPagina)
                .Take(ofertasPorPagina)
                .ToList();

            // =========================================
            // CATEGORIAS PARA O FILTRO
            // =========================================

            ViewBag.Categorias = await _context.Categorias
                .OrderBy(c => c.Nome)
                .ToListAsync();


            // =========================================
            // CORES
            // Produtos + produtos que participam de combos
            // =========================================

            ViewBag.Cores = await _context.Produtos
                .Where(p =>
                    p.Ativo &&
                    (
                        p.EmPromocao ||
                        _context.ComboItens.Any(ci =>
                            ci.ProdutoId == p.Id &&
                            ci.Combo!.Ativo &&
                            ci.Combo.EmPromocao)
                    ) &&
                    !string.IsNullOrEmpty(p.Cor))
                .Select(p => p.Cor!)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            // =========================================
            // INFORMAÇÕES DA PAGINAÇÃO
            // =========================================
            ViewBag.PaginaAtual = pagina;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.TotalOfertas = totalOfertas;

            return View(ofertasPaginadas);
        }

        // =========================================
        // COMBOS - LOJA PÚBLICA
        // =========================================

        public async Task<IActionResult> Combos()
        {
            var combos = await _context.Combos
                .Include(c => c.Itens)
                    .ThenInclude(i => i.Produto)
                .Where(c => c.Ativo)
                .OrderByDescending(c => c.Id)
                .ToListAsync();

            return View(combos);
        }

        // =========================================
        // DETALHES DO COMBO - LOJA PÚBLICA
        // =========================================

        public async Task<IActionResult> DetalhesCombo(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var combo = await _context.Combos
                .Include(c => c.Itens)
                    .ThenInclude(i => i.Produto)
                        .ThenInclude(p => p!.Imagens)
                .FirstOrDefaultAsync(c =>
                    c.Id == id &&
                    c.Ativo);

            if (combo == null)
            {
                return NotFound();
            }

            return View(combo);
        }

        // =========================================
        // CATÁLOGO MASCULINO - LOJA PÚBLICA
        // =========================================
        public async Task<IActionResult> Masculino(
             int? categoriaId,
             int pagina = 1)
        {
            var produtosQuery = _context.Produtos
                .Include(p => p.Categoria)
                .Include(p => p.Marca)
                .Include(p => p.Imagens)
                .Include(p => p.Estoques)
                .Include(p => p.Cores)
                    .ThenInclude(c => c.Imagens)
                .Include(p => p.Cores)
                    .ThenInclude(c => c.ImagemPrincipal)
                .Where(p =>
                    p.Ativo &&
                    p.Publico == "Masculino")
                .AsQueryable();


            // =========================================
            // FILTRO POR CATEGORIA
            // =========================================

            if (categoriaId.HasValue)
            {
                produtosQuery = produtosQuery
                    .Where(p => p.CategoriaId == categoriaId.Value);
            }


            var produtos = await produtosQuery
                .OrderBy(p => p.Nome)
                .ToListAsync();


            // =========================================
            // MONTAR PRODUTO + COR + IMAGEM
            // =========================================

            var itensCatalogo =
                new List<ProdutoCatalogoViewModel>();

            foreach (var produto in produtos)
            {
                var cores = produto.Cores
                    .Where(c => !string.IsNullOrWhiteSpace(c.Nome))
                    .ToList();

                if (cores.Any())
                {
                    foreach (var produtoCor in cores)
                    {
                        var imagem =
                            produtoCor.ImagemPrincipal?.Caminho
                            ??
                            produtoCor.Imagens
                                .OrderBy(i => i.Id)
                                .Select(i => i.Caminho)
                                .FirstOrDefault();

                        itensCatalogo.Add(
                            new ProdutoCatalogoViewModel
                            {
                                Produto = produto,
                                Cor = produtoCor,
                                Imagem = imagem ?? produto.Imagem
                            });
                    }
                }
                else
                {
                    itensCatalogo.Add(
                        new ProdutoCatalogoViewModel
                        {
                            Produto = produto,
                            Cor = null,
                            Imagem = produto.Imagem
                        });
                }
            }


            // =========================================
            // PAGINAÇÃO
            // =========================================

            const int itensPorPagina = 12;

            var totalItens = itensCatalogo.Count;

            var totalPaginas =
                (int)Math.Ceiling(
                    totalItens / (double)itensPorPagina);


            if (pagina < 1)
                pagina = 1;

            if (totalPaginas > 0 && pagina > totalPaginas)
                pagina = totalPaginas;


            var itensPagina =
                itensCatalogo
                    .Skip((pagina - 1) * itensPorPagina)
                    .Take(itensPorPagina)
                    .ToList();


            // =========================================
            // INFORMAÇÕES PARA A VIEW
            // =========================================

            ViewBag.PaginaAtual = pagina;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.TotalProdutos = totalItens;
            ViewBag.CategoriaId = categoriaId;


            return View(itensPagina);
        }

        // =========================================
        // CATÁLOGO FEMININO - LOJA PÚBLICA
        // =========================================
        public async Task<IActionResult> Feminino(
            int? categoriaId,
            int pagina = 1)
        {
            var produtosQuery = _context.Produtos
                .Include(p => p.Categoria)
                .Include(p => p.Marca)
                .Include(p => p.Imagens)
                .Include(p => p.Estoques)
                .Include(p => p.Cores)
                    .ThenInclude(c => c.Imagens)
                .Include(p => p.Cores)
                    .ThenInclude(c => c.ImagemPrincipal)
                .Where(p =>
                    p.Ativo &&
                    p.Publico == "Feminino")
                .AsQueryable();


            // =========================================
            // FILTRO POR CATEGORIA
            // =========================================

            if (categoriaId.HasValue)
            {
                produtosQuery = produtosQuery
                    .Where(p => p.CategoriaId == categoriaId.Value);
            }


            var produtos = await produtosQuery
                .OrderBy(p => p.Nome)
                .ToListAsync();


            // =========================================
            // MONTAR PRODUTO + COR + IMAGEM
            // =========================================

            var itensCatalogo =
                new List<ProdutoCatalogoViewModel>();

            foreach (var produto in produtos)
            {
                var cores = produto.Cores
                    .Where(c => !string.IsNullOrWhiteSpace(c.Nome))
                    .ToList();

                if (cores.Any())
                {
                    foreach (var produtoCor in cores)
                    {
                        var imagem =
                            produtoCor.ImagemPrincipal?.Caminho
                            ??
                            produtoCor.Imagens
                                .OrderBy(i => i.Id)
                                .Select(i => i.Caminho)
                                .FirstOrDefault();

                        itensCatalogo.Add(
                            new ProdutoCatalogoViewModel
                            {
                                Produto = produto,
                                Cor = produtoCor,
                                Imagem = imagem ?? produto.Imagem
                            });
                    }
                }
                else
                {
                    itensCatalogo.Add(
                        new ProdutoCatalogoViewModel
                        {
                            Produto = produto,
                            Cor = null,
                            Imagem = produto.Imagem
                        });
                }
            }


            // =========================================
            // PAGINAÇÃO
            // =========================================

            const int itensPorPagina = 12;

            var totalItens = itensCatalogo.Count;

            var totalPaginas =
                (int)Math.Ceiling(
                    totalItens / (double)itensPorPagina);


            if (pagina < 1)
                pagina = 1;

            if (totalPaginas > 0 && pagina > totalPaginas)
                pagina = totalPaginas;


            var itensPagina =
                itensCatalogo
                    .Skip((pagina - 1) * itensPorPagina)
                    .Take(itensPorPagina)
                    .ToList();


            // =========================================
            // INFORMAÇÕES PARA A VIEW
            // =========================================

            ViewBag.PaginaAtual = pagina;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.TotalProdutos = totalItens;
            ViewBag.CategoriaId = categoriaId;


            return View(itensPagina);
        }

        // =========================================
        // PRODUTOS POR CATEGORIA - LOJA PÚBLICA
        // =========================================

        public async Task<IActionResult> Categoria(
            int categoriaId,
            string? publico)
        {
            var categoria = await _context.Categorias
                .FirstOrDefaultAsync(c => c.Id == categoriaId);

            if (categoria == null)
            {
                return NotFound();
            }

            var produtosQuery = _context.Produtos
                .Include(p => p.Categoria)
                .Include(p => p.Marca)
                .Include(p => p.Imagens)
                .Where(p =>
                    p.Ativo &&
                    p.CategoriaId == categoriaId);

            if (!string.IsNullOrWhiteSpace(publico))
            {
                produtosQuery = produtosQuery
                    .Where(p => p.Publico == publico);
            }

            var produtos = await produtosQuery
                .OrderBy(p => p.Nome)
                .ToListAsync();

            ViewBag.CategoriaNome = categoria.Nome;
            ViewBag.Publico = publico;

            return View(produtos);
        }
    }

}