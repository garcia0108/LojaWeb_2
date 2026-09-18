using LojaWeb_2.Data;
using LojaWeb_2.Models;
using LojaWeb_2.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LojaWeb_2.Controllers
{
    [Authorize]
    public class ProdutosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProdutosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Produtos
        public async Task<IActionResult> Index(
            string? pesquisa,
            int? categoriaId,
            int? marcaId,
            int? fornecedorId,
            string? situacao)
        {
            var query = _context.Produtos
                .Include(p => p.Categoria)
                .Include(p => p.Fornecedor)
                .Include(p => p.Marca)
                .AsQueryable();

            // PESQUISA POR NOME
            
            if (!string.IsNullOrWhiteSpace(pesquisa))
            {
                pesquisa = pesquisa.Trim();

                query = query.Where(p =>
                    p.Nome.Contains(pesquisa));
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
            // FILTRO POR MARCA
            // =========================================

            if (marcaId.HasValue)
            {
                query = query.Where(p =>
                    p.MarcaId == marcaId.Value);
            }


            // =========================================
            // FILTRO POR FORNECEDOR
            // =========================================

            if (fornecedorId.HasValue)
            {
                query = query.Where(p =>
                    p.FornecedorId == fornecedorId.Value);
            }


            // =========================================
            // FILTRO POR SITUAÇÃO
            // =========================================

            if (!string.IsNullOrWhiteSpace(situacao))
            {
                switch (situacao)
                {
                    case "Ativo":

                        query = query.Where(p =>
                            p.Ativo);

                        break;

                    case "Inativo":

                        query = query.Where(p =>
                            !p.Ativo);

                        break;
                }
            }


            // =========================================
            // BUSCAR PRODUTOS
            // =========================================

            var produtos = await query
                .OrderBy(p => p.Nome)
                .ToListAsync();


            // =========================================
            // DADOS PARA OS FILTROS
            // =========================================

            ViewBag.Categorias = await _context.Categorias
                .OrderBy(c => c.Nome)
                .ToListAsync();

            ViewBag.Marcas = await _context.Marcas
                .OrderBy(m => m.Nome)
                .ToListAsync();

            ViewBag.Fornecedores = await _context.Fornecedores
                .OrderBy(f => f.Nome)
                .ToListAsync();


            // =========================================
            // MANTER FILTROS NA VIEW
            // =========================================

            ViewBag.Pesquisa = pesquisa;
            ViewBag.CategoriaId = categoriaId;
            ViewBag.MarcaId = marcaId;
            ViewBag.FornecedorId = fornecedorId;
            ViewBag.Situacao = situacao;


            return View(produtos);
        }

        // GET: Produtos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var produto = await _context.Produtos
                 .Include(p => p.Categoria)
                 .Include(p => p.Fornecedor)
                 .Include(p => p.Marca)
                 .Include(p => p.Imagens)
                 .Include(p => p.Cores)
                     .ThenInclude(c => c.Imagens)
                 .FirstOrDefaultAsync(p => p.Id == id);

            if (produto == null)
                return NotFound();

            return View(produto);
        }

        // GET: Produtos/Create
        [Authorize(Roles = "Administrador")]
        public IActionResult Create()
        {
            ViewBag.CategoriaId = new SelectList(
                _context.Categorias.OrderBy(c => c.Nome),
                "Id",
                "Nome");

            ViewBag.MarcaId = new SelectList(
                _context.Marcas.OrderBy(m => m.Nome),
                "Id",
                "Nome");

            ViewBag.FornecedorId = new SelectList(
                _context.Fornecedores.OrderBy(f => f.Nome),
                "Id",
                "Nome");

            return View();
        }

        // POST: Produtos/Create
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
                Produto produto,
                List<IFormFile>? imagens,
                List<ProdutoCorCadastroViewModel>? cores)
        {
            // =========================================
            // VALIDAR IMAGENS
            // =========================================

            if (imagens != null && imagens.Any())
            {
                foreach (var imagem in imagens)
                {
                    if (!ImagemValida(imagem, out string mensagemErro))
                    {
                        ModelState.AddModelError(
                            "imagens",
                            mensagemErro);
                    }
                }
            }

            // =========================================
            // VALIDAR IMAGENS DAS CORES
            // =========================================

            if (cores != null && cores.Any())
            {
                foreach (var cor in cores)
                {
                    if (cor.Imagens == null)
                        continue;

                    foreach (var imagem in cor.Imagens)
                    {
                        if (!ImagemValida(imagem, out string mensagemErro))
                        {
                            ModelState.AddModelError(
                                "cores",
                                $"Cor {cor.Nome}: {mensagemErro}");
                        }
                    }
                }
            }

            if (ModelState.IsValid)
            {
                // =========================================
                // SALVAR PRODUTO PRIMEIRO
                // =========================================

                _context.Produtos.Add(produto);

                await _context.SaveChangesAsync();


                // =========================================
                // SALVAR AS IMAGENS
                // =========================================

                if (imagens != null && imagens.Any())
                {
                    string pasta = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        "uploads");

                    if (!Directory.Exists(pasta))
                    {
                        Directory.CreateDirectory(pasta);
                    }


                    for (int i = 0; i < imagens.Count; i++)
                    {
                        var imagem = imagens[i];

                        string extensao = Path
                            .GetExtension(imagem.FileName)
                            .ToLowerInvariant();

                        string nomeArquivo =
                            Guid.NewGuid().ToString() +
                            extensao;

                        string caminhoFisico =
                            Path.Combine(
                                pasta,
                                nomeArquivo);

                        using (var stream = new FileStream(
                            caminhoFisico,
                            FileMode.Create))
                        {
                            await imagem.CopyToAsync(stream);
                        }


                        string caminhoImagem =
                            "/uploads/" + nomeArquivo;


                        // =====================================
                        // PRIMEIRA IMAGEM = IMAGEM PRINCIPAL
                        // =====================================

                        if (i == 0)
                        {
                            produto.Imagem = caminhoImagem;
                        }


                        // =====================================
                        // SALVAR TODAS AS IMAGENS
                        // =====================================

                        var produtoImagem = new ProdutoImagem
                        {
                            ProdutoId = produto.Id,
                            Caminho = caminhoImagem
                        };

                        _context.ProdutoImagens.Add(
                            produtoImagem);
                    }

                    await _context.SaveChangesAsync();
                }

                // =========================================
                // SALVAR CORES E IMAGENS DAS CORES
                // =========================================

                if (cores != null && cores.Any())
                {
                    string pasta = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        "uploads");

                    if (!Directory.Exists(pasta))
                    {
                        Directory.CreateDirectory(pasta);
                    }

                    foreach (var corCadastro in cores)
                    {
                        // Ignora uma eventual cor vazia
                        if (string.IsNullOrWhiteSpace(corCadastro.Nome))
                            continue;

                        // =====================================
                        // CRIAR A COR
                        // =====================================

                        var produtoCor = new ProdutoCor
                        {
                            ProdutoId = produto.Id,
                            Nome = corCadastro.Nome.Trim()
                        };

                        _context.ProdutoCores.Add(produtoCor);

                        // Precisamos do Id da cor antes
                        // de cadastrar suas imagens
                        await _context.SaveChangesAsync();


                        // =====================================
                        // IMAGENS DA COR
                        // =====================================

                        if (corCadastro.Imagens != null &&
                            corCadastro.Imagens.Any())
                        {
                            foreach (var imagem in corCadastro.Imagens)
                            {
                                string extensao = Path
                                    .GetExtension(imagem.FileName)
                                    .ToLowerInvariant();

                                string nomeArquivo =
                                    Guid.NewGuid().ToString() +
                                    extensao;

                                string caminhoFisico =
                                    Path.Combine(
                                        pasta,
                                        nomeArquivo);

                                using (var stream = new FileStream(
                                    caminhoFisico,
                                    FileMode.Create))
                                {
                                    await imagem.CopyToAsync(stream);
                                }

                                string caminhoImagem =
                                    "/uploads/" + nomeArquivo;


                                var produtoImagem = new ProdutoImagem
                                {
                                    ProdutoId = produto.Id,

                                    // Aqui está a ligação nova
                                    ProdutoCorId = produtoCor.Id,

                                    Caminho = caminhoImagem
                                };

                                _context.ProdutoImagens.Add(
                                    produtoImagem);
                            }

                            await _context.SaveChangesAsync();
                        }
                    }
                }

                TempData["Sucesso"] =
                    "Produto cadastrado com sucesso!";

                return RedirectToAction(nameof(Index));
            }

            // =========================================
            // RECARREGAR SELECTS EM CASO DE ERRO
            // =========================================

            ViewBag.CategoriaId = new SelectList(
                _context.Categorias.OrderBy(c => c.Nome),
                "Id",
                "Nome",
                produto.CategoriaId);

            ViewBag.MarcaId = new SelectList(
                _context.Marcas.OrderBy(m => m.Nome),
                "Id",
                "Nome",
                produto.MarcaId);

            ViewBag.FornecedorId = new SelectList(
                _context.Fornecedores.OrderBy(f => f.Nome),
                "Id",
                "Nome",
                produto.FornecedorId);

            return View(produto);
        }

        // GET: Produtos/Edit/5
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var produto = await _context.Produtos
                 .Include(p => p.Imagens)
                 .Include(p => p.Cores)
                     .ThenInclude(c => c.Imagens)
                 .FirstOrDefaultAsync(p => p.Id == id);

            if (produto == null)
                return NotFound();

            ViewBag.CategoriaId = new SelectList(
                _context.Categorias.OrderBy(c => c.Nome),
                "Id",
                "Nome",
                produto.CategoriaId);

            ViewBag.MarcaId = new SelectList(
                _context.Marcas.OrderBy(m => m.Nome),
                "Id",
                "Nome",
                produto.MarcaId);

            ViewBag.FornecedorId = new SelectList(
                _context.Fornecedores.OrderBy(f => f.Nome),
                "Id",
                "Nome",
                produto.FornecedorId);

            await CarregarCategorias();
            await CarregarMarcas(
                produto.MarcaId);

            return View(produto);
        }

        // POST: Produtos/Edit/5
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Produto produto,
            List<IFormFile>? imagens)
        {
            if (id != produto.Id)
                return NotFound();


            // =========================================
            // VALIDAR AS NOVAS IMAGENS
            // =========================================

            if (imagens != null && imagens.Any())
            {
                foreach (var imagem in imagens)
                {
                    if (!ImagemValida(
                        imagem,
                        out string mensagemErro))
                    {
                        ModelState.AddModelError(
                            "imagens",
                            mensagemErro);
                    }
                }
            }


            if (ModelState.IsValid)
            {
                try
                {
                    // =========================================
                    // BUSCA O PRODUTO ORIGINAL
                    // =========================================

                    var produtoBanco =
                        await _context.Produtos
                            .Include(p => p.Imagens)
                            .FirstOrDefaultAsync(
                                p => p.Id == produto.Id);

                    if (produtoBanco == null)
                        return NotFound();


                    // =========================================
                    // ATUALIZA OS DADOS DO PRODUTO
                    // =========================================

                    produtoBanco.Nome = produto.Nome;
                    produtoBanco.Descricao = produto.Descricao;
                    produtoBanco.Preco = produto.Preco;
                    produtoBanco.EmPromocao = produto.EmPromocao;
                    produtoBanco.PrecoPromocional = produto.PrecoPromocional;
                    produtoBanco.TipoPromocao = produto.TipoPromocao;
                    produtoBanco.QuantidadeMinimaPromocao = produto.QuantidadeMinimaPromocao;
                    produtoBanco.PrecoQuantidadePromocional = produto.PrecoQuantidadePromocional;
                    produtoBanco.Tamanho = produto.Tamanho;
                    produtoBanco.Cor = produto.Cor;
                    produtoBanco.Ativo = produto.Ativo;
                    produtoBanco.Publico = produto.Publico;
                    produtoBanco.Destaque = produto.Destaque;
                    produtoBanco.CategoriaId = produto.CategoriaId;
                    produtoBanco.MarcaId = produto.MarcaId;
                    produtoBanco.FornecedorId = produto.FornecedorId;


                    // =========================================
                    // ADICIONAR NOVAS IMAGENS
                    // =========================================

                    if (imagens != null && imagens.Any())
                    {
                        string pasta = Path.Combine(
                            Directory.GetCurrentDirectory(),
                            "wwwroot",
                            "uploads");

                        if (!Directory.Exists(pasta))
                        {
                            Directory.CreateDirectory(pasta);
                        }


                        foreach (var imagem in imagens)
                        {
                            string extensao = Path
                                .GetExtension(imagem.FileName)
                                .ToLowerInvariant();

                            string nomeArquivo =
                                Guid.NewGuid().ToString() +
                                extensao;

                            string caminhoCompleto =
                                Path.Combine(
                                    pasta,
                                    nomeArquivo);

                            using (var stream =
                                new FileStream(
                                    caminhoCompleto,
                                    FileMode.Create))
                            {
                                await imagem.CopyToAsync(stream);
                            }


                            string caminhoImagem =
                                "/uploads/" + nomeArquivo;


                            // =====================================
                            // SE NÃO EXISTE IMAGEM PRINCIPAL,
                            // ESTA VIRA A PRINCIPAL
                            // =====================================

                            if (string.IsNullOrEmpty(
                                produtoBanco.Imagem))
                            {
                                produtoBanco.Imagem =
                                    caminhoImagem;
                            }


                            // =====================================
                            // ADICIONA À GALERIA
                            // =====================================

                            var produtoImagem =
                                new ProdutoImagem
                                {
                                    ProdutoId =
                                        produtoBanco.Id,

                                    Caminho =
                                        caminhoImagem
                                };

                            _context.ProdutoImagens.Add(
                                produtoImagem);
                        }
                    }


                    await _context.SaveChangesAsync();


                    TempData["Sucesso"] =
                        "Produto atualizado com sucesso!";

                    return RedirectToAction(
                        nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProdutoExists(produto.Id))
                    {
                        return NotFound();
                    }

                    throw;
                }
            }


            // =========================================
            // RECARREGAR SELECTS
            // =========================================

            ViewBag.CategoriaId =
                new SelectList(
                    _context.Categorias.OrderBy(
                        c => c.Nome),
                    "Id",
                    "Nome",
                    produto.CategoriaId);

            ViewBag.MarcaId =
                new SelectList(
                    _context.Marcas.OrderBy(
                        m => m.Nome),
                    "Id",
                    "Nome",
                    produto.MarcaId);

            ViewBag.FornecedorId =
                new SelectList(
                    _context.Fornecedores.OrderBy(
                        f => f.Nome),
                    "Id",
                    "Nome",
                    produto.FornecedorId);

            return View(produto);
        }

        // POST: Produtos/AdicionarImagensCor
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdicionarImagensCor(
            int produtoId,
            int produtoCorId,
            List<IFormFile>? imagens)
        {
            // =========================================
            // VALIDAR PRODUTO
            // =========================================

            var produto = await _context.Produtos
                .FirstOrDefaultAsync(p => p.Id == produtoId);

            if (produto == null)
                return NotFound();


            // =========================================
            // VALIDAR COR
            // =========================================

            var produtoCor = await _context.ProdutoCores
                .FirstOrDefaultAsync(c =>
                    c.Id == produtoCorId &&
                    c.ProdutoId == produtoId);

            if (produtoCor == null)
                return NotFound();


            // =========================================
            // VERIFICAR SE FORAM ENVIADAS IMAGENS
            // =========================================

            if (imagens == null || !imagens.Any())
            {
                TempData["Erro"] =
                    "Selecione pelo menos uma imagem.";

                return RedirectToAction(
                    nameof(Edit),
                    new { id = produtoId });
            }


            // =========================================
            // VALIDAR IMAGENS
            // =========================================

            foreach (var imagem in imagens)
            {
                if (!ImagemValida(
                        imagem,
                        out string mensagemErro))
                {
                    TempData["Erro"] = mensagemErro;

                    return RedirectToAction(
                        nameof(Edit),
                        new { id = produtoId });
                }
            }


            // =========================================
            // GARANTIR PASTA DE UPLOADS
            // =========================================

            string pasta = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "uploads");

            if (!Directory.Exists(pasta))
            {
                Directory.CreateDirectory(pasta);
            }


            // =========================================
            // SALVAR IMAGENS
            // =========================================

            foreach (var imagem in imagens)
            {
                string extensao = Path
                    .GetExtension(imagem.FileName)
                    .ToLowerInvariant();

                string nomeArquivo =
                    Guid.NewGuid().ToString() +
                    extensao;

                string caminhoFisico =
                    Path.Combine(
                        pasta,
                        nomeArquivo);

                using (var stream =
                    new FileStream(
                        caminhoFisico,
                        FileMode.Create))
                {
                    await imagem.CopyToAsync(stream);
                }

                string caminhoImagem =
                    "/uploads/" + nomeArquivo;


                var produtoImagem =
                    new ProdutoImagem
                    {
                        ProdutoId = produtoId,
                        ProdutoCorId = produtoCorId,
                        Caminho = caminhoImagem
                    };

                _context.ProdutoImagens.Add(
                    produtoImagem);
            }


            await _context.SaveChangesAsync();


            // =========================================
            // MENSAGEM
            // =========================================

            TempData["Sucesso"] =
                $"Imagem(ns) adicionada(s) à cor {produtoCor.Nome} com sucesso!";


            return RedirectToAction(
                nameof(Edit),
                new { id = produtoId });
        }

        // POST: Produtos/AdicionarCor
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdicionarCor(
            int produtoId,
            string nome,
            List<IFormFile>? imagens)
        {
            // =========================================
            // VALIDAR PRODUTO
            // =========================================

            var produto = await _context.Produtos
                .FirstOrDefaultAsync(p => p.Id == produtoId);

            if (produto == null)
                return NotFound();


            // =========================================
            // VALIDAR NOME DA COR
            // =========================================

            if (string.IsNullOrWhiteSpace(nome))
            {
                TempData["Erro"] =
                    "Informe o nome da cor.";

                return RedirectToAction(
                    nameof(Edit),
                    new { id = produtoId });
            }

            nome = nome.Trim();


            // =========================================
            // VERIFICAR COR DUPLICADA
            // =========================================

            var corExistente =
                await _context.ProdutoCores
                    .AnyAsync(c =>
                        c.ProdutoId == produtoId &&
                        c.Nome.ToLower() == nome.ToLower());

            if (corExistente)
            {
                TempData["Erro"] =
                    "Essa cor já está cadastrada para este produto.";

                return RedirectToAction(
                    nameof(Edit),
                    new { id = produtoId });
            }


            // =========================================
            // CRIAR A COR
            // =========================================

            var produtoCor = new ProdutoCor
            {
                ProdutoId = produtoId,
                Nome = nome
            };

            _context.ProdutoCores.Add(produtoCor);

            await _context.SaveChangesAsync();


            // =========================================
            // VERIFICAR IMAGENS
            // =========================================

            if (imagens != null && imagens.Any())
            {
                foreach (var imagem in imagens)
                {
                    if (!ImagemValida(
                            imagem,
                            out string mensagemErro))
                    {
                        TempData["Erro"] = mensagemErro;

                        return RedirectToAction(
                            nameof(Edit),
                            new { id = produtoId });
                    }
                }


                // =========================================
                // GARANTIR PASTA DE UPLOADS
                // =========================================

                string pasta = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "uploads");

                if (!Directory.Exists(pasta))
                {
                    Directory.CreateDirectory(pasta);
                }


                // =========================================
                // SALVAR IMAGENS
                // =========================================

                foreach (var imagem in imagens)
                {
                    string extensao = Path
                        .GetExtension(imagem.FileName)
                        .ToLowerInvariant();

                    string nomeArquivo =
                        Guid.NewGuid().ToString() +
                        extensao;

                    string caminhoFisico =
                        Path.Combine(
                            pasta,
                            nomeArquivo);

                    using (var stream =
                        new FileStream(
                            caminhoFisico,
                            FileMode.Create))
                    {
                        await imagem.CopyToAsync(stream);
                    }

                    string caminhoImagem =
                        "/uploads/" + nomeArquivo;


                    var produtoImagem =
                        new ProdutoImagem
                        {
                            ProdutoId = produtoId,
                            ProdutoCorId = produtoCor.Id,
                            Caminho = caminhoImagem
                        };

                    _context.ProdutoImagens.Add(
                        produtoImagem);
                }


                await _context.SaveChangesAsync();
            }


            // =========================================
            // MENSAGEM
            // =========================================

            TempData["Sucesso"] =
                $"A cor {nome} foi adicionada com sucesso!";


            return RedirectToAction(
                nameof(Edit),
                new { id = produtoId });
        }

        // =========================================
        // EXCLUIR IMAGEM DE UMA COR
        // =========================================
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExcluirImagemCor(
            int id,
            int produtoId)
        {
            // =========================================
            // BUSCAR A IMAGEM
            // =========================================

            var imagem = await _context.ProdutoImagens
                .FirstOrDefaultAsync(i => i.Id == id);

            if (imagem == null)
                return NotFound();


            // =========================================
            // CONFIRMAR QUE A IMAGEM PERTENCE AO PRODUTO
            // =========================================

            if (imagem.ProdutoId != produtoId)
                return NotFound();


            // =========================================
            // CONFIRMAR QUE É UMA IMAGEM DE COR
            // =========================================

            if (!imagem.ProdutoCorId.HasValue)
            {
                TempData["Erro"] =
                    "Esta imagem não está vinculada a uma cor.";

                return RedirectToAction(
                    nameof(Edit),
                    new { id = produtoId });
            }


            // =========================================
            // CAMINHO FÍSICO DO ARQUIVO
            // =========================================

            string caminhoRelativo = imagem.Caminho;

            string caminhoFisico = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                caminhoRelativo.TrimStart('/')
                    .Replace("/", Path.DirectorySeparatorChar.ToString())
            );


            // =========================================
            // EXCLUIR DO BANCO
            // =========================================

            _context.ProdutoImagens.Remove(imagem);

            await _context.SaveChangesAsync();


            // =========================================
            // EXCLUIR ARQUIVO FÍSICO
            // =========================================

            if (System.IO.File.Exists(caminhoFisico))
            {
                System.IO.File.Delete(caminhoFisico);
            }


            // =========================================
            // MENSAGEM
            // =========================================

            TempData["Sucesso"] =
                "Imagem da cor excluída com sucesso!";


            return RedirectToAction(
                nameof(Edit),
                new { id = produtoId });
        }

        // GET: Produtos/Delete/5
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var produto = await _context.Produtos
                .Include(p => p.Categoria)
                .Include(p => p.Marca)
                .FirstOrDefaultAsync(
                    p => p.Id == id);

            if (produto == null)
                return NotFound();

            return View(produto);
        }

        // =========================================
        // POST: Produtos/Delete/5
        // =========================================
        [Authorize(Roles = "Administrador")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // =========================================
            // BUSCAR PRODUTO
            // =========================================

            var produto = await _context.Produtos
                .Include(p => p.Imagens)
                .Include(p => p.Cores)
                    .ThenInclude(c => c.Imagens)
                .Include(p => p.Estoques)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (produto == null)
                return NotFound();


            // =========================================
            // VERIFICAR HISTÓRICO DE VENDAS
            // =========================================

            bool possuiVenda =
                await _context.ItensVenda
                    .AnyAsync(i => i.ProdutoId == id);

            if (possuiVenda)
            {
                TempData["Erro"] =
                    "Este produto possui histórico de vendas e não pode ser excluído. " +
                    "Desative o produto para que ele não seja mais vendido.";

                return RedirectToAction(
                    nameof(Delete),
                    new { id });
            }


            // =========================================
            // VERIFICAR MOVIMENTAÇÕES DE ESTOQUE
            // =========================================

            bool possuiMovimentacao =
                await _context.MovimentacoesEstoque
                    .AnyAsync(m => m.ProdutoId == id);

            if (possuiMovimentacao)
            {
                TempData["Erro"] =
                    "Este produto possui histórico de movimentações de estoque " +
                    "e não pode ser excluído. Desative o produto.";

                return RedirectToAction(
                    nameof(Delete),
                    new { id });
            }


            // =========================================
            // EXCLUIR ITENS DO CARRINHO
            // =========================================

            var itensCarrinho =
                await _context.CarrinhoItens
                    .Where(c => c.ProdutoId == id)
                    .ToListAsync();

            if (itensCarrinho.Any())
            {
                _context.CarrinhoItens.RemoveRange(
                    itensCarrinho);
            }


            // =========================================
            // EXCLUIR IMAGENS DAS CORES
            // =========================================

            var imagensCores =
                produto.Cores
                    .SelectMany(c => c.Imagens)
                    .ToList();

            foreach (var imagem in imagensCores)
            {
                ExcluirArquivoImagem(imagem.Caminho);
            }

            if (imagensCores.Any())
            {
                _context.ProdutoImagens.RemoveRange(
                    imagensCores);
            }


            // =========================================
            // EXCLUIR CORES
            // =========================================

            if (produto.Cores.Any())
            {
                _context.ProdutoCores.RemoveRange(
                    produto.Cores);
            }


            // =========================================
            // EXCLUIR IMAGENS GERAIS
            // =========================================

            foreach (var imagem in produto.Imagens)
            {
                ExcluirArquivoImagem(imagem.Caminho);
            }

            if (produto.Imagens.Any())
            {
                _context.ProdutoImagens.RemoveRange(
                    produto.Imagens);
            }


            // =========================================
            // EXCLUIR ESTOQUES
            // =========================================

            if (produto.Estoques.Any())
            {
                _context.Estoques.RemoveRange(
                    produto.Estoques);
            }


            // =========================================
            // EXCLUIR IMAGEM PRINCIPAL
            // =========================================

            if (!string.IsNullOrWhiteSpace(produto.Imagem))
            {
                ExcluirArquivoImagem(
                    produto.Imagem);
            }


            // =========================================
            // EXCLUIR PRODUTO
            // =========================================

            _context.Produtos.Remove(produto);


            // =========================================
            // SALVAR
            // =========================================

            await _context.SaveChangesAsync();


            TempData["Sucesso"] =
                "Produto excluído com sucesso!";


            return RedirectToAction(
                nameof(Index));
        }

        // =========================================
        // EXCLUIR ARQUIVO FÍSICO DA IMAGEM
        // =========================================
        private void ExcluirArquivoImagem(string? caminhoImagem)
        {
            if (string.IsNullOrWhiteSpace(caminhoImagem))
                return;

            string caminho =
                Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    caminhoImagem.TrimStart('/'));

            if (System.IO.File.Exists(caminho))
            {
                System.IO.File.Delete(caminho);
            }
        }

        // CARREGAR CATEGORIAS
        private async Task CarregarCategorias(
            int? categoriaId = null)
        {
            ViewBag.CategoriaId = new SelectList(
                await _context.Categorias
                    .OrderBy(c => c.Nome)
                    .ToListAsync(),
                "Id",
                "Nome",
                categoriaId);
        }

        // CARREGAR MARCAS
        private async Task CarregarMarcas(
            int? marcaId = null)
        {
            ViewBag.MarcaId = new SelectList(
                await _context.Marcas
                    .OrderBy(m => m.Nome)
                    .ToListAsync(),
                "Id",
                "Nome",
                marcaId);
        }

        // =========================================
        // VALIDAR IMAGEM
        // =========================================

        private bool ImagemValida(
            IFormFile imagem,
            out string mensagemErro)
        {
            mensagemErro = string.Empty;

            var extensoesPermitidas = new[]
            {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    };

            var extensao = Path
                .GetExtension(imagem.FileName)
                .ToLowerInvariant();

            if (!extensoesPermitidas.Contains(extensao))
            {
                mensagemErro =
                    "Formato de imagem não permitido. " +
                    "Utilize JPG, JPEG, PNG ou WEBP.";

                return false;
            }

            // Limite de 5 MB
            if (imagem.Length > 5 * 1024 * 1024)
            {
                mensagemErro =
                    "A imagem deve ter no máximo 5 MB.";

                return false;
            }

            return true;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DefinirImagemPrincipalCor(
                int id,
                int produtoId,
                int produtoCorId)
        {
            var produto = await _context.Produtos
                .FirstOrDefaultAsync(p => p.Id == produtoId);

            if (produto == null)
            {
                return NotFound();
            }

            var cor = await _context.ProdutoCores
                .Include(c => c.Imagens)
                .FirstOrDefaultAsync(c =>
                    c.Id == produtoCorId &&
                    c.ProdutoId == produtoId);

            if (cor == null)
            {
                TempData["Erro"] = "Cor não encontrada.";
                return RedirectToAction(nameof(Edit), new { id = produtoId });
            }

            var imagem = cor.Imagens
                .FirstOrDefault(i => i.Id == id);

            if (imagem == null)
            {
                TempData["Erro"] = "Imagem não encontrada.";
                return RedirectToAction(nameof(Edit), new { id = produtoId });
            }

            cor.ImagemPrincipalId = imagem.Id;

            await _context.SaveChangesAsync();

            TempData["Sucesso"] =
                $"A imagem principal da cor {cor.Nome} foi definida com sucesso.";

            return RedirectToAction(nameof(Edit), new { id = produtoId });
        }

        // =========================================
        // DEFINIR IMAGEM PRINCIPAL
        // =========================================
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DefinirImagemPrincipal(
            int id,
            int produtoId)
        {
            var imagem = await _context.ProdutoImagens
                .FirstOrDefaultAsync(i => i.Id == id);

            if (imagem == null)
                return NotFound();

            // Confirma se a imagem pertence ao produto
            if (imagem.ProdutoId != produtoId)
                return NotFound();

            var produto = await _context.Produtos
                .FirstOrDefaultAsync(p => p.Id == produtoId);

            if (produto == null)
                return NotFound();

            // Define esta imagem como principal
            produto.Imagem = imagem.Caminho;

            await _context.SaveChangesAsync();

            TempData["Sucesso"] =
                "Imagem principal definida com sucesso!";

            return RedirectToAction(
                nameof(Edit),
                new { id = produtoId });
        }

        // =========================================
        // EXCLUIR UMA IMAGEM DO PRODUTO
        // =========================================
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExcluirImagem(
            int id,
            int produtoId)
        {
            var imagem = await _context.ProdutoImagens
                .FirstOrDefaultAsync(i => i.Id == id);

            if (imagem == null)
                return NotFound();

            // Segurança: verifica se a imagem pertence ao produto
            if (imagem.ProdutoId != produtoId)
                return NotFound();

            // Caminho físico da imagem
            var caminhoFisico = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                imagem.Caminho.TrimStart('/'));

            // Remove o arquivo físico
            if (System.IO.File.Exists(caminhoFisico))
            {
                System.IO.File.Delete(caminhoFisico);
            }

            // Remove o registro do banco
            _context.ProdutoImagens.Remove(imagem);

            await _context.SaveChangesAsync();

            TempData["Sucesso"] =
                "Imagem excluída com sucesso!";

            return RedirectToAction(
                nameof(Edit),
                new { id = produtoId });
        }

        // VERIFICAR PRODUTO
        private bool ProdutoExists(int id)
        {
            return _context.Produtos
                .Any(e => e.Id == id);
        }

        // GET: Loja/Promocoes
        public async Task<IActionResult> Promocoes(
            string? publico,
            int? categoriaId,
            string? cor)
        {
            var query = _context.Produtos
                .Include(p => p.Categoria)
                .Include(p => p.Marca)
                .Include(p => p.Imagens)
                .Where(p =>
                    p.Ativo &&
                    p.EmPromocao &&
                    p.PrecoPromocional != null)
                .AsQueryable();


            // FILTRO POR PÚBLICO
            if (!string.IsNullOrWhiteSpace(publico))
            {
                query = query.Where(p => p.Publico == publico);
            }


            // FILTRO POR CATEGORIA
            if (categoriaId.HasValue)
            {
                query = query.Where(p =>
                    p.CategoriaId == categoriaId.Value);
            }


            // FILTRO POR COR
            if (!string.IsNullOrWhiteSpace(cor))
            {
                query = query.Where(p => p.Cor == cor);
            }


            var produtos = await query
                .OrderByDescending(p => p.Id)
                .ToListAsync();


            // CATEGORIAS PARA O FILTRO
            ViewBag.Categorias = await _context.Categorias
                .OrderBy(c => c.Nome)
                .ToListAsync();


            // CORES EXISTENTES NOS PRODUTOS EM PROMOÇÃO
            ViewBag.Cores = await _context.Produtos
                .Where(p =>
                    p.Ativo &&
                    p.EmPromocao &&
                    p.PrecoPromocional != null &&
                    !string.IsNullOrEmpty(p.Cor))
                .Select(p => p.Cor!)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();


            return View(produtos);
        }
    }
}