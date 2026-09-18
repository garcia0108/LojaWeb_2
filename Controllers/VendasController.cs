using LojaWeb_2.Data;
using LojaWeb_2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LojaWeb_2.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class VendasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VendasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Vendas
        public async Task<IActionResult> Index(
            string? busca,
            DateTime? dataInicial,
            DateTime? dataFinal,
            string? formaPagamento,
            string? status)
        {
            var query = _context.Vendas
                .Include(v => v.Cliente)
                .AsQueryable();

            // ================================
            // CLIENTE
            // ================================

            if (!string.IsNullOrWhiteSpace(busca))
            {
                busca = busca.Trim();

                query = query.Where(v =>
                    v.Cliente != null &&
                    v.Cliente.Nome.Contains(busca));
            }


            // ================================
            // DATA INICIAL
            // ================================

            if (dataInicial.HasValue)
            {
                query = query.Where(v =>
                    v.DataVenda.Date >= dataInicial.Value.Date);
            }


            // ================================
            // DATA FINAL
            // ================================

            if (dataFinal.HasValue)
            {
                query = query.Where(v =>
                    v.DataVenda.Date <= dataFinal.Value.Date);
            }


            // ================================
            // FORMA DE PAGAMENTO
            // ================================

            if (!string.IsNullOrWhiteSpace(formaPagamento))
            {
                query = query.Where(v =>
                    v.FormaPagamento == formaPagamento);
            }


            // ================================
            // STATUS
            // ================================

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(v =>
                    v.Status == status);
            }


            var vendas = await query
                .OrderByDescending(v => v.DataVenda)
                .ToListAsync();


            // ================================
            // RESUMO
            // ================================

            ViewBag.TotalVendas = vendas.Count;

            ViewBag.TotalConcluidas =
                vendas.Count(v => v.Status == "Concluída");

            ViewBag.TotalCanceladas =
                vendas.Count(v => v.Status == "Cancelada");

            ViewBag.Faturamento =
                vendas
                    .Where(v => v.Status == "Concluída")
                    .Sum(v => v.Total);


            // Manter filtros na tela

            ViewBag.Busca = busca;

            ViewBag.DataInicial =
                dataInicial?.ToString("yyyy-MM-dd");

            ViewBag.DataFinal =
                dataFinal?.ToString("yyyy-MM-dd");

            ViewBag.FormaPagamento =
                formaPagamento;

            ViewBag.Status =
                status;


            return View(vendas);
        }

        // GET: Vendas/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Clientes = await _context.Clientes
                .Where(c => c.Ativo)
                .OrderBy(c => c.Nome)
                .ToListAsync();

            ViewBag.Produtos = await _context.Produtos
                .OrderBy(p => p.Nome)
                .ToListAsync();

            await CarregarDadosVenda();
            return View();
        }

        // GET: Vendas/AlterarStatus/5
        public async Task<IActionResult> AlterarStatus(int id)
        {
            var venda = await _context.Vendas
                .Include(v => v.Cliente)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venda == null)
            {
                return NotFound();
            }

            return View(venda);
        }

        //// GET: Vendas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venda = await _context.Vendas
                .Include(v => v.Cliente)
                .Include(v => v.Itens)
                    .ThenInclude(i => i.Produto)
                .Include(v => v.Itens)
                    .ThenInclude(i => i.Marca)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venda == null)
            {
                return NotFound();
            }


            // =========================================
            // CUSTO TOTAL DA VENDA
            // =========================================

            decimal custoTotal = venda.Itens
                .Sum(i =>
                    i.Quantidade *
                    i.CustoUnitario);


            // =========================================
            // LUCRO BRUTO
            // =========================================

            decimal lucroBruto =
                venda.Total - custoTotal;


            // =========================================
            // MARGEM DE LUCRO
            // =========================================

            decimal margemLucro = 0;

            if (venda.Total > 0)
            {
                margemLucro =
                    (lucroBruto / venda.Total) * 100;
            }


            ViewBag.CustoTotal = custoTotal;

            ViewBag.LucroBruto = lucroBruto;

            ViewBag.MargemLucro = margemLucro;


            return View(venda);
        }

        // GET: Vendas/Dashboard
        [Authorize(Roles ="Administrador")]
        public async Task<IActionResult> Dashboard(
            DateTime? dataInicial,
            DateTime? dataFinal)
        {
            var query = _context.Vendas
                .Include(v => v.Itens)
                .ThenInclude(i => i.Produto)
                .Where(v => v.Status == "Concluída")
                .AsQueryable();

            // DATA INICIAL
            if (dataInicial.HasValue)
            {
                query = query.Where(v =>
                    v.DataVenda.Date >= dataInicial.Value.Date);
            }

            // DATA FINAL
            if (dataFinal.HasValue)
            {
                query = query.Where(v =>
                    v.DataVenda.Date <= dataFinal.Value.Date);
            }

            var vendas = await query
                .ToListAsync();

            // =========================================
            // INDICADORES
            // =========================================

            int totalVendas = vendas.Count;

            decimal faturamento = vendas.Sum(v => v.Total);

            decimal custoTotal = vendas
                .SelectMany(v => v.Itens)
                .Sum(i =>
                    i.Quantidade *
                    i.CustoUnitario);

            decimal lucroBruto =
                faturamento - custoTotal;

            decimal margemLucro = 0;

            if (faturamento > 0)
            {
                margemLucro =
                    (lucroBruto / faturamento) * 100;
            }

            // =========================================
            // VENDAS CANCELADAS
            // =========================================

            var queryCanceladas = _context.Vendas
                .Where(v => v.Status == "Cancelada")
                .AsQueryable();


            // DATA INICIAL
            if (dataInicial.HasValue)
            {
                queryCanceladas = queryCanceladas
                    .Where(v =>
                            v.DataCancelamento.HasValue &&
                            v.DataCancelamento.Value.Date >= dataInicial.Value.Date);
            }


            // DATA FINAL
            if (dataFinal.HasValue)
            {
                queryCanceladas = queryCanceladas
                    .Where(v =>
                            v.DataCancelamento.HasValue &&
                            v.DataCancelamento.Value.Date <= dataFinal.Value.Date);
            }


            var vendasCanceladas =
                await queryCanceladas.ToListAsync();

            foreach (var venda in vendasCanceladas)
            {
                Console.WriteLine(
                    $"CANCELADA -> ID: {venda.Id} | " +
                    $"DataVenda: {venda.DataVenda:dd/MM/yyyy} | " +
                    $"Total: {venda.Total:C2}");
            }


            int totalVendasCanceladas =
                vendasCanceladas.Count;


            decimal valorVendasCanceladas =
                vendasCanceladas.Sum(v => v.Total);


            ViewBag.TotalVendasCanceladas =
                totalVendasCanceladas;

            ViewBag.ValorVendasCanceladas =
                valorVendasCanceladas;

            // =========================================
            // TICKET MÉDIO
            // =========================================

            decimal ticketMedio = 0;

            if (totalVendas > 0)
            {
                ticketMedio = faturamento / totalVendas;
            }

            ViewBag.TicketMedio = ticketMedio;

            // =========================================
            // PRODUTOS MAIS VENDIDOS
            // =========================================

            var produtosMaisVendidos = vendas
                .SelectMany(v => v.Itens)
                .GroupBy(i => new
                {
                    i.ProdutoId,
                    NomeProduto = i.Produto != null
                        ? i.Produto.Nome
                        : "Produto não encontrado"
                })
                .Select(g => new
                {
                    Produto = g.Key.NomeProduto,
                    QuantidadeVendida = g.Sum(i => i.Quantidade),
                    Faturamento = g.Sum(i => i.Subtotal)
                })
                .OrderByDescending(x => x.QuantidadeVendida)
                .Take(5)
                .ToList();

            ViewBag.ProdutosMaisVendidos =
                produtosMaisVendidos;

            // =========================================
            // VENDAS POR FORMA DE PAGAMENTO
            // =========================================

            var vendasPorPagamento = vendas
                .GroupBy(v => v.FormaPagamento)
                .Select(g => new
                {
                    FormaPagamento = g.Key,
                    QuantidadeVendas = g.Count(),
                    Faturamento = g.Sum(v => v.Total)
                })
                .OrderByDescending(x => x.Faturamento)
                .ToList();

            ViewBag.VendasPorPagamento = vendasPorPagamento;

            // =========================================
            // FATURAMENTO POR DIA
            // =========================================

            var faturamentoPorDia = vendas
                .GroupBy(v => v.DataVenda.Date)
                .Select(g => new
                {
                    Data = g.Key,
                    Faturamento = g.Sum(v => v.Total)
                })
                .OrderBy(x => x.Data)
                .ToList();

            ViewBag.FaturamentoPorDia = faturamentoPorDia;

            // =========================================
            // LUCRO POR DIA
            // =========================================

            var lucroPorDia = vendas
                .GroupBy(v => v.DataVenda.Date)
                .Select(g => new
                {
                    Data = g.Key,

                    Faturamento = g.Sum(v => v.Total),

                    Custo = g
                        .SelectMany(v => v.Itens)
                        .Sum(i =>
                            i.Quantidade *
                            i.CustoUnitario)
                })
                .Select(x => new
                {
                    x.Data,
                    Lucro = x.Faturamento - x.Custo
                })
                .OrderBy(x => x.Data)
                .ToList();

            ViewBag.LucroPorDia = lucroPorDia;

            // =========================================
            // ENVIAR PARA A VIEW
            // =========================================

            ViewBag.TotalVendas = totalVendas;

            ViewBag.Faturamento = faturamento;

            ViewBag.CustoTotal = custoTotal;

            ViewBag.LucroBruto = lucroBruto;

            ViewBag.MargemLucro = margemLucro;

            ViewBag.DataInicial =
                dataInicial?.ToString("yyyy-MM-dd");

            ViewBag.DataFinal =
                dataFinal?.ToString("yyyy-MM-dd");


            return View();
        }

        // POST: Vendas/Cancelar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancelar(int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var venda = await _context.Vendas
                    .Include(v => v.Itens)
                    .FirstOrDefaultAsync(v => v.Id == id);

                if (venda == null)
                {
                    await transaction.RollbackAsync();
                    return NotFound();
                }

                // Não permitir cancelar uma venda já cancelada
                if (venda.Status == "Cancelada")
                {
                    TempData["Erro"] =
                        "Esta venda já está cancelada.";

                    await transaction.RollbackAsync();

                    return RedirectToAction(nameof(Details), new { id });
                }

                // =========================================
                // DEVOLVER OS PRODUTOS AO ESTOQUE
                // =========================================

                foreach (var item in venda.Itens)
                {
                    var estoque = await _context.Estoques
                        .FirstOrDefaultAsync(e =>
                            e.ProdutoId == item.ProdutoId &&
                            e.MarcaId == item.MarcaId &&
                            e.Tamanho == item.Tamanho &&
                            e.Cor == item.Cor);

                    if (estoque == null)
                    {
                        throw new Exception(
                            $"O estoque do produto não foi encontrado para o item da venda.");
                    }

                    // Devolve a quantidade
                    estoque.Quantidade += item.Quantidade;


                    // =====================================
                    // REGISTRAR ENTRADA NO HISTÓRICO
                    // =====================================

                    var movimentacao =
                        new MovimentacaoEstoque
                        {
                            ProdutoId = item.ProdutoId,

                            MarcaId = item.MarcaId,

                            Tamanho = item.Tamanho,

                            Cor = item.Cor,

                            Quantidade = item.Quantidade,

                            Tipo = TipoMovimentacao.Entrada,

                            Data = DateTime.Now,

                            Observacao =
                                $"Devolução referente ao cancelamento da venda #{venda.Id}."
                        };

                    _context.MovimentacoesEstoque
                        .Add(movimentacao);
                }


                // =========================================
                // ALTERAR STATUS
                // =========================================

                venda.Status = "Cancelada";

                venda.DataCancelamento = DateTime.Now;

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();


                TempData["Sucesso"] =
                    "Venda cancelada e estoque devolvido com sucesso!";


                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                TempData["Erro"] =
                    "Não foi possível cancelar a venda: " +
                    ex.Message;

                return RedirectToAction(nameof(Details), new { id });
            }
        }

        // POST: Vendas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Venda? venda)
        {
            if (venda == null)
            {
                return BadRequest("Venda inválida.");
            }

            if (venda.ClienteId <= 0)
            {
                ModelState.AddModelError(
                    nameof(venda.ClienteId),
                    "Selecione um cliente.");
            }

            if (venda.Itens == null || !venda.Itens.Any())
            {
                ModelState.AddModelError(
                    "",
                    "Adicione pelo menos um produto à venda.");
            }

            if (string.IsNullOrWhiteSpace(venda.FormaPagamento))
            {
                ModelState.AddModelError(
                    nameof(venda.FormaPagamento),
                    "Selecione uma forma de pagamento.");
            }

            if (!ModelState.IsValid)
            {
                await CarregarDadosVenda();
                return View(venda);
            }

            if (venda.Itens == null || !venda.Itens.Any())
            {
                ModelState.AddModelError(
                    "",
                    "Adicione pelo menos um produto à venda.");
            }

            using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                // =========================================
                // VERIFICAR CLIENTE
                // =========================================

                var cliente = await _context.Clientes
                    .FirstOrDefaultAsync(c =>
                        c.Id == venda.ClienteId &&
                        c.Ativo);

                if (cliente == null)
                {
                    ModelState.AddModelError(
                        nameof(venda.ClienteId),
                        "Cliente inválido ou inativo.");

                    await transaction.RollbackAsync();

                    await CarregarDadosVenda();

                    return View(venda);
                }


                decimal total = 0;


                // =========================================
                // VALIDAR E PROCESSAR ITENS
                // =========================================

                foreach (var item in venda.Itens)
                {
                    if (item.Quantidade <= 0)
                    {
                        throw new Exception(
                            "A quantidade de um dos produtos é inválida.");
                    }


                    // =========================================
                    // BUSCAR PRODUTO
                    // =========================================

                    var produto = await _context.Produtos
                        .FirstOrDefaultAsync(p =>
                            p.Id == item.ProdutoId &&
                            p.Ativo);

                    if (produto == null)
                    {
                        throw new Exception(
                            "Um dos produtos não existe ou está inativo.");
                    }


                    // =========================================
                    // BUSCAR ESTOQUE DA VARIANTE
                    // =========================================

                    var estoque = await _context.Estoques
                        .FirstOrDefaultAsync(e =>
                            e.ProdutoId == item.ProdutoId &&
                            e.MarcaId == item.MarcaId &&
                            e.Tamanho == item.Tamanho &&
                            e.Cor == item.Cor);


                    if (estoque == null)
                    {
                        throw new Exception(
                            $"A variante do produto '{produto.Nome}' " +
                            $"não foi encontrada no estoque.");
                    }


                    // =========================================
                    // VERIFICAR ESTOQUE
                    // =========================================

                    if (estoque.Quantidade < item.Quantidade)
                    {
                        throw new Exception(
                            $"Estoque insuficiente para o produto " +
                            $"'{produto.Nome}'. " +
                            $"Disponível: {estoque.Quantidade}.");
                    }


                    // =========================================
                    // PREÇO DE VENDA
                    // =========================================

                    item.PrecoUnitario = produto.Preco;


                    // =========================================
                    // CUSTO MÉDIO
                    // =========================================

                    decimal custoMedio = estoque.CustoMedio;

                    if (custoMedio <= 0)
                    {
                        throw new Exception(
                            $"Custo médio inválido para o estoque " +
                            $"{estoque.Id}.");
                    }

                    item.CustoUnitario =
                        Math.Round(custoMedio, 2);


                    // =========================================
                    // GARANTIR DADOS DA VARIANTE
                    // =========================================

                    item.MarcaId = estoque.MarcaId;
                    item.Tamanho = estoque.Tamanho;
                    item.Cor = estoque.Cor;


                    // =========================================
                    // SUBTOTAL
                    // =========================================

                    item.Subtotal =
                        item.Quantidade *
                        item.PrecoUnitario;

                    total += item.Subtotal;


                    // =========================================
                    // BAIXAR ESTOQUE
                    // =========================================

                    estoque.Quantidade -= item.Quantidade;


                    // =========================================
                    // REGISTRAR MOVIMENTAÇÃO DE SAÍDA
                    // =========================================

                    var movimentacao =
                        new MovimentacaoEstoque
                        {
                            ProdutoId = estoque.ProdutoId,

                            MarcaId = estoque.MarcaId,

                            Tamanho = estoque.Tamanho,

                            Cor = estoque.Cor,

                            Quantidade = item.Quantidade,

                            PrecoCompraUnitario = item.CustoUnitario,

                            Tipo = TipoMovimentacao.Saida,

                            Data = DateTime.Now,

                            Observacao =
                                "Saída referente à venda."
                        };

                    _context.MovimentacoesEstoque
                        .Add(movimentacao);
                }


                // =========================================
                // CRIAR VENDA
                // =========================================

                venda.DataVenda = DateTime.Now;

                venda.Status = "Concluída";

                venda.Total = total;

                // =========================================
                // SALVAR VENDA
                // =========================================

                _context.Vendas.Add(venda);

                await _context.SaveChangesAsync();


                // =========================================
                // CONFIRMAR TRANSAÇÃO
                // =========================================

                await transaction.CommitAsync();


                TempData["Sucesso"] =
                    "Venda realizada com sucesso!";


                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                ModelState.AddModelError(
                    "",
                    ex.Message);

                await CarregarDadosVenda();

                return View(venda);
            }
        }

        // POST: Vendas/AlterarStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlterarStatus(
    string? codigoRastreamento,
            int id,
            string status,
            string? transportadora,
            DateTime? dataEnvio)
        {
            var venda = await _context.Vendas
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venda == null)
            {
                return NotFound();
            }

            var statusPermitidos = new[]
            {
        "Pedido realizado",
        "Pagamento aprovado",
        "Em preparação",
        "Enviado",
        "Entregue",
        "Cancelado"
    };

            if (!statusPermitidos.Contains(status))
            {
                TempData["Erro"] =
                    "Status inválido.";

                return RedirectToAction(
                    nameof(AlterarStatus),
                    new { id });
            }

            venda.Status = status;

            if (status == "Enviado")
            {
                venda.Transportadora = transportadora;

                venda.CodigoRastreamento = codigoRastreamento;

                venda.DataEnvio = dataEnvio ?? DateTime.Now;
            }

            await _context.SaveChangesAsync();

            TempData["Sucesso"] =
                "Status do pedido atualizado com sucesso!";

            return RedirectToAction(
                nameof(Details),
                new { id });
        }

        // GET: Vendas/ObterEstoque
        [HttpGet]
        [Route("Vendas/ObterEstoque")]
        public async Task<IActionResult> ObterEstoque(int produtoId)
        {
            if (produtoId <= 0)
            {
                return BadRequest("Produto inválido.");
            }

            var estoque = await _context.Estoques
                .Include(e => e.Marca)
                .Include(e => e.Produto)
                .Where(e =>
                    e.ProdutoId == produtoId &&
                    e.Quantidade > 0)
                .Select(e => new
                {
                    estoqueId = e.Id,
                    produtoId = e.ProdutoId,
                    produto = e.Produto!.Nome,
                    marcaId = e.MarcaId,
                    marca = e.Marca!.Nome,
                    tamanho = e.Tamanho,
                    cor = e.Cor,
                    quantidadeDisponivel = e.Quantidade,
                    preco = e.Produto!.Preco
                })
                .ToListAsync();

            return Json(estoque);
        }

        private async Task CarregarDadosVenda()
        {
            ViewBag.Clientes = await _context.Clientes
                .Where(c => c.Ativo)
                .OrderBy(c => c.Nome)
                .ToListAsync();

            ViewBag.Produtos = await _context.Produtos
                .Where(p => p.Ativo)
                .OrderBy(p => p.Nome)
                .ToListAsync();
        }
    }
}
