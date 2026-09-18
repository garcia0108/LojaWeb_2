using LojaWeb_2.Data;
using LojaWeb_2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LojaWeb_2.Controllers
{
    [Authorize(Roles = "Cliente")]
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public CheckoutController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        // =========================================
        // GET: CHECKOUT
        // =========================================

        public async Task<IActionResult> Index()
        {
            var usuario =
                await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }


            // =====================================
            // BUSCAR CLIENTE
            // =====================================

            var cliente =
                await _context.Clientes
                    .FirstOrDefaultAsync(c =>
                        c.Email == usuario.Email);

            if (cliente == null)
            {
                TempData["Erro"] =
                    "Não encontramos seu cadastro de cliente.";

                return RedirectToAction(
                    "Index",
                    "Carrinho");
            }


            // =====================================
            // PRODUTOS DO CARRINHO
            // =====================================

            var itensProdutos =
                await _context.CarrinhoItens
                    .Include(i => i.Produto)
                    .Where(i =>
                        i.ClienteId == cliente.Id)
                    .OrderBy(i => i.DataAdicao)
                    .ToListAsync();


            // =====================================
            // COMBOS DO CARRINHO
            // =====================================

            var itensCombos =
                await _context.CarrinhoComboItens
                    .Include(i => i.Combo)
                        .ThenInclude(c => c.Itens)
                            .ThenInclude(i => i.Produto)
                    .Where(i =>
                        i.ClienteId == cliente.Id)
                    .OrderBy(i => i.DataAdicao)
                    .ToListAsync();


            // =====================================
            // VERIFICAR CARRINHO VAZIO
            // =====================================

            if (!itensProdutos.Any() &&
                !itensCombos.Any())
            {
                TempData["Erro"] =
                    "Seu carrinho está vazio.";

                return RedirectToAction(
                    "Index",
                    "Carrinho");
            }


            // =====================================
            // MONTAR LISTA PARA A VIEW
            // =====================================

            var carrinho =
                new List<ItemCarrinho>();


            // =====================================
            // PRODUTOS NORMAIS
            // =====================================

            carrinho.AddRange(
                itensProdutos.Select(i =>
                    new ItemCarrinho
                    {
                        ProdutoId =
                            i.ProdutoId,

                        ComboId = null,

                        EhCombo = false,

                        NomeProduto =
                            i.Produto?.Nome
                            ?? "Produto",

                        Imagem =
                            i.Produto?.Imagem,

                        Cor =
                            i.Cor,

                        Tamanho =
                            i.Tamanho,

                        PrecoUnitario =
                            i.PrecoUnitario,

                        Quantidade =
                            i.Quantidade
                    })
            );


            // =====================================
            // COMBOS
            // =====================================

            carrinho.AddRange(
                itensCombos.Select(i =>
                    new ItemCarrinho
                    {
                        ProdutoId = 0,

                        ComboId =
                            i.ComboId,

                        EhCombo = true,

                        NomeProduto =
                            i.Combo?.Nome
                            ?? "Combo",

                        Imagem =
                            i.Combo?.Imagem,

                        Cor = string.Empty,

                        Tamanho = string.Empty,

                        PrecoUnitario =
                            i.PrecoUnitario,

                        Quantidade =
                            i.Quantidade
                    })
            );


            // =====================================
            // TOTAL
            // =====================================

            decimal total =
                carrinho.Sum(i =>
                    i.Subtotal);

            // =====================================
            // FRETE SELECIONADO
            // =====================================

            var cotacaoFrete =
                await _context.CotacoesFrete
                    .Where(c =>
                        c.ClienteId == cliente.Id)
                    .OrderByDescending(c =>
                        c.DataCotacao)
                    .FirstOrDefaultAsync();

            decimal valorFrete =
                cotacaoFrete?.Valor ?? 0;

            decimal totalComFrete =
                total + valorFrete;

            // =====================================
            // CHECKOUT
            // =====================================

            var checkout =
                new CheckoutViewModel
                {
                    ClienteId =
                        cliente.Id,

                    Nome =
                        cliente.Nome,

                    Email =
                        cliente.Email
                        ?? usuario.Email
                        ?? string.Empty,

                    Telefone =
                        cliente.Telefone,

                    Endereco =
                        cliente.Endereco
                        ?? string.Empty,

                    Numero =
                        cliente.Numero
                        ?? string.Empty,

                    Bairro =
                        cliente.Bairro
                        ?? string.Empty,

                    Cidade =
                        cliente.Cidade
                        ?? string.Empty,

                    Estado =
                        cliente.Estado
                        ?? string.Empty,

                    Cep =
                        cliente.Cep
                        ?? string.Empty,

                    Total =
                        totalComFrete,

                    Itens =
                        carrinho
                            .OrderBy(i =>
                                i.NomeProduto)
                            .ToList()
                };

            ViewBag.CotacaoFrete = cotacaoFrete;

            return View(checkout);
        }


        // =========================================
        // POST: CHECKOUT / CONFIRMAR
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirmar(
            CheckoutViewModel model)
        {
            // =====================================
            // USUÁRIO LOGADO
            // =====================================

            var usuario =
                await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }


            // =====================================
            // BUSCAR CLIENTE
            // =====================================

            var cliente =
                await _context.Clientes
                    .FirstOrDefaultAsync(c =>
                        c.Email == usuario.Email);

            if (cliente == null)
            {
                TempData["Erro"] =
                    "Cadastro de cliente não encontrado.";

                return RedirectToAction(
                    "Index",
                    "Carrinho");
            }


            // =====================================
            // BUSCAR PRODUTOS DO CARRINHO
            // =====================================

            var itensProdutos =
                await _context.CarrinhoItens
                    .Include(i => i.Produto)
                    .Where(i =>
                        i.ClienteId == cliente.Id)
                    .ToListAsync();


            // =====================================
            // BUSCAR COMBOS DO CARRINHO
            // =====================================

            var itensCombos =
                await _context.CarrinhoComboItens
                    .Include(i => i.Combo)
                        .ThenInclude(c => c.Itens)
                            .ThenInclude(i => i.Produto)
                    .Where(i =>
                        i.ClienteId == cliente.Id)
                    .ToListAsync();


            // =====================================
            // CARRINHO VAZIO
            // =====================================

            if (!itensProdutos.Any() &&
                !itensCombos.Any())
            {
                TempData["Erro"] =
                    "Seu carrinho está vazio.";

                return RedirectToAction(
                    "Index",
                    "Carrinho");
            }


            // =====================================
            // VALIDAR FORMULÁRIO
            // =====================================

            if (!ModelState.IsValid)
            {
                var carrinhoErro =
                    new List<ItemCarrinho>();


                carrinhoErro.AddRange(
                    itensProdutos.Select(i =>
                        new ItemCarrinho
                        {
                            ProdutoId =
                                i.ProdutoId,

                            ComboId = null,

                            EhCombo = false,

                            NomeProduto =
                                i.Produto?.Nome
                                ?? "Produto",

                            Imagem =
                                i.Produto?.Imagem,

                            Cor =
                                i.Cor,

                            Tamanho =
                                i.Tamanho,

                            PrecoUnitario =
                                i.PrecoUnitario,

                            Quantidade =
                                i.Quantidade
                        })
                );


                carrinhoErro.AddRange(
                    itensCombos.Select(i =>
                        new ItemCarrinho
                        {
                            ProdutoId = 0,

                            ComboId =
                                i.ComboId,

                            EhCombo = true,

                            NomeProduto =
                                i.Combo?.Nome
                                ?? "Combo",

                            Imagem =
                                i.Combo?.Imagem,

                            PrecoUnitario =
                                i.PrecoUnitario,

                            Quantidade =
                                i.Quantidade
                        })
                );


                model.Itens =
                    carrinhoErro
                        .OrderBy(i =>
                            i.NomeProduto)
                        .ToList();


                model.Total =
                    carrinhoErro.Sum(i =>
                        i.Subtotal);


                model.ClienteId =
                    cliente.Id;


                return View(
                    "Index",
                    model);
            }

            // =====================================
            // BUSCAR FRETE SELECIONADO
            // =====================================

            var cotacaoFrete =
                await _context.CotacoesFrete
                    .Where(c =>
                        c.ClienteId == cliente.Id)
                    .OrderByDescending(c =>
                        c.DataCotacao)
                    .FirstOrDefaultAsync();


            // =====================================
            // VALIDAR FRETE
            // =====================================

            if (cotacaoFrete == null)
            {
                TempData["Erro"] =
                    "Selecione uma forma de entrega antes de finalizar a compra.";

                return RedirectToAction(
                    "Index",
                    "Carrinho");
            }

            decimal valorFrete =
                cotacaoFrete.Valor;

            // =====================================
            // INICIAR TRANSAÇÃO
            // =====================================

            using var transacao =
                await _context.Database
                    .BeginTransactionAsync();


            try
            {
                decimal totalVenda = 0;


                // =================================
                // CRIAR VENDA
                // =================================

                var venda =
                     new Venda
                     {
                         ClienteId =
                             cliente.Id,

                         DataVenda =
                             DateTime.Now,

                         FormaPagamento =
                             model.FormaPagamento,

                         Status =
                             "Concluída",

                         TipoFrete =
                             cotacaoFrete.TipoFrete,

                         ValorFrete =
                             cotacaoFrete.Valor,

                         CepEntrega =
                             cotacaoFrete.CepDestino,

                         PrazoEntregaDias =
                             cotacaoFrete.PrazoDias,

                         Total = 0
                     };


                // =================================
                // PRODUTOS NORMAIS
                // =================================

                foreach (var itemCarrinho
                    in itensProdutos)
                {
                    // -----------------------------
                    // PRODUTO
                    // -----------------------------

                    var produto =
                        itemCarrinho.Produto;

                    if (produto == null ||
                        !produto.Ativo)
                    {
                        throw new Exception(
                            "Um dos produtos do carrinho não está mais disponível.");
                    }


                    // -----------------------------
                    // ESTOQUE
                    // -----------------------------

                    var estoque =
                        await _context.Estoques
                            .FirstOrDefaultAsync(e =>
                                e.ProdutoId ==
                                    itemCarrinho.ProdutoId &&

                                e.Cor ==
                                    itemCarrinho.Cor &&

                                e.Tamanho ==
                                    itemCarrinho.Tamanho);


                    if (estoque == null)
                    {
                        throw new Exception(
                            $"A combinação " +
                            $"{itemCarrinho.Cor} / " +
                            $"{itemCarrinho.Tamanho} " +
                            $"do produto " +
                            $"{produto.Nome} " +
                            $"não está mais disponível.");
                    }


                    // -----------------------------
                    // ESTOQUE INSUFICIENTE
                    // -----------------------------

                    if (itemCarrinho.Quantidade >
                        estoque.Quantidade)
                    {
                        throw new Exception(
                            $"Estoque insuficiente para " +
                            $"{produto.Nome}. " +
                            $"Disponível: " +
                            $"{estoque.Quantidade}.");
                    }


                    // -----------------------------
                    // CUSTO MÉDIO
                    // -----------------------------

                    if (estoque.CustoMedio <= 0)
                    {
                        throw new Exception(
                            $"O produto " +
                            $"{produto.Nome} " +
                            $"não possui custo médio cadastrado.");
                    }


                    // -----------------------------
                    // PREÇO
                    // -----------------------------

                    decimal precoUnitario =
                        itemCarrinho.PrecoUnitario;


                    decimal subtotal =
                        precoUnitario *
                        itemCarrinho.Quantidade;


                    // -----------------------------
                    // ITEM DA VENDA
                    // -----------------------------

                    var itemVenda =
                        new ItemVenda
                        {
                            Venda = venda,

                            ProdutoId =
                                produto.Id,

                            ComboId = null,

                            MarcaId =
                                estoque.MarcaId,

                            Tamanho =
                                itemCarrinho.Tamanho,

                            Cor =
                                itemCarrinho.Cor,

                            Quantidade =
                                itemCarrinho.Quantidade,

                            PrecoUnitario =
                                precoUnitario,

                            CustoUnitario =
                                estoque.CustoMedio,

                            Subtotal =
                                subtotal
                        };


                    _context.ItensVenda.Add(
                        itemVenda);


                    // -----------------------------
                    // BAIXAR ESTOQUE
                    // -----------------------------

                    estoque.Quantidade -=
                        itemCarrinho.Quantidade;


                    // -----------------------------
                    // MOVIMENTAÇÃO
                    // -----------------------------

                    var movimentacao =
                        new MovimentacaoEstoque
                        {
                            ProdutoId =
                                produto.Id,

                            MarcaId =
                                estoque.MarcaId,

                            Tamanho =
                                itemCarrinho.Tamanho,

                            Cor =
                                itemCarrinho.Cor,

                            Quantidade =
                                itemCarrinho.Quantidade,

                            PrecoCompraUnitario =
                                estoque.CustoMedio,

                            Tipo =
                                TipoMovimentacao.Saida,

                            Data =
                                DateTime.Now,

                            Observacao =
                                $"Saída referente ao pedido " +
                                $"#{venda.Id}"
                        };


                    _context.MovimentacoesEstoque.Add(
                        movimentacao);


                    totalVenda +=
                        subtotal;
                }


                // =================================
                // COMBOS
                // =================================

                foreach (var itemCarrinhoCombo
                    in itensCombos)
                {
                    // -----------------------------
                    // VALIDAR COMBO
                    // -----------------------------

                    var combo =
                        itemCarrinhoCombo.Combo;


                    if (combo == null ||
                        !combo.Ativo)
                    {
                        throw new Exception(
                            "Um dos Combos do carrinho não está mais disponível.");
                    }


                    // -----------------------------
                    // VALIDAR ITENS
                    // -----------------------------

                    if (combo.Itens == null ||
                        !combo.Itens.Any())
                    {
                        throw new Exception(
                            $"O Combo '{combo.Nome}' " +
                            $"não possui produtos cadastrados.");
                    }


                    if (itemCarrinhoCombo.Quantidade <= 0)
                    {
                        throw new Exception(
                            $"Quantidade inválida para o Combo '{combo.Nome}'.");
                    }


                    // -----------------------------
                    // VALOR DO COMBO
                    // -----------------------------

                    decimal precoCombo =
                        itemCarrinhoCombo.PrecoUnitario;


                    decimal subtotalCombo =
                        precoCombo *
                        itemCarrinhoCombo.Quantidade;


                    // =================================
                    // CALCULAR VALOR DOS PRODUTOS
                    // =================================

                    decimal valorTotalProdutos =
                        0;


                    foreach (var comboItem
                        in combo.Itens)
                    {
                        if (comboItem.Produto == null)
                        {
                            throw new Exception(
                                $"Um produto do Combo " +
                                $"'{combo.Nome}' não foi encontrado.");
                        }


                        valorTotalProdutos +=
                            comboItem.Produto.Preco *
                            comboItem.Quantidade;
                    }


                    if (valorTotalProdutos <= 0)
                    {
                        throw new Exception(
                            $"Não foi possível calcular o valor dos produtos do Combo '{combo.Nome}'.");
                    }


                    // =================================
                    // PROCESSAR PRODUTOS DO COMBO
                    // =================================

                    decimal valorDistribuido =
                        0;

                    int ultimoItem =
                        combo.Itens.Count() - 1;

                    int indice =
                        0;


                    foreach (var comboItem
                        in combo.Itens)
                    {
                        var produto =
                            comboItem.Produto;


                        if (produto == null ||
                            !produto.Ativo)
                        {
                            throw new Exception(
                                $"O produto de um dos itens do Combo " +
                                $"'{combo.Nome}' não está mais disponível.");
                        }


                        // -----------------------------
                        // ESTOQUE
                        // -----------------------------

                        var estoque =
                            await _context.Estoques
                                .FirstOrDefaultAsync(e =>
                                    e.ProdutoId ==
                                        comboItem.ProdutoId &&

                                    e.Cor ==
                                        comboItem.Cor &&

                                    e.Tamanho ==
                                        comboItem.Tamanho);


                        if (estoque == null)
                        {
                            throw new Exception(
                                $"A combinação " +
                                $"{comboItem.Cor} / " +
                                $"{comboItem.Tamanho} " +
                                $"do produto " +
                                $"{produto.Nome} " +
                                $"não está disponível no estoque " +
                                $"para o Combo '{combo.Nome}'.");
                        }


                        // -----------------------------
                        // QUANTIDADE TOTAL
                        // -----------------------------

                        int quantidadeNecessaria =
                            comboItem.Quantidade *
                            itemCarrinhoCombo.Quantidade;


                        // -----------------------------
                        // ESTOQUE INSUFICIENTE
                        // -----------------------------

                        if (quantidadeNecessaria >
                            estoque.Quantidade)
                        {
                            throw new Exception(
                                $"Estoque insuficiente para " +
                                $"{produto.Nome} " +
                                $"({comboItem.Cor} / " +
                                $"{comboItem.Tamanho}) " +
                                $"no Combo '{combo.Nome}'. " +
                                $"Necessário: " +
                                $"{quantidadeNecessaria}. " +
                                $"Disponível: " +
                                $"{estoque.Quantidade}.");
                        }


                        // -----------------------------
                        // CUSTO MÉDIO
                        // -----------------------------

                        if (estoque.CustoMedio <= 0)
                        {
                            throw new Exception(
                                $"O produto " +
                                $"{produto.Nome} " +
                                $"não possui custo médio cadastrado.");
                        }


                        // =================================
                        // DISTRIBUIR PREÇO DO COMBO
                        // =================================

                        decimal valorItemCombo =
                            comboItem.Quantidade *
                            produto.Preco;


                        decimal proporcao =
                            valorItemCombo /
                            valorTotalProdutos;


                        decimal valorVendaItem;


                        if (indice == ultimoItem)
                        {
                            valorVendaItem =
                                subtotalCombo -
                                valorDistribuido;
                        }
                        else
                        {
                            valorVendaItem =
                                Math.Round(
                                    subtotalCombo *
                                    proporcao,
                                    2);

                            valorDistribuido +=
                                valorVendaItem;
                        }


                        decimal precoUnitarioItem =
                            valorVendaItem /
                            quantidadeNecessaria;


                        // =================================
                        // ITEM DA VENDA
                        // =================================

                        var itemVenda =
                            new ItemVenda
                            {
                                Venda = venda,

                                ProdutoId =
                                    produto.Id,

                                ComboId =
                                    combo.Id,

                                MarcaId =
                                    estoque.MarcaId,

                                Tamanho =
                                    comboItem.Tamanho
                                    ?? string.Empty,

                                Cor =
                                    comboItem.Cor
                                    ?? string.Empty,

                                Quantidade =
                                    quantidadeNecessaria,

                                PrecoUnitario =
                                    Math.Round(
                                        precoUnitarioItem,
                                        2),

                                CustoUnitario =
                                    estoque.CustoMedio,

                                Subtotal =
                                    valorVendaItem
                            };


                        _context.ItensVenda.Add(
                            itemVenda);


                        // =================================
                        // BAIXAR ESTOQUE
                        // =================================

                        estoque.Quantidade -=
                            quantidadeNecessaria;


                        // =================================
                        // MOVIMENTAÇÃO
                        // =================================

                        var movimentacao =
                            new MovimentacaoEstoque
                            {
                                ProdutoId =
                                    produto.Id,

                                MarcaId =
                                    estoque.MarcaId,

                                Tamanho =
                                    comboItem.Tamanho
                                    ?? string.Empty,

                                Cor =
                                    comboItem.Cor
                                    ?? string.Empty,

                                Quantidade =
                                    quantidadeNecessaria,

                                PrecoCompraUnitario =
                                    estoque.CustoMedio,

                                Tipo =
                                    TipoMovimentacao.Saida,

                                Data =
                                    DateTime.Now,

                                Observacao =
                                    $"Saída do Combo " +
                                    $"'{combo.Nome}' " +
                                    $"referente ao pedido " +
                                    $"#{venda.Id}"
                            };


                        _context.MovimentacoesEstoque.Add(
                            movimentacao);


                        indice++;
                    }


                    // =================================
                    // TOTAL DO COMBO
                    // =================================

                    totalVenda +=
                        subtotalCombo;
                }


                // =================================
                // ATUALIZAR TOTAL DA VENDA
                // =================================
               
                totalVenda += valorFrete;// ADICIONAR FRETE

                venda.Total =
                    totalVenda; // ATUALIZAR TOTAL DA VENDA

                await _context.SaveChangesAsync();


                // =================================
                // LIMPAR PRODUTOS
                // =================================

                _context.CarrinhoItens.RemoveRange(
                    itensProdutos);


                // =================================
                // LIMPAR COMBOS
                // =================================

                _context.CarrinhoComboItens.RemoveRange(
                    itensCombos);


                await _context.SaveChangesAsync();


                // =================================
                // CONFIRMAR TRANSAÇÃO
                // =================================

                await transacao.CommitAsync();


                TempData["Sucesso"] =
                    "Pedido realizado com sucesso!";


                return RedirectToAction(
                    "Sucesso");
            }
            catch (Exception ex)
            {
                await transacao.RollbackAsync();


                TempData["Erro"] =
                    ex.Message;


                return RedirectToAction(
                    "Index");
            }
        }


        // =========================================
        // PEDIDO REALIZADO
        // =========================================

        public IActionResult Sucesso()
        {
            return View();
        }
    }
}