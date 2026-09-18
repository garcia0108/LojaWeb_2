using LojaWeb_2.Data;
using LojaWeb_2.Models;
using LojaWeb_2.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


namespace LojaWeb_2.Controllers
{
    [Authorize(Roles = "Cliente")]
    public class CarrinhoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly FreteService _freteService;

        public CarrinhoController(
            ApplicationDbContext context,
            FreteService freteService)
        {
            _context = context;
            _freteService = freteService;
        }


        // =========================================
        // OBTER CLIENTE LOGADO
        // =========================================
        private async Task<Cliente?> ObterClienteLogado()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrEmpty(email))
            {
                return null;
            }

            return await _context.Clientes
                .FirstOrDefaultAsync(c =>
                    c.Email == email);
        }


        // =========================================
        // GET: VISUALIZAR CARRINHO
        // =========================================
        public async Task<IActionResult> Index(
                bool compraRapida = false,
                int? produtoId = null,
                string? cor = null,
                string? tamanho = null)
        {
            var cliente =
                await ObterClienteLogado();

            if (cliente == null)
            {
                return Redirect(
                    "/Identity/Account/Login");
            }

            var carrinho =
                await ObterCarrinho(cliente.Id);


            // =========================================
            // COMPRA RÁPIDA
            // =========================================

            if (compraRapida &&
                produtoId.HasValue &&
                !string.IsNullOrWhiteSpace(cor) &&
                !string.IsNullOrWhiteSpace(tamanho))
            {
                cor = cor.Trim();
                tamanho = tamanho.Trim();

                var produtoCompraRapida =
                    await _context.Produtos
                        .FirstOrDefaultAsync(p =>
                            p.Id == produtoId.Value &&
                            p.Ativo);

                if (produtoCompraRapida == null)
                {
                    TempData["Erro"] =
                        "Produto não encontrado.";

                    return RedirectToAction(
                        nameof(Index));
                }


                // =========================================
                // VERIFICAR ESTOQUE DA VARIANTE
                // =========================================

                var estoqueSelecionado =
                    await _context.Estoques
                        .FirstOrDefaultAsync(e =>
                            e.ProdutoId == produtoId.Value &&
                            e.Cor == cor &&
                            e.Tamanho == tamanho &&
                            e.Quantidade > 0);


                if (estoqueSelecionado == null)
                {
                    TempData["Erro"] =
                        "A cor e o tamanho selecionados não estão mais disponíveis.";

                    return RedirectToAction(
                        nameof(Index));
                }


                // =========================================
                // ENVIAR COMPRA RÁPIDA PARA A VIEW
                // =========================================

                ViewBag.CompraRapida = true;

                ViewBag.CompraRapidaProdutoId =
                    produtoCompraRapida.Id;

                ViewBag.CompraRapidaProduto =
                    produtoCompraRapida;

                ViewBag.CompraRapidaCor =
                    cor;

                ViewBag.CompraRapidaTamanho =
                    tamanho;

                ViewBag.CompraRapidaEstoque =
                    estoqueSelecionado.Quantidade;
            }


            // =========================================
            // BUSCAR FRETE SELECIONADO
            // =========================================

            var cotacaoFrete =
                await _context.CotacoesFrete
                    .Where(c =>
                        c.ClienteId == cliente.Id)
                    .OrderByDescending(c =>
                        c.DataCotacao)
                    .FirstOrDefaultAsync();


            // =========================================
            // ENVIAR FRETE PARA A VIEW
            // =========================================

            ViewBag.CotacaoFrete =
                cotacaoFrete;


            return View(carrinho);
        }


        // =========================================
        // POST: ADICIONAR AO CARRINHO
        // =========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Adicionar(
            int produtoId,
            string cor,
            string tamanho,
            int quantidade)
        {
            var cliente =
                await ObterClienteLogado();

            if (cliente == null)
            {
                return Unauthorized();
            }


            if (quantidade <= 0)
            {
                return BadRequest();
            }


            var produto = await _context.Produtos
                .FirstOrDefaultAsync(p =>
                    p.Id == produtoId &&
                    p.Ativo);

            if (produto == null)
            {
                return NotFound();
            }


            // =====================================
            // VERIFICAR ESTOQUE
            // =====================================

            var estoque = await _context.Estoques
                .FirstOrDefaultAsync(e =>
                    e.ProdutoId == produtoId &&
                    e.Cor == cor &&
                    e.Tamanho == tamanho);


            if (estoque == null)
            {
                TempData["Erro"] =
                    "A combinação selecionada não está disponível.";

                return RedirectToAction(
                    "Detalhes",
                    "Loja",
                    new { id = produtoId });
            }


            if (quantidade > estoque.Quantidade)
            {
                TempData["Erro"] =
                    $"Quantidade insuficiente. " +
                    $"Disponível: {estoque.Quantidade}.";

                return RedirectToAction(
                    "Detalhes",
                    "Loja",
                    new { id = produtoId });
            }


            // =====================================
            // VERIFICAR ITEM EXISTENTE
            // =====================================

            var itemExistente =
                await _context.CarrinhoItens
                    .FirstOrDefaultAsync(i =>
                        i.ClienteId == cliente.Id &&
                        i.ProdutoId == produtoId &&
                        i.Cor == cor &&
                        i.Tamanho == tamanho);


            if (itemExistente != null)
            {
                int novaQuantidade =
                    itemExistente.Quantidade +
                    quantidade;


                if (novaQuantidade > estoque.Quantidade)
                {
                    TempData["Erro"] =
                        $"Quantidade insuficiente. " +
                        $"Disponível: {estoque.Quantidade}.";

                    return RedirectToAction(
                        "Detalhes",
                        "Loja",
                        new { id = produtoId });
                }


                itemExistente.Quantidade =
                    novaQuantidade;

                itemExistente.PrecoUnitario =
                        produto.EmPromocao &&
                        produto.PrecoPromocional.HasValue &&
                        produto.PrecoPromocional.Value > 0 &&
                        produto.PrecoPromocional.Value < produto.Preco
                            ? produto.PrecoPromocional.Value
                            : produto.Preco;
            }
            else
            {
                _context.CarrinhoItens.Add(
                    new CarrinhoItem
                    {
                        ClienteId = cliente.Id,
                        ProdutoId = produto.Id,
                        Cor = cor,
                        Tamanho = tamanho,
                        Quantidade = quantidade,
                        PrecoUnitario =
                        produto.EmPromocao &&
                        produto.PrecoPromocional.HasValue &&
                        produto.PrecoPromocional.Value > 0 &&
                        produto.PrecoPromocional.Value < produto.Preco
                            ? produto.PrecoPromocional.Value
                            : produto.Preco,
                        DataAdicao = DateTime.Now
                    });
            }


            await _context.SaveChangesAsync();


            TempData["Sucesso"] =
                "Produto adicionado ao carrinho!";


            return RedirectToAction(nameof(Index));
        }

        // =========================================
        // COMPRA RÁPIDA
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompraRapida(
            int produtoId,
            string cor,
            string tamanho,
            int quantidade = 1)
        {
            var cliente = await ObterClienteLogado();

            if (cliente == null)
                return Unauthorized();

            if (quantidade <= 0)
                quantidade = 1;

            cor = cor?.Trim() ?? string.Empty;
            tamanho = tamanho?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(cor) ||
                string.IsNullOrWhiteSpace(tamanho))
            {
                TempData["Erro"] =
                    "Selecione a cor e o tamanho do produto.";

                return RedirectToAction(
                    "Produtos",
                    "Loja");
            }

            // =========================================
            // LOCALIZAR PRODUTO
            // =========================================

            var produto = await _context.Produtos
                .FirstOrDefaultAsync(p =>
                    p.Id == produtoId &&
                    p.Ativo);

            if (produto == null)
            {
                TempData["Erro"] =
                    "Produto não encontrado.";

                return RedirectToAction(
                    "Produtos",
                    "Loja");
            }

            // =========================================
            // LOCALIZAR ESTOQUE DA VARIANTE
            // =========================================

            var estoque = await _context.Estoques
                .FirstOrDefaultAsync(e =>
                    e.ProdutoId == produtoId &&
                    e.Cor == cor &&
                    e.Tamanho == tamanho);

            if (estoque == null)
            {
                TempData["Erro"] =
                    "A combinação de cor e tamanho não está disponível.";

                return RedirectToAction(
                    "Detalhes",
                    "Loja",
                    new { id = produtoId });
            }

            // =========================================
            // VERIFICAR ESTOQUE
            // =========================================

            if (estoque.Quantidade < quantidade)
            {
                TempData["Erro"] =
                    $"Quantidade insuficiente. Disponível: {estoque.Quantidade}.";

                return RedirectToAction(
                    "Detalhes",
                    "Loja",
                    new { id = produtoId });
            }

            // =========================================
            // VERIFICAR SE JÁ EXISTE NO CARRINHO
            // =========================================

            var itemExistente = await _context.CarrinhoItens
                .FirstOrDefaultAsync(i =>
                    i.ClienteId == cliente.Id &&
                    i.ProdutoId == produtoId &&
                    i.Cor == cor &&
                    i.Tamanho == tamanho);

            if (itemExistente != null)
            {
                int novaQuantidade =
                    itemExistente.Quantidade + quantidade;

                if (novaQuantidade > estoque.Quantidade)
                {
                    TempData["Erro"] =
                        $"Quantidade insuficiente. Disponível: {estoque.Quantidade}.";

                    return RedirectToAction(
                        "Detalhes",
                        "Loja",
                        new { id = produtoId });
                }

                itemExistente.Quantidade = novaQuantidade;
            }
            else
            {
                // =========================================
                // ADICIONAR NOVO ITEM
                // =========================================

                _context.CarrinhoItens.Add(
                    new CarrinhoItem
                    {
                        ClienteId = cliente.Id,
                        ProdutoId = produto.Id,
                        Cor = cor,
                        Tamanho = tamanho,
                        Quantidade = quantidade,
                        PrecoUnitario = produto.Preco,
                        DataAdicao = DateTime.Now
                    });
            }

            await _context.SaveChangesAsync();

            TempData["Sucesso"] =
                "Produto adicionado ao carrinho!";

            // =========================================
            // IR DIRETO PARA O CARRINHO
            // =========================================

            return RedirectToAction(nameof(Index));
        }

        // =========================================
        // ADICIONAR COMBO AO CARRINHO
        // =========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdicionarCombo(
            int comboId,
            int quantidade = 1)
        {
            var cliente =
                await ObterClienteLogado();

            if (cliente == null)
            {
                return Unauthorized();
            }

            if (quantidade <= 0)
            {
                return BadRequest();
            }

            var combo = await _context.Combos
                .Include(c => c.Itens)
                    .ThenInclude(i => i.Produto)
                .FirstOrDefaultAsync(c =>
                    c.Id == comboId &&
                    c.Ativo);

            if (combo == null)
            {
                return NotFound();
            }

            // =========================================
            // VERIFICAR SE O COMBO JÁ ESTÁ NO CARRINHO
            // =========================================

            var itemExistente =
                await _context.CarrinhoComboItens
                    .FirstOrDefaultAsync(i =>
                        i.ClienteId == cliente.Id &&
                        i.ComboId == comboId);

            if (itemExistente != null)
            {
                itemExistente.Quantidade += quantidade;
            }
            else
            {
                _context.CarrinhoComboItens.Add(
                    new CarrinhoComboItem
                    {
                        ClienteId = cliente.Id,
                        ComboId = combo.Id,
                        Quantidade = quantidade,
                        PrecoUnitario = combo.Preco,
                        DataAdicao = DateTime.Now
                    });
            }

            await _context.SaveChangesAsync();

            TempData["Sucesso"] =
                "Combo adicionado ao carrinho!";

            return RedirectToAction(nameof(Index));
        }

        // =========================================
        // POST: CALCULAR FRETE
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CalcularFrete(string cepDestino)
        {
            var cliente = await ObterClienteLogado();

            if (cliente == null)
            {
                return Unauthorized();
            }

            // =========================================
            // VALIDAR CEP
            // =========================================

            if (string.IsNullOrWhiteSpace(cepDestino))
            {
                TempData["Erro"] =
                    "Informe o CEP de entrega.";

                return RedirectToAction(nameof(Index));
            }


            // Remove caracteres como "-" e espaços

            cepDestino = new string(
                cepDestino
                    .Where(char.IsDigit)
                    .ToArray());


            if (cepDestino.Length != 8)
            {
                TempData["Erro"] =
                    "Informe um CEP válido.";

                return RedirectToAction(nameof(Index));
            }


            // =========================================
            // OBTER CARRINHO
            // =========================================

            var carrinho =
                await ObterCarrinho(cliente.Id);


            if (!carrinho.Any())
            {
                TempData["Erro"] =
                    "Seu carrinho está vazio.";

                return RedirectToAction(nameof(Index));
            }


            // =========================================
            // CALCULAR FRETE
            // =========================================

            var opcoes =
                await _freteService.CalcularFrete(
                    carrinho,
                    cepDestino);


            // =========================================
            // ENVIAR PARA A VIEW
            // =========================================

            ViewBag.CepDestino = cepDestino;

            ViewBag.OpcoesFrete = opcoes;


            return View("Index", carrinho);
        }

        // =========================================
        // AUMENTAR QUANTIDADE
        // =========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Aumentar(
            int produtoId,
            string cor,
            string tamanho)
        {
            var cliente =
                await ObterClienteLogado();

            if (cliente == null)
            {
                return Unauthorized();
            }


            var item = await _context.CarrinhoItens
                .FirstOrDefaultAsync(i =>
                    i.ClienteId == cliente.Id &&
                    i.ProdutoId == produtoId &&
                    i.Cor == cor &&
                    i.Tamanho == tamanho);


            if (item == null)
            {
                return RedirectToAction(
                    nameof(Index));
            }


            var estoque = await _context.Estoques
                .FirstOrDefaultAsync(e =>
                    e.ProdutoId == produtoId &&
                    e.Cor == cor &&
                    e.Tamanho == tamanho);


            if (estoque == null)
            {
                TempData["Erro"] =
                    "Essa combinação não está mais disponível.";

                return RedirectToAction(
                    nameof(Index));
            }


            if (item.Quantidade >= estoque.Quantidade)
            {
                TempData["Erro"] =
                    $"Quantidade máxima disponível: " +
                    $"{estoque.Quantidade}.";

                return RedirectToAction(
                    nameof(Index));
            }


            item.Quantidade++;

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(Index));
        }


        // =========================================
        // DIMINUIR QUANTIDADE
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Diminuir(
            int produtoId,
            string cor,
            string tamanho)
        {
            var cliente =
                await ObterClienteLogado();

            if (cliente == null)
            {
                return Unauthorized();
            }


            var item = await _context.CarrinhoItens
                .FirstOrDefaultAsync(i =>
                    i.ClienteId == cliente.Id &&
                    i.ProdutoId == produtoId &&
                    i.Cor == cor &&
                    i.Tamanho == tamanho);


            if (item == null)
            {
                return RedirectToAction(
                    nameof(Index));
            }


            item.Quantidade--;


            if (item.Quantidade <= 0)
            {
                _context.CarrinhoItens
                    .Remove(item);
            }


            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(Index));
        }


        // =========================================
        // REMOVER ITEM
        // =========================================
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Remover(
            int produtoId,
            string cor,
            string tamanho)
        {
            var cliente =
                await ObterClienteLogado();

            if (cliente == null)
            {
                return Unauthorized();
            }


            var item = await _context.CarrinhoItens
                .FirstOrDefaultAsync(i =>
                    i.ClienteId == cliente.Id &&
                    i.ProdutoId == produtoId &&
                    i.Cor == cor &&
                    i.Tamanho == tamanho);


            if (item != null)
            {
                _context.CarrinhoItens
                    .Remove(item);

                await _context.SaveChangesAsync();
            }


            TempData["Sucesso"] =
                "Produto removido do carrinho.";

            return RedirectToAction(
                nameof(Index));
        }

        // =========================================
        // AUMENTAR QUANTIDADE DO COMBO
        // =========================================
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AumentarCombo(int comboId)
        {
            var cliente = await ObterClienteLogado();

            if (cliente == null)
            {
                return Unauthorized();
            }

            var item = await _context.CarrinhoComboItens
                .FirstOrDefaultAsync(i =>
                    i.ClienteId == cliente.Id &&
                    i.ComboId == comboId);

            if (item == null)
            {
                return RedirectToAction(nameof(Index));
            }

            item.Quantidade++;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // =========================================
        // DIMINUIR QUANTIDADE DO COMBO
        // =========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DiminuirCombo(int comboId)
        {
            var cliente = await ObterClienteLogado();

            if (cliente == null)
            {
                return Unauthorized();
            }

            var item = await _context.CarrinhoComboItens
                .FirstOrDefaultAsync(i =>
                    i.ClienteId == cliente.Id &&
                    i.ComboId == comboId);

            if (item == null)
            {
                return RedirectToAction(nameof(Index));
            }

            item.Quantidade--;

            if (item.Quantidade <= 0)
            {
                _context.CarrinhoComboItens.Remove(item);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // =========================================
        // REMOVER COMBO
        // =========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoverCombo(int comboId)
        {
            var cliente = await ObterClienteLogado();

            if (cliente == null)
            {
                return Unauthorized();
            }

            var item = await _context.CarrinhoComboItens
                .FirstOrDefaultAsync(i =>
                    i.ClienteId == cliente.Id &&
                    i.ComboId == comboId);

            if (item != null)
            {
                _context.CarrinhoComboItens.Remove(item);

                await _context.SaveChangesAsync();
            }

            TempData["Sucesso"] =
                "Combo removido do carrinho.";

            return RedirectToAction(nameof(Index));
        }

        // =========================================
        // OBTER CARRINHO DO BANCO
        // =========================================
        private async Task<List<ItemCarrinho>> ObterCarrinho(int clienteId)
        {
            var carrinho = new List<ItemCarrinho>();

            // =========================================
            // PRODUTOS DO CARRINHO
            // =========================================
            var produtos = await _context.CarrinhoItens
                .Include(i => i.Produto)
                    .ThenInclude(p => p.Cores)
                        .ThenInclude(c => c.Imagens)
                .Where(i => i.ClienteId == clienteId)
                .OrderBy(i => i.DataAdicao)
                .ToListAsync();

            carrinho.AddRange(produtos.Select(i =>
            {
                var corSelecionada = i.Produto!.Cores
                    .FirstOrDefault(c =>
                        string.Equals(
                            c.Nome,
                            i.Cor,
                            StringComparison.OrdinalIgnoreCase));

                var imagemDaCor = corSelecionada?.Imagens
                    .OrderBy(img => img.Id)
                    .Select(img => img.Caminho)
                    .FirstOrDefault();

                return new ItemCarrinho
                {
                    ProdutoId = i.ProdutoId,
                    ComboId = null,
                    EhCombo = false,

                    NomeProduto = i.Produto.Nome,

                    // Primeiro tenta a imagem da cor.
                    // Se não existir, usa a imagem principal do produto.
                    Imagem = imagemDaCor ?? i.Produto.Imagem,

                    Cor = i.Cor,
                    Tamanho = i.Tamanho,

                    Quantidade = i.Quantidade,

                    PrecoUnitario =
                        i.Produto.EmPromocao &&
                        i.Produto.PrecoPromocional.HasValue &&
                        i.Produto.PrecoPromocional.Value > 0 &&
                        i.Produto.PrecoPromocional.Value < i.Produto.Preco
                            ? i.Produto.PrecoPromocional.Value
                            : i.Produto.Preco
                };
            }));


            // =========================================
            // COMBOS DO CARRINHO
            // =========================================
            var combos = await _context.CarrinhoComboItens
                .Include(c => c.Combo)
                    .ThenInclude(c => c.Itens)
                        .ThenInclude(i => i.Produto)
                .Where(c => c.ClienteId == clienteId)
                .OrderBy(c => c.DataAdicao)
                .ToListAsync();

            carrinho.AddRange(combos.Select(c => new ItemCarrinho
            {
                ProdutoId = 0,
                ComboId = c.ComboId,
                EhCombo = true,

                NomeProduto = c.Combo!.Nome,
                Imagem = c.Combo.Imagem,

                Cor = "",
                Tamanho = "",

                Quantidade = c.Quantidade,
                PrecoUnitario = c.PrecoUnitario
            }));


            return carrinho
                .OrderBy(i => i.NomeProduto)
                .ToList();
        }

        // =========================================
        // POST: SELECIONAR FRETE
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SelecionarFrete(
            string tipoFrete,
            decimal valorFrete,
            int prazoDias,
            string cepDestino)
        {
            var cliente = await ObterClienteLogado();

            if (cliente == null)
            {
                return Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(tipoFrete) ||
                valorFrete < 0 ||
                prazoDias <= 0 ||
                string.IsNullOrWhiteSpace(cepDestino))
            {
                TempData["Erro"] =
                    "Selecione uma opção de frete válida.";

                return RedirectToAction(nameof(Index));
            }

            cepDestino = new string(
                cepDestino
                    .Where(char.IsDigit)
                    .ToArray());

            if (cepDestino.Length != 8)
            {
                TempData["Erro"] =
                    "Informe um CEP válido.";

                return RedirectToAction(nameof(Index));
            }

            // =========================================
            // REMOVER COTAÇÃO ANTERIOR
            // =========================================

            var cotacaoAnterior =
                await _context.CotacoesFrete
                    .Where(c => c.ClienteId == cliente.Id)
                    .OrderByDescending(c => c.DataCotacao)
                    .FirstOrDefaultAsync();

            if (cotacaoAnterior != null)
            {
                _context.CotacoesFrete.Remove(
                    cotacaoAnterior);
            }

            // =========================================
            // SALVAR NOVA COTAÇÃO
            // =========================================

            var cotacao = new CotacaoFrete
            {
                ClienteId = cliente.Id,
                CepDestino = cepDestino,
                TipoFrete = tipoFrete,
                Valor = valorFrete,
                PrazoDias = prazoDias,
                DataCotacao = DateTime.Now
            };

            _context.CotacoesFrete.Add(cotacao);

            await _context.SaveChangesAsync();

            TempData["Sucesso"] =
                $"Frete {tipoFrete} selecionado com sucesso.";

            return RedirectToAction(nameof(Index));
        }

    }
}