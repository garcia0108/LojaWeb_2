using LojaWeb_2.Data;
using LojaWeb_2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace LojaWeb_2.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class RelatoriosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RelatoriosController(
            ApplicationDbContext context)
        {
            _context = context;
        }


        // =========================================
        // RELATÓRIO DE VENDAS
        // =========================================

        public async Task<IActionResult> Vendas(
            DateTime? dataInicial,
            DateTime? dataFinal,
            string? formaPagamento,
            string? status)
        {
            var query = _context.Vendas
                .Include(v => v.Cliente)
                .Include(v => v.Itens)
                .ThenInclude(i => i.Produto)
                .AsQueryable();


            // =====================================
            // DATA INICIAL
            // =====================================

            if (dataInicial.HasValue)
            {
                query = query.Where(v =>
                    v.DataVenda.Date >=
                    dataInicial.Value.Date);
            }


            // =====================================
            // DATA FINAL
            // =====================================

            if (dataFinal.HasValue)
            {
                query = query.Where(v =>
                    v.DataVenda.Date <=
                    dataFinal.Value.Date);
            }


            // =====================================
            // FORMA DE PAGAMENTO
            // =====================================

            if (!string.IsNullOrWhiteSpace(formaPagamento))
            {
                query = query.Where(v =>
                    v.FormaPagamento ==
                    formaPagamento);
            }


            // =====================================
            // STATUS
            // =====================================

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(v =>
                    v.Status == status);
            }


            var vendas = await query
                .OrderByDescending(v => v.DataVenda)
                .ToListAsync();


            // =====================================
            // INDICADORES
            // =====================================

            var vendasConcluidas =
                vendas
                    .Where(v => v.Status == "Concluída")
                    .ToList();


            var vendasCanceladas =
                vendas
                    .Where(v => v.Status == "Cancelada")
                    .ToList();


            int totalVendas =
                vendas.Count;


            int totalConcluidas =
                vendasConcluidas.Count;


            int totalCanceladas =
                vendasCanceladas.Count;


            decimal faturamento =
                vendasConcluidas.Sum(v => v.Total);


            decimal valorCancelado =
                vendasCanceladas.Sum(v => v.Total);


            decimal custoTotal =
                vendasConcluidas
                    .SelectMany(v => v.Itens)
                    .Sum(i =>
                        i.Quantidade *
                        i.CustoUnitario);


            decimal lucroBruto =
                faturamento -
                custoTotal;


            decimal margemLucro = 0;


            if (faturamento > 0)
            {
                margemLucro =
                    (lucroBruto /
                     faturamento) * 100;
            }


            decimal ticketMedio = 0;


            if (totalConcluidas > 0)
            {
                ticketMedio =
                    faturamento /
                    totalConcluidas;
            }


            // =====================================
            // ENVIAR PARA VIEW
            // =====================================

            ViewBag.TotalVendas =
                totalVendas;

            ViewBag.TotalConcluidas =
                totalConcluidas;

            ViewBag.TotalCanceladas =
                totalCanceladas;

            ViewBag.Faturamento =
                faturamento;

            ViewBag.ValorCancelado =
                valorCancelado;

            ViewBag.CustoTotal =
                custoTotal;

            ViewBag.LucroBruto =
                lucroBruto;

            ViewBag.MargemLucro =
                margemLucro;

            ViewBag.TicketMedio =
                ticketMedio;


            // =====================================
            // MANTER FILTROS
            // =====================================

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

        // =========================================
        // RELATÓRIO DE ESTOQUE
        // =========================================

        public async Task<IActionResult> Estoque(
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


            // =========================================
            // PESQUISA
            // =========================================

            if (!string.IsNullOrWhiteSpace(pesquisa))
            {
                pesquisa = pesquisa.Trim();

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

                    e.Tamanho.Contains(pesquisa)

                    ||

                    e.Cor.Contains(pesquisa)
                );
            }


            // =========================================
            // PRODUTO
            // =========================================

            if (produtoId.HasValue)
            {
                query = query.Where(e =>
                    e.ProdutoId == produtoId.Value);
            }


            // =========================================
            // MARCA
            // =========================================

            if (marcaId.HasValue)
            {
                query = query.Where(e =>
                    e.MarcaId == marcaId.Value);
            }


            // =========================================
            // TAMANHO
            // =========================================

            if (!string.IsNullOrWhiteSpace(tamanho))
            {
                query = query.Where(e =>
                    e.Tamanho == tamanho);
            }


            // =========================================
            // COR
            // =========================================

            if (!string.IsNullOrWhiteSpace(cor))
            {
                query = query.Where(e =>
                    e.Cor == cor);
            }


            // =========================================
            // SITUAÇÃO
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


            // =========================================
            // LISTA
            // =========================================

            var estoque = await query
                .OrderBy(e => e.Produto!.Nome)
                .ThenBy(e => e.Marca!.Nome)
                .ThenBy(e => e.Tamanho)
                .ThenBy(e => e.Cor)
                .ToListAsync();


            // =========================================
            // INDICADORES
            // =========================================

            int totalVariacoes = estoque.Count;


            int totalProdutos = estoque
                .Select(e => e.ProdutoId)
                .Distinct()
                .Count();


            int totalUnidades = estoque
                .Sum(e => e.Quantidade);


            int estoqueBaixo = estoque
                .Count(e =>
                    e.Quantidade > 0 &&
                    e.Quantidade <= 5);


            int esgotados = estoque
                .Count(e =>
                    e.Quantidade == 0);


            decimal valorEstoque = estoque
                .Sum(e =>
                    e.Quantidade *
                    e.CustoMedio);


            // =========================================
            // ENVIAR PARA VIEW
            // =========================================

            ViewBag.TotalVariacoes =
                totalVariacoes;

            ViewBag.TotalProdutos =
                totalProdutos;

            ViewBag.TotalUnidades =
                totalUnidades;

            ViewBag.EstoqueBaixo =
                estoqueBaixo;

            ViewBag.Esgotados =
                esgotados;

            ViewBag.ValorEstoque =
                valorEstoque;


            // =========================================
            // DADOS DOS FILTROS
            // =========================================

            ViewBag.Produtos =
                await _context.Produtos
                    .Where(p => p.Ativo)
                    .OrderBy(p => p.Nome)
                    .ToListAsync();


            ViewBag.Marcas =
                await _context.Marcas
                    .OrderBy(m => m.Nome)
                    .ToListAsync();


            ViewBag.Tamanhos =
                await _context.Estoques
                    .Select(e => e.Tamanho)
                    .Distinct()
                    .OrderBy(t => t)
                    .ToListAsync();


            ViewBag.Cores =
                await _context.Estoques
                    .Select(e => e.Cor)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToListAsync();


            // =========================================
            // MANTER FILTROS
            // =========================================

            ViewBag.Pesquisa =
                pesquisa;

            ViewBag.ProdutoId =
                produtoId;

            ViewBag.MarcaId =
                marcaId;

            ViewBag.Tamanho =
                tamanho;

            ViewBag.Cor =
                cor;

            ViewBag.Situacao =
                situacao;


            return View(estoque);
        }

        // =========================================
        // RELATÓRIO DE MOVIMENTAÇÕES
        // =========================================

        public async Task<IActionResult> Movimentacoes(
            DateTime? dataInicial,
            DateTime? dataFinal,
            int? produtoId,
            int? marcaId,
            int? fornecedorId,
            TipoMovimentacao? tipo)
        {
            var query = _context.MovimentacoesEstoque
                .Include(m => m.Produto)
                .Include(m => m.Marca)
                .Include(m => m.Fornecedor)
                .AsQueryable();


            // =========================================
            // DATA INICIAL
            // =========================================

            if (dataInicial.HasValue)
            {
                query = query.Where(m =>
                    m.Data.Date >= dataInicial.Value.Date);
            }


            // =========================================
            // DATA FINAL
            // =========================================

            if (dataFinal.HasValue)
            {
                query = query.Where(m =>
                    m.Data.Date <= dataFinal.Value.Date);
            }


            // =========================================
            // PRODUTO
            // =========================================

            if (produtoId.HasValue)
            {
                query = query.Where(m =>
                    m.ProdutoId == produtoId.Value);
            }


            // =========================================
            // MARCA
            // =========================================

            if (marcaId.HasValue)
            {
                query = query.Where(m =>
                    m.MarcaId == marcaId.Value);
            }


            // =========================================
            // FORNECEDOR
            // =========================================

            if (fornecedorId.HasValue)
            {
                query = query.Where(m =>
                    m.FornecedorId == fornecedorId.Value);
            }


            // =========================================
            // TIPO
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
            // INDICADORES
            // =========================================

            var entradas = movimentacoes
                .Where(m => m.Tipo == TipoMovimentacao.Entrada)
                .ToList();


            var saidas = movimentacoes
                .Where(m => m.Tipo == TipoMovimentacao.Saida)
                .ToList();


            int totalEntradas = entradas
                .Sum(m => m.Quantidade);


            int totalSaidas = saidas
                .Sum(m => m.Quantidade);


            int totalMovimentado =
                totalEntradas + totalSaidas;


            decimal valorEntradas = entradas
                .Sum(m =>
                    m.Quantidade *
                    m.PrecoCompraUnitario);


            decimal valorSaidas = saidas
                .Sum(m =>
                    m.Quantidade *
                    m.PrecoCompraUnitario);


            // =========================================
            // ENVIAR INDICADORES PARA VIEW
            // =========================================

            ViewBag.TotalEntradas =
                totalEntradas;

            ViewBag.TotalSaidas =
                totalSaidas;

            ViewBag.TotalMovimentado =
                totalMovimentado;

            ViewBag.ValorEntradas =
                valorEntradas;

            ViewBag.ValorSaidas =
                valorSaidas;


            // =========================================
            // DADOS DOS FILTROS
            // =========================================

            ViewBag.Produtos =
                await _context.Produtos
                    .OrderBy(p => p.Nome)
                    .ToListAsync();


            ViewBag.Marcas =
                await _context.Marcas
                    .OrderBy(m => m.Nome)
                    .ToListAsync();


            ViewBag.Fornecedores =
                await _context.Fornecedores
                    .OrderBy(f => f.Nome)
                    .ToListAsync();


            // =========================================
            // MANTER FILTROS
            // =========================================

            ViewBag.DataInicial =
                dataInicial?.ToString("yyyy-MM-dd");

            ViewBag.DataFinal =
                dataFinal?.ToString("yyyy-MM-dd");

            ViewBag.ProdutoId =
                produtoId;

            ViewBag.MarcaId =
                marcaId;

            ViewBag.FornecedorId =
                fornecedorId;

            ViewBag.Tipo =
                tipo;


            return View(movimentacoes);
        }

        // =========================================
        // RELATÓRIO DE COMPRAS / FORNECEDORES
        // =========================================

        public async Task<IActionResult> Compras(
            DateTime? dataInicial,
            DateTime? dataFinal,
            int? fornecedorId,
            int? produtoId,
            int? marcaId)
        {
            var query = _context.MovimentacoesEstoque
                .Include(m => m.Produto)
                .Include(m => m.Marca)
                .Include(m => m.Fornecedor)
                .Where(m => m.Tipo == TipoMovimentacao.Entrada)
                .AsQueryable();


            // =========================================
            // DATA INICIAL
            // =========================================

            if (dataInicial.HasValue)
            {
                query = query.Where(m =>
                    m.Data.Date >= dataInicial.Value.Date);
            }


            // =========================================
            // DATA FINAL
            // =========================================

            if (dataFinal.HasValue)
            {
                query = query.Where(m =>
                    m.Data.Date <= dataFinal.Value.Date);
            }


            // =========================================
            // FORNECEDOR
            // =========================================

            if (fornecedorId.HasValue)
            {
                query = query.Where(m =>
                    m.FornecedorId == fornecedorId.Value);
            }


            // =========================================
            // PRODUTO
            // =========================================

            if (produtoId.HasValue)
            {
                query = query.Where(m =>
                    m.ProdutoId == produtoId.Value);
            }


            // =========================================
            // MARCA
            // =========================================

            if (marcaId.HasValue)
            {
                query = query.Where(m =>
                    m.MarcaId == marcaId.Value);
            }


            // =========================================
            // BUSCAR COMPRAS
            // =========================================

            var compras = await query
                .OrderByDescending(m => m.Data)
                .ToListAsync();


            // =========================================
            // INDICADORES
            // =========================================

            int totalCompras = compras.Count;

            int totalUnidades = compras
                .Sum(m => m.Quantidade);

            decimal valorTotalComprado = compras
                .Sum(m =>
                    m.Quantidade *
                    m.PrecoCompraUnitario);

            int totalFornecedores = compras
                .Where(m => m.FornecedorId.HasValue)
                .Select(m => m.FornecedorId!.Value)
                .Distinct()
                .Count();


            // =========================================
            // ENVIAR INDICADORES PARA VIEW
            // =========================================

            ViewBag.TotalCompras =
                totalCompras;

            ViewBag.TotalUnidades =
                totalUnidades;

            ViewBag.ValorTotalComprado =
                valorTotalComprado;

            ViewBag.TotalFornecedores =
                totalFornecedores;


            // =========================================
            // DADOS DOS FILTROS
            // =========================================

            ViewBag.Fornecedores =
                await _context.Fornecedores
                    .OrderBy(f => f.Nome)
                    .ToListAsync();


            ViewBag.Produtos =
                await _context.Produtos
                    .OrderBy(p => p.Nome)
                    .ToListAsync();


            ViewBag.Marcas =
                await _context.Marcas
                    .OrderBy(m => m.Nome)
                    .ToListAsync();


            // =========================================
            // MANTER FILTROS
            // =========================================

            ViewBag.DataInicial =
                dataInicial?.ToString("yyyy-MM-dd");

            ViewBag.DataFinal =
                dataFinal?.ToString("yyyy-MM-dd");

            ViewBag.FornecedorId =
                fornecedorId;

            ViewBag.ProdutoId =
                produtoId;

            ViewBag.MarcaId =
                marcaId;


            return View(compras);
        }

        // =========================================
        // RELATÓRIO DE CLIENTES
        // =========================================
        public async Task<IActionResult> Clientes(
            string? pesquisa,
            DateTime? dataInicio,
            DateTime? dataFim)
        {
            var query = _context.Vendas
                .Include(v => v.Cliente)
                .Include(v => v.Itens)
                .AsQueryable();

            // FILTRO POR DATA INICIAL
            if (dataInicio.HasValue)
            {
                query = query.Where(v =>
                    v.DataVenda >= dataInicio.Value);
            }

            // FILTRO POR DATA FINAL
            if (dataFim.HasValue)
            {
                var dataFinal = dataFim.Value.Date.AddDays(1);

                query = query.Where(v =>
                    v.DataVenda < dataFinal);
            }

            // FILTRO POR CLIENTE
            if (!string.IsNullOrWhiteSpace(pesquisa))
            {
                pesquisa = pesquisa.Trim();

                query = query.Where(v =>
                    v.Cliente != null &&
                    (
                        v.Cliente.Nome.Contains(pesquisa) ||
                        (v.Cliente.Email != null &&
                         v.Cliente.Email.Contains(pesquisa))
                    ));
            }

            var vendas = await query
                .OrderByDescending(v => v.DataVenda)
                .ToListAsync();

            var clientes = vendas
                .Where(v => v.Cliente != null)
                .GroupBy(v => new
                {
                    v.Cliente!.Id,
                    v.Cliente.Nome,
                    v.Cliente.Email,
                    v.Cliente.Telefone
                })
                .Select(g => new
                {
                    ClienteId = g.Key.Id,
                    Nome = g.Key.Nome,
                    Email = g.Key.Email,
                    Telefone = g.Key.Telefone,

                    QuantidadePedidos = g.Count(),

                    QuantidadeItens = g
                        .SelectMany(v => v.Itens)
                        .Sum(i => i.Quantidade),

                    TotalComprado = g.Sum(v => v.Total),

                    UltimaCompra = g
                        .Max(v => v.DataVenda)
                })
                .OrderByDescending(c => c.TotalComprado)
                .ToList();

            ViewBag.Pesquisa = pesquisa;
            ViewBag.DataInicio = dataInicio?.ToString("yyyy-MM-dd");
            ViewBag.DataFim = dataFim?.ToString("yyyy-MM-dd");

            return View(clientes);
        }

        // =========================================
        // EXPORTAR RELATÓRIO DE CLIENTES PARA EXCEL
        // =========================================

        [HttpGet]
        public async Task<IActionResult> ExportarClientesExcel(
            string? pesquisa,
            DateTime? dataInicio,
            DateTime? dataFim)
        {
            var query = _context.Vendas
                .Include(v => v.Cliente)
                .Include(v => v.Itens)
                .AsQueryable();


            // FILTRO POR DATA INICIAL

            if (dataInicio.HasValue)
            {
                query = query.Where(v =>
                    v.DataVenda >= dataInicio.Value);
            }


            // FILTRO POR DATA FINAL

            if (dataFim.HasValue)
            {
                var dataFinal = dataFim.Value.Date.AddDays(1);

                query = query.Where(v =>
                    v.DataVenda < dataFinal);
            }


            // FILTRO POR CLIENTE

            if (!string.IsNullOrWhiteSpace(pesquisa))
            {
                pesquisa = pesquisa.Trim();

                query = query.Where(v =>
                    v.Cliente != null &&
                    (
                        v.Cliente.Nome.Contains(pesquisa) ||
                        (v.Cliente.Email != null &&
                         v.Cliente.Email.Contains(pesquisa))
                    ));
            }


            // BUSCAR VENDAS

            var vendas = await query
                .OrderByDescending(v => v.DataVenda)
                .ToListAsync();


            // AGRUPAR CLIENTES

            var clientes = vendas
                .Where(v => v.Cliente != null)
                .GroupBy(v => new
                {
                    v.Cliente!.Id,
                    v.Cliente.Nome,
                    v.Cliente.Email,
                    v.Cliente.Telefone
                })
                .Select(g => new
                {
                    ClienteId = g.Key.Id,
                    Nome = g.Key.Nome,
                    Email = g.Key.Email,
                    Telefone = g.Key.Telefone,

                    QuantidadePedidos = g.Count(),

                    QuantidadeItens = g
                        .SelectMany(v => v.Itens)
                        .Sum(i => i.Quantidade),

                    TotalComprado = g.Sum(v => v.Total),

                    UltimaCompra = g.Max(v => v.DataVenda)
                })
                .OrderByDescending(c => c.TotalComprado)
                .ToList();


            // =========================================
            // CRIAR ARQUIVO EXCEL
            // =========================================

            using var workbook = new XLWorkbook();

            var worksheet =
                workbook.Worksheets.Add("Clientes");


            // TÍTULO

            worksheet.Cell(1, 1)
                .Value = "RELATÓRIO DE CLIENTES";

            worksheet.Range(1, 1, 1, 8)
                .Merge();

            worksheet.Cell(1, 1)
                .Style.Font.Bold = true;

            worksheet.Cell(1, 1)
                .Style.Font.FontSize = 16;


            // DATA DE GERAÇÃO

            worksheet.Cell(2, 1)
                .Value = $"Gerado em: {DateTime.Now:dd/MM/yyyy HH:mm}";

            worksheet.Range(2, 1, 2, 8)
                .Merge();


            // CABEÇALHOS

            worksheet.Cell(4, 1).Value = "ID";
            worksheet.Cell(4, 2).Value = "Cliente";
            worksheet.Cell(4, 3).Value = "E-mail";
            worksheet.Cell(4, 4).Value = "Telefone";
            worksheet.Cell(4, 5).Value = "Pedidos";
            worksheet.Cell(4, 6).Value = "Itens";
            worksheet.Cell(4, 7).Value = "Total Comprado";
            worksheet.Cell(4, 8).Value = "Última Compra";


            // ESTILO DOS CABEÇALHOS

            var cabecalho =
                worksheet.Range(4, 1, 4, 8);

            cabecalho.Style.Font.Bold = true;

            cabecalho.Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;


            // DADOS

            int linha = 5;

            foreach (var cliente in clientes)
            {
                worksheet.Cell(linha, 1)
                    .Value = cliente.ClienteId;

                worksheet.Cell(linha, 2)
                    .Value = cliente.Nome;

                worksheet.Cell(linha, 3)
                    .Value = cliente.Email;

                worksheet.Cell(linha, 4)
                    .Value = cliente.Telefone;

                worksheet.Cell(linha, 5)
                    .Value = cliente.QuantidadePedidos;

                worksheet.Cell(linha, 6)
                    .Value = cliente.QuantidadeItens;

                worksheet.Cell(linha, 7)
                    .Value = cliente.TotalComprado;

                worksheet.Cell(linha, 8)
                    .Value = cliente.UltimaCompra;

                linha++;
            }


            // FORMATAÇÃO

            worksheet.Column(7)
                .Style.NumberFormat.Format =
                "R$ #,##0.00";

            worksheet.Column(8)
                .Style.DateFormat.Format =
                "dd/MM/yyyy HH:mm";


            // AJUSTAR COLUNAS

            worksheet.Columns()
                .AdjustToContents();


            // CONGELAR CABEÇALHO

            worksheet.SheetView.FreezeRows(4);


            // GERAR ARQUIVO

            using var stream = new MemoryStream();

            workbook.SaveAs(stream);

            var arquivo = stream.ToArray();


            return File(
                arquivo,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Relatorio_Clientes_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
        }

        // =========================================
        // EXPORTAR RELATÓRIO DE CLIENTES PARA PDF
        // =========================================

        [HttpGet]
        public async Task<IActionResult> ExportarClientesPdf(
            string? pesquisa,
            DateTime? dataInicio,
            DateTime? dataFim)
        {
            var query = _context.Vendas
                .Include(v => v.Cliente)
                .Include(v => v.Itens)
                .AsQueryable();


            // FILTRO POR DATA INICIAL

            if (dataInicio.HasValue)
            {
                query = query.Where(v =>
                    v.DataVenda >= dataInicio.Value);
            }


            // FILTRO POR DATA FINAL

            if (dataFim.HasValue)
            {
                var dataFinal = dataFim.Value.Date.AddDays(1);

                query = query.Where(v =>
                    v.DataVenda < dataFinal);
            }


            // FILTRO POR CLIENTE

            if (!string.IsNullOrWhiteSpace(pesquisa))
            {
                pesquisa = pesquisa.Trim();

                query = query.Where(v =>
                    v.Cliente != null &&
                    (
                        v.Cliente.Nome.Contains(pesquisa) ||
                        (v.Cliente.Email != null &&
                         v.Cliente.Email.Contains(pesquisa))
                    ));
            }


            // BUSCAR VENDAS

            var vendas = await query
                .OrderByDescending(v => v.DataVenda)
                .ToListAsync();


            // AGRUPAR CLIENTES

            var clientes = vendas
                .Where(v => v.Cliente != null)
                .GroupBy(v => new
                {
                    v.Cliente!.Id,
                    v.Cliente.Nome,
                    v.Cliente.Email,
                    v.Cliente.Telefone
                })
                .Select(g => new
                {
                    ClienteId = g.Key.Id,
                    Nome = g.Key.Nome,
                    Email = g.Key.Email,
                    Telefone = g.Key.Telefone,

                    QuantidadePedidos = g.Count(),

                    QuantidadeItens = g
                        .SelectMany(v => v.Itens)
                        .Sum(i => i.Quantidade),

                    TotalComprado = g.Sum(v => v.Total),

                    UltimaCompra = g.Max(v => v.DataVenda)
                })
                .OrderByDescending(c => c.TotalComprado)
                .ToList();


            // =========================================
            // CRIAR PDF
            // =========================================

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());

                    page.Margin(30);

                    page.DefaultTextStyle(
                        x => x.FontSize(9));


                    // =================================
                    // CABEÇALHO
                    // =================================

                    page.Header()
                        .Column(column =>
                        {
                            column.Item()
                                .Text("LojaWeb")
                                .Bold()
                                .FontSize(22);

                            column.Item()
                                .Text("Relatório de Clientes")
                                .Bold()
                                .FontSize(16);

                            column.Item()
                                .Text(
                                    $"Gerado em: {DateTime.Now:dd/MM/yyyy HH:mm}")
                                .FontSize(9);

                            if (!string.IsNullOrWhiteSpace(pesquisa))
                            {
                                column.Item()
                                    .Text(
                                        $"Pesquisa: {pesquisa}")
                                    .FontSize(9);
                            }

                            if (dataInicio.HasValue ||
                                dataFim.HasValue)
                            {
                                column.Item()
                                    .Text(
                                        $"Período: " +
                                        $"{(dataInicio.HasValue ? dataInicio.Value.ToString("dd/MM/yyyy") : "...")} " +
                                        $"até " +
                                        $"{(dataFim.HasValue ? dataFim.Value.ToString("dd/MM/yyyy") : "...")}")
                                    .FontSize(9);
                            }
                        });


                    // =================================
                    // CONTEÚDO
                    // =================================

                    page.Content()
                        .PaddingVertical(20)
                        .Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(35);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(1.4f);
                                columns.ConstantColumn(55);
                                columns.ConstantColumn(45);
                                columns.RelativeColumn(1.3f);
                                columns.RelativeColumn(1.4f);
                            });


                            // CABEÇALHO DA TABELA

                            table.Header(header =>
                            {
                                header.Cell()
                                    .Background(Colors.Grey.Lighten2)
                                    .Padding(5)
                                    .Text("ID")
                                    .Bold();

                                header.Cell()
                                    .Background(Colors.Grey.Lighten2)
                                    .Padding(5)
                                    .Text("Cliente")
                                    .Bold();

                                header.Cell()
                                    .Background(Colors.Grey.Lighten2)
                                    .Padding(5)
                                    .Text("E-mail")
                                    .Bold();

                                header.Cell()
                                    .Background(Colors.Grey.Lighten2)
                                    .Padding(5)
                                    .Text("Telefone")
                                    .Bold();

                                header.Cell()
                                    .Background(Colors.Grey.Lighten2)
                                    .Padding(5)
                                    .Text("Pedidos")
                                    .Bold();

                                header.Cell()
                                    .Background(Colors.Grey.Lighten2)
                                    .Padding(5)
                                    .Text("Itens")
                                    .Bold();

                                header.Cell()
                                    .Background(Colors.Grey.Lighten2)
                                    .Padding(5)
                                    .Text("Total")
                                    .Bold();

                                header.Cell()
                                    .Background(Colors.Grey.Lighten2)
                                    .Padding(5)
                                    .Text("Última compra")
                                    .Bold();
                            });


                            // DADOS

                            foreach (var cliente in clientes)
                            {
                                table.Cell()
                                    .Padding(5)
                                    .Text(cliente.ClienteId.ToString());

                                table.Cell()
                                    .Padding(5)
                                    .Text(cliente.Nome);

                                table.Cell()
                                    .Padding(5)
                                    .Text(cliente.Email ?? "-");

                                table.Cell()
                                    .Padding(5)
                                    .Text(cliente.Telefone ?? "-");

                                table.Cell()
                                    .Padding(5)
                                    .AlignCenter()
                                    .Text(
                                        cliente.QuantidadePedidos.ToString());

                                table.Cell()
                                    .Padding(5)
                                    .AlignCenter()
                                    .Text(
                                        cliente.QuantidadeItens.ToString());

                                table.Cell()
                                    .Padding(5)
                                    .Text(
                                        cliente.TotalComprado.ToString("C"));

                                table.Cell()
                                    .Padding(5)
                                    .Text(
                                        cliente.UltimaCompra
                                            .ToString("dd/MM/yyyy HH:mm"));
                            }
                        });


                    // =================================
                    // RODAPÉ
                    // =================================

                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span("LojaWeb - Relatório de Clientes | Página ");

                            text.CurrentPageNumber();

                            text.Span(" de ");

                            text.TotalPages();
                        });
                });
            });


            // =========================================
            // GERAR PDF
            // =========================================

            var pdf = document.GeneratePdf();


            return File(
                pdf,
                "application/pdf",
                $"Relatorio_Clientes_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
        }

        // =========================================
        // EXPORTAR RELATÓRIO DE VENDAS PARA EXCEL
        // =========================================

        [HttpGet]
        public async Task<IActionResult> ExportarVendasExcel(
            DateTime? dataInicial,
            DateTime? dataFinal,
            string? formaPagamento,
            string? status)
        {
            var query = _context.Vendas
                .Include(v => v.Cliente)
                .Include(v => v.Itens)
                .AsQueryable();


            // FILTRO DATA INICIAL

            if (dataInicial.HasValue)
            {
                query = query.Where(v =>
                    v.DataVenda >= dataInicial.Value);
            }


            // FILTRO DATA FINAL

            if (dataFinal.HasValue)
            {
                var dataFim = dataFinal.Value.Date.AddDays(1);

                query = query.Where(v =>
                    v.DataVenda < dataFim);
            }


            // FILTRO FORMA DE PAGAMENTO

            if (!string.IsNullOrWhiteSpace(formaPagamento))
            {
                query = query.Where(v =>
                    v.FormaPagamento == formaPagamento);
            }


            // FILTRO STATUS

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(v =>
                    v.Status == status);
            }


            var vendas = await query
                .OrderByDescending(v => v.DataVenda)
                .ToListAsync();


            // =========================================
            // CRIAR EXCEL
            // =========================================

            using var workbook = new XLWorkbook();

            var worksheet =
                workbook.Worksheets.Add("Vendas");


            // =========================================
            // TÍTULO
            // =========================================

            worksheet.Cell(1, 1)
                .Value = "RELATÓRIO DE VENDAS";

            worksheet.Range(1, 1, 1, 7)
                .Merge();

            worksheet.Cell(1, 1)
                .Style.Font.Bold = true;

            worksheet.Cell(1, 1)
                .Style.Font.FontSize = 16;


            worksheet.Cell(2, 1)
                .Value =
                    $"Gerado em: {DateTime.Now:dd/MM/yyyy HH:mm}";

            worksheet.Range(2, 1, 2, 7)
                .Merge();


            // =========================================
            // FILTROS UTILIZADOS
            // =========================================

            int linhaFiltro = 3;

            if (dataInicial.HasValue)
            {
                worksheet.Cell(linhaFiltro, 1)
                    .Value =
                    $"Data inicial: {dataInicial.Value:dd/MM/yyyy}";

                linhaFiltro++;
            }

            if (dataFinal.HasValue)
            {
                worksheet.Cell(linhaFiltro, 1)
                    .Value =
                    $"Data final: {dataFinal.Value:dd/MM/yyyy}";

                linhaFiltro++;
            }

            if (!string.IsNullOrWhiteSpace(formaPagamento))
            {
                worksheet.Cell(linhaFiltro, 1)
                    .Value =
                    $"Forma de pagamento: {formaPagamento}";

                linhaFiltro++;
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                worksheet.Cell(linhaFiltro, 1)
                    .Value =
                    $"Status: {status}";

                linhaFiltro++;
            }


            // =========================================
            // CABEÇALHO DA TABELA
            // =========================================

            int linhaCabecalho = linhaFiltro + 1;

            worksheet.Cell(linhaCabecalho, 1)
                .Value = "Venda";

            worksheet.Cell(linhaCabecalho, 2)
                .Value = "Data";

            worksheet.Cell(linhaCabecalho, 3)
                .Value = "Cliente";

            worksheet.Cell(linhaCabecalho, 4)
                .Value = "Pagamento";

            worksheet.Cell(linhaCabecalho, 5)
                .Value = "Itens";

            worksheet.Cell(linhaCabecalho, 6)
                .Value = "Total";

            worksheet.Cell(linhaCabecalho, 7)
                .Value = "Status";


            var cabecalho = worksheet.Range(
                linhaCabecalho,
                1,
                linhaCabecalho,
                7);

            cabecalho.Style.Font.Bold = true;

            cabecalho.Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;


            // =========================================
            // DADOS
            // =========================================

            int linha = linhaCabecalho + 1;

            foreach (var venda in vendas)
            {
                worksheet.Cell(linha, 1)
                    .Value = venda.Id;

                worksheet.Cell(linha, 2)
                    .Value = venda.DataVenda;

                worksheet.Cell(linha, 3)
                    .Value =
                    venda.Cliente?.Nome ??
                    "Cliente não encontrado";

                worksheet.Cell(linha, 4)
                    .Value = venda.FormaPagamento;

                worksheet.Cell(linha, 5)
                    .Value = venda.Itens.Sum(
                        i => i.Quantidade);

                worksheet.Cell(linha, 6)
                    .Value = venda.Total;

                worksheet.Cell(linha, 7)
                    .Value = venda.Status;

                linha++;
            }


            // =========================================
            // FORMATAÇÃO
            // =========================================

            worksheet.Column(2)
                .Style.DateFormat.Format =
                "dd/MM/yyyy HH:mm";

            worksheet.Column(6)
                .Style.NumberFormat.Format =
                "R$ #,##0.00";


            // =========================================
            // AJUSTAR COLUNAS
            // =========================================

            worksheet.Columns()
                .AdjustToContents();


            // =========================================
            // CONGELAR CABEÇALHO
            // =========================================

            worksheet.SheetView
                .FreezeRows(linhaCabecalho);


            // =========================================
            // GERAR ARQUIVO
            // =========================================

            using var stream = new MemoryStream();

            workbook.SaveAs(stream);

            var arquivo = stream.ToArray();


            return File(
                arquivo,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Relatorio_Vendas_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
        }

        // =========================================
        // EXPORTAR RELATÓRIO DE VENDAS PARA PDF
        // =========================================

        [HttpGet]
        public async Task<IActionResult> ExportarVendasPdf(
            DateTime? dataInicial,
            DateTime? dataFinal,
            string? formaPagamento,
            string? status)
        {
            var query = _context.Vendas
                .Include(v => v.Cliente)
                .Include(v => v.Itens)
                .AsQueryable();


            // FILTRO DATA INICIAL

            if (dataInicial.HasValue)
            {
                query = query.Where(v =>
                    v.DataVenda >= dataInicial.Value);
            }


            // FILTRO DATA FINAL

            if (dataFinal.HasValue)
            {
                var dataFim = dataFinal.Value.Date.AddDays(1);

                query = query.Where(v =>
                    v.DataVenda < dataFim);
            }


            // FILTRO FORMA DE PAGAMENTO

            if (!string.IsNullOrWhiteSpace(formaPagamento))
            {
                query = query.Where(v =>
                    v.FormaPagamento == formaPagamento);
            }


            // FILTRO STATUS

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(v =>
                    v.Status == status);
            }


            var vendas = await query
                .OrderByDescending(v => v.DataVenda)
                .ToListAsync();


            // =========================================
            // CRIAR PDF
            // =========================================

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());

                    page.Margin(30);

                    page.DefaultTextStyle(
                        x => x.FontSize(8));


                    // =================================
                    // CABEÇALHO
                    // =================================

                    page.Header()
                        .Column(column =>
                        {
                            column.Item()
                                .Text("LojaWeb")
                                .Bold()
                                .FontSize(22);

                            column.Item()
                                .Text("Relatório de Vendas")
                                .Bold()
                                .FontSize(16);

                            column.Item()
                                .Text(
                                    $"Gerado em: {DateTime.Now:dd/MM/yyyy HH:mm}")
                                .FontSize(9);


                            if (dataInicial.HasValue ||
                                dataFinal.HasValue)
                            {
                                column.Item()
                                    .Text(
                                        $"Período: " +
                                        $"{(dataInicial.HasValue ? dataInicial.Value.ToString("dd/MM/yyyy") : "...")} " +
                                        $"até " +
                                        $"{(dataFinal.HasValue ? dataFinal.Value.ToString("dd/MM/yyyy") : "...")}")
                                    .FontSize(9);
                            }


                            if (!string.IsNullOrWhiteSpace(
                                formaPagamento))
                            {
                                column.Item()
                                    .Text(
                                        $"Forma de pagamento: {formaPagamento}")
                                    .FontSize(9);
                            }


                            if (!string.IsNullOrWhiteSpace(status))
                            {
                                column.Item()
                                    .Text(
                                        $"Status: {status}")
                                    .FontSize(9);
                            }
                        });


                    // =================================
                    // TABELA
                    // =================================

                    page.Content()
                        .PaddingVertical(20)
                        .Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(40);
                                columns.ConstantColumn(85);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(1.5f);
                                columns.ConstantColumn(45);
                                columns.RelativeColumn(1.2f);
                                columns.RelativeColumn(1.2f);
                            });


                            // CABEÇALHO

                            table.Header(header =>
                            {
                                header.Cell()
                                    .Background(Colors.Grey.Lighten2)
                                    .Padding(5)
                                    .Text("Venda")
                                    .Bold();

                                header.Cell()
                                    .Background(Colors.Grey.Lighten2)
                                    .Padding(5)
                                    .Text("Data")
                                    .Bold();

                                header.Cell()
                                    .Background(Colors.Grey.Lighten2)
                                    .Padding(5)
                                    .Text("Cliente")
                                    .Bold();

                                header.Cell()
                                    .Background(Colors.Grey.Lighten2)
                                    .Padding(5)
                                    .Text("Pagamento")
                                    .Bold();

                                header.Cell()
                                    .Background(Colors.Grey.Lighten2)
                                    .Padding(5)
                                    .Text("Itens")
                                    .Bold();

                                header.Cell()
                                    .Background(Colors.Grey.Lighten2)
                                    .Padding(5)
                                    .Text("Total")
                                    .Bold();

                                header.Cell()
                                    .Background(Colors.Grey.Lighten2)
                                    .Padding(5)
                                    .Text("Status")
                                    .Bold();
                            });


                            // DADOS

                            foreach (var venda in vendas)
                            {
                                table.Cell()
                                    .Padding(5)
                                    .Text(venda.Id.ToString());

                                table.Cell()
                                    .Padding(5)
                                    .Text(
                                        venda.DataVenda
                                            .ToString("dd/MM/yyyy HH:mm"));

                                table.Cell()
                                    .Padding(5)
                                    .Text(
                                        venda.Cliente?.Nome ??
                                        "Cliente não encontrado");

                                table.Cell()
                                    .Padding(5)
                                    .Text(
                                        venda.FormaPagamento);

                                table.Cell()
                                    .Padding(5)
                                    .AlignCenter()
                                    .Text(
                                        venda.Itens
                                            .Sum(i => i.Quantidade)
                                            .ToString());

                                table.Cell()
                                    .Padding(5)
                                    .Text(
                                        venda.Total.ToString("C"));

                                table.Cell()
                                    .Padding(5)
                                    .Text(
                                        venda.Status);
                            }
                        });


                    // =================================
                    // RODAPÉ
                    // =================================

                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span(
                                "LojaWeb - Relatório de Vendas | Página ");

                            text.CurrentPageNumber();

                            text.Span(" de ");

                            text.TotalPages();
                        });
                });
            });


            var pdf = document.GeneratePdf();


            return File(
                pdf,
                "application/pdf",
                $"Relatorio_Vendas_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
        }

        // =========================================
        // EXPORTAR RELATÓRIO DE ESTOQUE PARA EXCEL
        // =========================================

        [HttpGet]
        public async Task<IActionResult> ExportarEstoqueExcel(
            string? pesquisa,
            int? produtoId,
            int? marcaId,
            string? tamanho,
            string? cor,
            string? situacao)
        {
            var query = _context.Estoques
                .Include(e => e.Produto)
                .Include(e => e.Marca)
                .AsQueryable();


            // PESQUISA

            if (!string.IsNullOrWhiteSpace(pesquisa))
            {
                pesquisa = pesquisa.Trim();

                query = query.Where(e =>
                    (e.Produto != null &&
                     e.Produto.Nome.Contains(pesquisa)) ||
                    (e.Marca != null &&
                     e.Marca.Nome.Contains(pesquisa)) ||
                    (e.Tamanho != null &&
                     e.Tamanho.Contains(pesquisa)) ||
                    (e.Cor != null &&
                     e.Cor.Contains(pesquisa)));
            }


            // PRODUTO

            if (produtoId.HasValue)
            {
                query = query.Where(e =>
                    e.ProdutoId == produtoId.Value);
            }


            // MARCA

            if (marcaId.HasValue)
            {
                query = query.Where(e =>
                    e.MarcaId == marcaId.Value);
            }


            // TAMANHO

            if (!string.IsNullOrWhiteSpace(tamanho))
            {
                query = query.Where(e =>
                    e.Tamanho == tamanho);
            }


            // COR

            if (!string.IsNullOrWhiteSpace(cor))
            {
                query = query.Where(e =>
                    e.Cor == cor);
            }


            // SITUAÇÃO

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


            var estoques = await query
                .OrderBy(e => e.Produto!.Nome)
                .ThenBy(e => e.Tamanho)
                .ThenBy(e => e.Cor)
                .ToListAsync();


            // =========================================
            // CRIAR EXCEL
            // =========================================

            using var workbook = new XLWorkbook();

            var worksheet =
                workbook.Worksheets.Add("Estoque");


            // TÍTULO

            worksheet.Cell(1, 1)
                .Value = "RELATÓRIO DE ESTOQUE";

            worksheet.Range(1, 1, 1, 8)
                .Merge();

            worksheet.Cell(1, 1)
                .Style.Font.Bold = true;

            worksheet.Cell(1, 1)
                .Style.Font.FontSize = 16;


            // DATA DE GERAÇÃO

            worksheet.Cell(2, 1)
                .Value =
                $"Gerado em: {DateTime.Now:dd/MM/yyyy HH:mm}";

            worksheet.Range(2, 1, 2, 8)
                .Merge();


            // FILTROS

            int linhaFiltro = 3;

            if (!string.IsNullOrWhiteSpace(pesquisa))
            {
                worksheet.Cell(linhaFiltro, 1)
                    .Value = $"Pesquisa: {pesquisa}";

                linhaFiltro++;
            }

            if (produtoId.HasValue)
            {
                worksheet.Cell(linhaFiltro, 1)
                    .Value = $"Produto ID: {produtoId}";

                linhaFiltro++;
            }

            if (marcaId.HasValue)
            {
                worksheet.Cell(linhaFiltro, 1)
                    .Value = $"Marca ID: {marcaId}";

                linhaFiltro++;
            }

            if (!string.IsNullOrWhiteSpace(tamanho))
            {
                worksheet.Cell(linhaFiltro, 1)
                    .Value = $"Tamanho: {tamanho}";

                linhaFiltro++;
            }

            if (!string.IsNullOrWhiteSpace(cor))
            {
                worksheet.Cell(linhaFiltro, 1)
                    .Value = $"Cor: {cor}";

                linhaFiltro++;
            }

            if (!string.IsNullOrWhiteSpace(situacao))
            {
                var descricaoSituacao = situacao switch
                {
                    "EmEstoque" => "Em estoque",
                    "EstoqueBaixo" => "Estoque baixo",
                    "Esgotado" => "Esgotado",
                    _ => situacao
                };

                worksheet.Cell(linhaFiltro, 1)
                    .Value = $"Situação: {descricaoSituacao}";

                linhaFiltro++;
            }


            // =========================================
            // CABEÇALHO
            // =========================================

            int linhaCabecalho = linhaFiltro + 1;

            worksheet.Cell(linhaCabecalho, 1)
                .Value = "Produto";

            worksheet.Cell(linhaCabecalho, 2)
                .Value = "Marca";

            worksheet.Cell(linhaCabecalho, 3)
                .Value = "Tamanho";

            worksheet.Cell(linhaCabecalho, 4)
                .Value = "Cor";

            worksheet.Cell(linhaCabecalho, 5)
                .Value = "Quantidade";

            worksheet.Cell(linhaCabecalho, 6)
                .Value = "Custo Médio";

            worksheet.Cell(linhaCabecalho, 7)
                .Value = "Valor Estoque";

            worksheet.Cell(linhaCabecalho, 8)
                .Value = "Situação";


            var cabecalho = worksheet.Range(
                linhaCabecalho,
                1,
                linhaCabecalho,
                8);

            cabecalho.Style.Font.Bold = true;

            cabecalho.Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;


            // =========================================
            // DADOS
            // =========================================

            int linha = linhaCabecalho + 1;

            foreach (var item in estoques)
            {
                decimal valorItem =
                    item.Quantidade *
                    item.CustoMedio;


                string situacaoItem;

                if (item.Quantidade == 0)
                {
                    situacaoItem = "Esgotado";
                }
                else if (item.Quantidade <= 5)
                {
                    situacaoItem = "Estoque baixo";
                }
                else
                {
                    situacaoItem = "Em estoque";
                }


                worksheet.Cell(linha, 1)
                    .Value =
                    item.Produto?.Nome ??
                    "Produto não encontrado";

                worksheet.Cell(linha, 2)
                    .Value =
                    item.Marca?.Nome ??
                    "Marca não encontrada";

                worksheet.Cell(linha, 3)
                    .Value = item.Tamanho;

                worksheet.Cell(linha, 4)
                    .Value = item.Cor;

                worksheet.Cell(linha, 5)
                    .Value = item.Quantidade;

                worksheet.Cell(linha, 6)
                    .Value = item.CustoMedio;

                worksheet.Cell(linha, 7)
                    .Value = valorItem;

                worksheet.Cell(linha, 8)
                    .Value = situacaoItem;

                linha++;
            }


            // FORMATAÇÃO

            worksheet.Column(6)
                .Style.NumberFormat.Format =
                "R$ #,##0.00";

            worksheet.Column(7)
                .Style.NumberFormat.Format =
                "R$ #,##0.00";


            // AJUSTAR COLUNAS

            worksheet.Columns()
                .AdjustToContents();


            // CONGELAR CABEÇALHO

            worksheet.SheetView
                .FreezeRows(linhaCabecalho);


            // GERAR ARQUIVO

            using var stream = new MemoryStream();

            workbook.SaveAs(stream);

            var arquivo = stream.ToArray();


            return File(
                arquivo,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Relatorio_Estoque_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
        }

        // =========================================
        // EXPORTAR RELATÓRIO DE ESTOQUE PARA PDF
        // =========================================

        [HttpGet]
        public async Task<IActionResult> ExportarEstoquePdf(
            string? pesquisa,
            int? produtoId,
            int? marcaId,
            string? tamanho,
            string? cor,
            string? situacao)
        {
            var query = _context.Estoques
                .Include(e => e.Produto)
                .Include(e => e.Marca)
                .AsQueryable();


            // PESQUISA

            if (!string.IsNullOrWhiteSpace(pesquisa))
            {
                pesquisa = pesquisa.Trim();

                query = query.Where(e =>
                    (e.Produto != null &&
                     e.Produto.Nome.Contains(pesquisa)) ||
                    (e.Marca != null &&
                     e.Marca.Nome.Contains(pesquisa)) ||
                    (e.Tamanho != null &&
                     e.Tamanho.Contains(pesquisa)) ||
                    (e.Cor != null &&
                     e.Cor.Contains(pesquisa)));
            }


            // PRODUTO

            if (produtoId.HasValue)
            {
                query = query.Where(e =>
                    e.ProdutoId == produtoId.Value);
            }


            // MARCA

            if (marcaId.HasValue)
            {
                query = query.Where(e =>
                    e.MarcaId == marcaId.Value);
            }


            // TAMANHO

            if (!string.IsNullOrWhiteSpace(tamanho))
            {
                query = query.Where(e =>
                    e.Tamanho == tamanho);
            }


            // COR

            if (!string.IsNullOrWhiteSpace(cor))
            {
                query = query.Where(e =>
                    e.Cor == cor);
            }


            // SITUAÇÃO

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


            var estoques = await query
                .OrderBy(e => e.Produto!.Nome)
                .ThenBy(e => e.Tamanho)
                .ThenBy(e => e.Cor)
                .ToListAsync();


            // =========================================
            // CRIAR PDF
            // =========================================

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());

                    page.Margin(30);

                    page.DefaultTextStyle(
                        x => x.FontSize(8));


                    // =================================
                    // CABEÇALHO
                    // =================================

                    page.Header()
                        .Column(column =>
                        {
                            column.Item()
                                .Text("LojaWeb")
                                .Bold()
                                .FontSize(22);

                            column.Item()
                                .Text("Relatório de Estoque")
                                .Bold()
                                .FontSize(16);

                            column.Item()
                                .Text(
                                    $"Gerado em: {DateTime.Now:dd/MM/yyyy HH:mm}")
                                .FontSize(9);


                            if (!string.IsNullOrWhiteSpace(pesquisa))
                            {
                                column.Item()
                                    .Text($"Pesquisa: {pesquisa}")
                                    .FontSize(9);
                            }


                            if (produtoId.HasValue)
                            {
                                column.Item()
                                    .Text($"Produto ID: {produtoId}")
                                    .FontSize(9);
                            }


                            if (marcaId.HasValue)
                            {
                                column.Item()
                                    .Text($"Marca ID: {marcaId}")
                                    .FontSize(9);
                            }


                            if (!string.IsNullOrWhiteSpace(tamanho))
                            {
                                column.Item()
                                    .Text($"Tamanho: {tamanho}")
                                    .FontSize(9);
                            }


                            if (!string.IsNullOrWhiteSpace(cor))
                            {
                                column.Item()
                                    .Text($"Cor: {cor}")
                                    .FontSize(9);
                            }


                            if (!string.IsNullOrWhiteSpace(situacao))
                            {
                                var descricaoSituacao =
                                    situacao switch
                                    {
                                        "EmEstoque" => "Em estoque",
                                        "EstoqueBaixo" => "Estoque baixo",
                                        "Esgotado" => "Esgotado",
                                        _ => situacao
                                    };

                                column.Item()
                                    .Text(
                                        $"Situação: {descricaoSituacao}")
                                    .FontSize(9);
                            }
                        });


                    // =================================
                    // TABELA
                    // =================================

                    page.Content()
                        .PaddingVertical(20)
                        .Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2.2f);
                                columns.RelativeColumn(1.6f);
                                columns.ConstantColumn(55);
                                columns.RelativeColumn(1.2f);
                                columns.ConstantColumn(60);
                                columns.RelativeColumn(1.2f);
                                columns.RelativeColumn(1.3f);
                                columns.RelativeColumn(1.2f);
                            });


                            // CABEÇALHO

                            table.Header(header =>
                            {
                                header.Cell()
                                    .Background(Colors.Grey.Lighten2)
                                    .Padding(5)
                                    .Text("Produto")
                                    .Bold();

                                header.Cell()
                                    .Background(Colors.Grey.Lighten2)
                                    .Padding(5)
                                    .Text("Marca")
                                    .Bold();

                                header.Cell()
                                    .Background(Colors.Grey.Lighten2)
                                    .Padding(5)
                                    .Text("Tamanho")
                                    .Bold();

                                header.Cell()
                                    .Background(Colors.Grey.Lighten2)
                                    .Padding(5)
                                    .Text("Cor")
                                    .Bold();

                                header.Cell()
                                    .Background(Colors.Grey.Lighten2)
                                    .Padding(5)
                                    .Text("Quantidade")
                                    .Bold();

                                header.Cell()
                                    .Background(Colors.Grey.Lighten2)
                                    .Padding(5)
                                    .Text("Custo médio")
                                    .Bold();

                                header.Cell()
                                    .Background(Colors.Grey.Lighten2)
                                    .Padding(5)
                                    .Text("Valor estoque")
                                    .Bold();

                                header.Cell()
                                    .Background(Colors.Grey.Lighten2)
                                    .Padding(5)
                                    .Text("Situação")
                                    .Bold();
                            });


                            // DADOS

                            foreach (var item in estoques)
                            {
                                decimal valorItem =
                                    item.Quantidade *
                                    item.CustoMedio;


                                string situacaoItem;

                                if (item.Quantidade == 0)
                                {
                                    situacaoItem = "Esgotado";
                                }
                                else if (item.Quantidade <= 5)
                                {
                                    situacaoItem = "Estoque baixo";
                                }
                                else
                                {
                                    situacaoItem = "Em estoque";
                                }


                                table.Cell()
                                    .Padding(5)
                                    .Text(
                                        item.Produto?.Nome ??
                                        "Produto não encontrado");

                                table.Cell()
                                    .Padding(5)
                                    .Text(
                                        item.Marca?.Nome ??
                                        "Marca não encontrada");

                                table.Cell()
                                    .Padding(5)
                                    .Text(
                                        item.Tamanho ?? "-");

                                table.Cell()
                                    .Padding(5)
                                    .Text(
                                        item.Cor ?? "-");

                                table.Cell()
                                    .Padding(5)
                                    .AlignCenter()
                                    .Text(
                                        item.Quantidade.ToString());

                                table.Cell()
                                    .Padding(5)
                                    .Text(
                                        item.CustoMedio.ToString("C"));

                                table.Cell()
                                    .Padding(5)
                                    .Text(
                                        valorItem.ToString("C"));

                                table.Cell()
                                    .Padding(5)
                                    .Text(
                                        situacaoItem);
                            }
                        });


                    // =================================
                    // RODAPÉ
                    // =================================

                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span(
                                "LojaWeb - Relatório de Estoque | Página ");

                            text.CurrentPageNumber();

                            text.Span(" de ");

                            text.TotalPages();
                        });
                });
            });


            var pdf = document.GeneratePdf();


            return File(
                pdf,
                "application/pdf",
                $"Relatorio_Estoque_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
        }

        // =========================================
        // EXPORTAR MOVIMENTAÇÕES PARA EXCEL
        // =========================================

        [HttpGet]
        public async Task<IActionResult> ExportarMovimentacoesExcel(
            DateTime? dataInicial,
            DateTime? dataFinal,
            int? produtoId,
            int? marcaId,
            int? fornecedorId,
            string? tipo)
        {
            var query = _context.MovimentacoesEstoque
                .Include(m => m.Produto)
                .Include(m => m.Marca)
                .Include(m => m.Fornecedor)
                .AsQueryable();


            // DATA INICIAL

            if (dataInicial.HasValue)
            {
                query = query.Where(m =>
                    m.Data >= dataInicial.Value);
            }


            // DATA FINAL

            if (dataFinal.HasValue)
            {
                var dataFim = dataFinal.Value.Date.AddDays(1);

                query = query.Where(m =>
                    m.Data < dataFim);
            }


            // PRODUTO

            if (produtoId.HasValue)
            {
                query = query.Where(m =>
                    m.ProdutoId == produtoId.Value);
            }


            // MARCA

            if (marcaId.HasValue)
            {
                query = query.Where(m =>
                    m.MarcaId == marcaId.Value);
            }


            // FORNECEDOR

            if (fornecedorId.HasValue)
            {
                query = query.Where(m =>
                    m.FornecedorId == fornecedorId.Value);
            }


            // TIPO

            if (!string.IsNullOrWhiteSpace(tipo))
            {
                if (tipo == "Entrada")
                {
                    query = query.Where(m =>
                        m.Tipo == TipoMovimentacao.Entrada);
                }
                else if (tipo == "Saida")
                {
                    query = query.Where(m =>
                        m.Tipo == TipoMovimentacao.Saida);
                }
            }


            var movimentacoes = await query
                .OrderByDescending(m => m.Data)
                .ToListAsync();


            // =========================================
            // CRIAR EXCEL
            // =========================================

            using var workbook = new XLWorkbook();

            var worksheet =
                workbook.Worksheets.Add("Movimentações");


            // TÍTULO

            worksheet.Cell(1, 1)
                .Value = "RELATÓRIO DE MOVIMENTAÇÕES";

            worksheet.Range(1, 1, 1, 10)
                .Merge();

            worksheet.Cell(1, 1)
                .Style.Font.Bold = true;

            worksheet.Cell(1, 1)
                .Style.Font.FontSize = 16;


            // DATA DE GERAÇÃO

            worksheet.Cell(2, 1)
                .Value =
                $"Gerado em: {DateTime.Now:dd/MM/yyyy HH:mm}";

            worksheet.Range(2, 1, 2, 10)
                .Merge();


            // =========================================
            // FILTROS
            // =========================================

            int linhaFiltro = 3;

            if (dataInicial.HasValue)
            {
                worksheet.Cell(linhaFiltro, 1)
                    .Value =
                    $"Data inicial: {dataInicial.Value:dd/MM/yyyy}";

                linhaFiltro++;
            }

            if (dataFinal.HasValue)
            {
                worksheet.Cell(linhaFiltro, 1)
                    .Value =
                    $"Data final: {dataFinal.Value:dd/MM/yyyy}";

                linhaFiltro++;
            }

            if (produtoId.HasValue)
            {
                worksheet.Cell(linhaFiltro, 1)
                    .Value =
                    $"Produto ID: {produtoId.Value}";

                linhaFiltro++;
            }

            if (marcaId.HasValue)
            {
                worksheet.Cell(linhaFiltro, 1)
                    .Value =
                    $"Marca ID: {marcaId.Value}";

                linhaFiltro++;
            }

            if (fornecedorId.HasValue)
            {
                worksheet.Cell(linhaFiltro, 1)
                    .Value =
                    $"Fornecedor ID: {fornecedorId.Value}";

                linhaFiltro++;
            }

            if (!string.IsNullOrWhiteSpace(tipo))
            {
                worksheet.Cell(linhaFiltro, 1)
                    .Value =
                    $"Tipo: {(tipo == "Saida" ? "Saída" : tipo)}";

                linhaFiltro++;
            }


            // =========================================
            // CABEÇALHO
            // =========================================

            int linhaCabecalho = linhaFiltro + 1;

            worksheet.Cell(linhaCabecalho, 1)
                .Value = "Data";

            worksheet.Cell(linhaCabecalho, 2)
                .Value = "Produto";

            worksheet.Cell(linhaCabecalho, 3)
                .Value = "Marca";

            worksheet.Cell(linhaCabecalho, 4)
                .Value = "Fornecedor";

            worksheet.Cell(linhaCabecalho, 5)
                .Value = "Tamanho";

            worksheet.Cell(linhaCabecalho, 6)
                .Value = "Cor";

            worksheet.Cell(linhaCabecalho, 7)
                .Value = "Tipo";

            worksheet.Cell(linhaCabecalho, 8)
                .Value = "Quantidade";

            worksheet.Cell(linhaCabecalho, 9)
                .Value = "Preço de compra";

            worksheet.Cell(linhaCabecalho, 10)
                .Value = "Observação";


            var cabecalho = worksheet.Range(
                linhaCabecalho,
                1,
                linhaCabecalho,
                10);

            cabecalho.Style.Font.Bold = true;

            cabecalho.Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;


            // =========================================
            // DADOS
            // =========================================

            int linha = linhaCabecalho + 1;

            foreach (var item in movimentacoes)
            {
                worksheet.Cell(linha, 1)
                    .Value = item.Data;

                worksheet.Cell(linha, 2)
                    .Value =
                    item.Produto?.Nome ??
                    "Produto não encontrado";

                worksheet.Cell(linha, 3)
                    .Value =
                    item.Marca?.Nome ??
                    "Marca não encontrada";

                worksheet.Cell(linha, 4)
                    .Value =
                    item.Fornecedor?.Nome ??
                    "Não informado";

                worksheet.Cell(linha, 5)
                    .Value = item.Tamanho;

                worksheet.Cell(linha, 6)
                    .Value = item.Cor;

                worksheet.Cell(linha, 7)
                    .Value =
                    item.Tipo == TipoMovimentacao.Entrada
                        ? "Entrada"
                        : "Saída";

                worksheet.Cell(linha, 8)
                    .Value = item.Quantidade;

                worksheet.Cell(linha, 9)
                    .Value = item.PrecoCompraUnitario;

                worksheet.Cell(linha, 10)
                    .Value =
                    string.IsNullOrWhiteSpace(item.Observacao)
                        ? "-"
                        : item.Observacao;

                linha++;
            }


            // =========================================
            // FORMATAÇÃO
            // =========================================

            worksheet.Column(1)
                .Style.DateFormat.Format =
                "dd/MM/yyyy HH:mm";

            worksheet.Column(9)
                .Style.NumberFormat.Format =
                "R$ #,##0.00";


            // =========================================
            // AJUSTAR COLUNAS
            // =========================================

            worksheet.Columns()
                .AdjustToContents();


            // =========================================
            // CONGELAR CABEÇALHO
            // =========================================

            worksheet.SheetView
                .FreezeRows(linhaCabecalho);


            // =========================================
            // GERAR ARQUIVO
            // =========================================

            using var stream = new MemoryStream();

            workbook.SaveAs(stream);

            var arquivo = stream.ToArray();


            return File(
                arquivo,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Relatorio_Movimentacoes_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
        }

        // =========================================
        // EXPORTAR MOVIMENTAÇÕES PARA PDF
        // =========================================

        [HttpGet]
        public async Task<IActionResult> ExportarMovimentacoesPdf(
            DateTime? dataInicial,
            DateTime? dataFinal,
            int? produtoId,
            int? marcaId,
            int? fornecedorId,
            string? tipo)
        {
            var query = _context.MovimentacoesEstoque
                .Include(m => m.Produto)
                .Include(m => m.Marca)
                .Include(m => m.Fornecedor)
                .AsQueryable();

            if (dataInicial.HasValue)
            {
                query = query.Where(m =>
                    m.Data >= dataInicial.Value);
            }

            if (dataFinal.HasValue)
            {
                var dataFim = dataFinal.Value.Date.AddDays(1);

                query = query.Where(m =>
                    m.Data < dataFim);
            }

            if (produtoId.HasValue)
            {
                query = query.Where(m =>
                    m.ProdutoId == produtoId.Value);
            }

            if (marcaId.HasValue)
            {
                query = query.Where(m =>
                    m.MarcaId == marcaId.Value);
            }

            if (fornecedorId.HasValue)
            {
                query = query.Where(m =>
                    m.FornecedorId == fornecedorId.Value);
            }

            if (!string.IsNullOrWhiteSpace(tipo))
            {
                if (tipo == "Entrada")
                {
                    query = query.Where(m =>
                        m.Tipo == TipoMovimentacao.Entrada);
                }
                else if (tipo == "Saida")
                {
                    query = query.Where(m =>
                        m.Tipo == TipoMovimentacao.Saida);
                }
            }

            var movimentacoes = await query
                .OrderByDescending(m => m.Data)
                .ToListAsync();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());

                    page.Margin(25);

                    page.DefaultTextStyle(
                        x => x.FontSize(7));

                    // =================================
                    // CABEÇALHO
                    // =================================

                    page.Header()
                        .Column(column =>
                        {
                            column.Item()
                                .Text("LojaWeb")
                                .Bold()
                                .FontSize(22);

                            column.Item()
                                .Text("Relatório de Movimentações")
                                .Bold()
                                .FontSize(16);

                            column.Item()
                                .Text(
                                    $"Gerado em: {DateTime.Now:dd/MM/yyyy HH:mm}")
                                .FontSize(9);

                            if (dataInicial.HasValue ||
                                dataFinal.HasValue)
                            {
                                column.Item()
                                    .Text(
                                        $"Período: " +
                                        $"{(dataInicial.HasValue ? dataInicial.Value.ToString("dd/MM/yyyy") : "...")} " +
                                        $"até " +
                                        $"{(dataFinal.HasValue ? dataFinal.Value.ToString("dd/MM/yyyy") : "...")}")
                                    .FontSize(8);
                            }

                            if (!string.IsNullOrWhiteSpace(tipo))
                            {
                                column.Item()
                                    .Text(
                                        $"Tipo: {(tipo == "Saida" ? "Saída" : tipo)}")
                                    .FontSize(8);
                            }
                        });

                    // =================================
                    // TABELA
                    // =================================

                    page.Content()
                        .PaddingVertical(15)
                        .Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(65);
                                columns.RelativeColumn(1.7f);
                                columns.RelativeColumn(1.2f);
                                columns.RelativeColumn(1.4f);
                                columns.ConstantColumn(45);
                                columns.RelativeColumn(1f);
                                columns.ConstantColumn(50);
                                columns.ConstantColumn(55);
                                columns.RelativeColumn(1f);
                                columns.RelativeColumn(1.8f);
                            });

                            table.Header(header =>
                            {
                                header.Cell()
                                    .Background(Colors.Grey.Lighten2)
                                    .Padding(4)
                                    .Text("Data")
                                    .Bold();

                                header.Cell()
                                    .Background(Colors.Grey.Lighten2)
                                    .Padding(4)
                                    .Text("Produto")
                                    .Bold();

                                header.Cell()
                                    .Background(Colors.Grey.Lighten2)
                                    .Padding(4)
                                    .Text("Marca")
                                    .Bold();

                                header.Cell()
                                    .Background(Colors.Grey.Lighten2)
                                    .Padding(4)
                                    .Text("Fornecedor")
                                    .Bold();

                                header.Cell()
                                    .Background(Colors.Grey.Lighten2)
                                    .Padding(4)
                                    .Text("Tam.")
                                    .Bold();

                                header.Cell()
                                    .Background(Colors.Grey.Lighten2)
                                    .Padding(4)
                                    .Text("Cor")
                                    .Bold();

                                header.Cell()
                                    .Background(Colors.Grey.Lighten2)
                                    .Padding(4)
                                    .Text("Tipo")
                                    .Bold();

                                header.Cell()
                                    .Background(Colors.Grey.Lighten2)
                                    .Padding(4)
                                    .Text("Qtd.")
                                    .Bold();

                                header.Cell()
                                    .Background(Colors.Grey.Lighten2)
                                    .Padding(4)
                                    .Text("Preço")
                                    .Bold();

                                header.Cell()
                                    .Background(Colors.Grey.Lighten2)
                                    .Padding(4)
                                    .Text("Observação")
                                    .Bold();
                            });

                            foreach (var item in movimentacoes)
                            {
                                var tipoMovimentacao =
                                    item.Tipo == TipoMovimentacao.Entrada
                                        ? "Entrada"
                                        : "Saída";

                                table.Cell()
                                    .Padding(4)
                                    .Text(
                                        item.Data.ToString(
                                            "dd/MM/yyyy HH:mm"));

                                table.Cell()
                                    .Padding(4)
                                    .Text(
                                        item.Produto?.Nome ??
                                        "Produto não encontrado");

                                table.Cell()
                                    .Padding(4)
                                    .Text(
                                        item.Marca?.Nome ??
                                        "Marca não encontrada");

                                table.Cell()
                                    .Padding(4)
                                    .Text(
                                        item.Fornecedor?.Nome ??
                                        "Não informado");

                                table.Cell()
                                    .Padding(4)
                                    .Text(
                                        item.Tamanho ?? "-");

                                table.Cell()
                                    .Padding(4)
                                    .Text(
                                        item.Cor ?? "-");

                                table.Cell()
                                    .Padding(4)
                                    .Text(tipoMovimentacao);

                                table.Cell()
                                    .Padding(4)
                                    .AlignCenter()
                                    .Text(
                                        item.Quantidade.ToString());

                                table.Cell()
                                    .Padding(4)
                                    .Text(
                                        item.PrecoCompraUnitario
                                            .ToString("C"));

                                table.Cell()
                                    .Padding(4)
                                    .Text(
                                        string.IsNullOrWhiteSpace(
                                            item.Observacao)
                                            ? "-"
                                            : item.Observacao);
                            }
                        });

                    // =================================
                    // RODAPÉ
                    // =================================

                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span(
                                "LojaWeb - Relatório de Movimentações | Página ");

                            text.CurrentPageNumber();

                            text.Span(" de ");

                            text.TotalPages();
                        });
                });
            });

            var pdf = document.GeneratePdf();

            return File(
                pdf,
                "application/pdf",
                $"Relatorio_Movimentacoes_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
        }

        // =========================================
        // EXPORTAR RELATÓRIO DE COMPRAS PARA EXCEL
        // =========================================

        [HttpGet]
        public async Task<IActionResult> ExportarComprasExcel(
            DateTime? dataInicial,
            DateTime? dataFinal,
            int? fornecedorId,
            int? produtoId,
            int? marcaId)
        {
            var query = _context.MovimentacoesEstoque
                .Include(m => m.Fornecedor)
                .Include(m => m.Produto)
                .Include(m => m.Marca)
                .Where(m => m.Tipo == TipoMovimentacao.Entrada)
                .AsQueryable();


            // DATA INICIAL

            if (dataInicial.HasValue)
            {
                query = query.Where(m =>
                    m.Data >= dataInicial.Value);
            }


            // DATA FINAL

            if (dataFinal.HasValue)
            {
                var dataFim = dataFinal.Value.Date.AddDays(1);

                query = query.Where(m =>
                    m.Data < dataFim);
            }


            // FORNECEDOR

            if (fornecedorId.HasValue)
            {
                query = query.Where(m =>
                    m.FornecedorId == fornecedorId.Value);
            }


            // PRODUTO

            if (produtoId.HasValue)
            {
                query = query.Where(m =>
                    m.ProdutoId == produtoId.Value);
            }


            // MARCA

            if (marcaId.HasValue)
            {
                query = query.Where(m =>
                    m.MarcaId == marcaId.Value);
            }


            var compras = await query
                .OrderByDescending(m => m.Data)
                .ToListAsync();


            // =========================================
            // CRIAR EXCEL
            // =========================================

            using var workbook = new XLWorkbook();

            var worksheet =
                workbook.Worksheets.Add("Compras");


            // TÍTULO

            worksheet.Cell(1, 1)
                .Value = "RELATÓRIO DE COMPRAS";

            worksheet.Range(1, 1, 1, 10)
                .Merge();

            worksheet.Cell(1, 1)
                .Style.Font.Bold = true;

            worksheet.Cell(1, 1)
                .Style.Font.FontSize = 16;


            // DATA DE GERAÇÃO

            worksheet.Cell(2, 1)
                .Value =
                $"Gerado em: {DateTime.Now:dd/MM/yyyy HH:mm}";

            worksheet.Range(2, 1, 2, 10)
                .Merge();


            // =========================================
            // FILTROS
            // =========================================

            int linhaFiltro = 3;

            if (dataInicial.HasValue)
            {
                worksheet.Cell(linhaFiltro, 1)
                    .Value =
                    $"Data inicial: {dataInicial.Value:dd/MM/yyyy}";

                linhaFiltro++;
            }

            if (dataFinal.HasValue)
            {
                worksheet.Cell(linhaFiltro, 1)
                    .Value =
                    $"Data final: {dataFinal.Value:dd/MM/yyyy}";

                linhaFiltro++;
            }

            if (fornecedorId.HasValue)
            {
                worksheet.Cell(linhaFiltro, 1)
                    .Value =
                    $"Fornecedor ID: {fornecedorId.Value}";

                linhaFiltro++;
            }

            if (produtoId.HasValue)
            {
                worksheet.Cell(linhaFiltro, 1)
                    .Value =
                    $"Produto ID: {produtoId.Value}";

                linhaFiltro++;
            }

            if (marcaId.HasValue)
            {
                worksheet.Cell(linhaFiltro, 1)
                    .Value =
                    $"Marca ID: {marcaId.Value}";

                linhaFiltro++;
            }


            // =========================================
            // CABEÇALHO
            // =========================================

            int linhaCabecalho = linhaFiltro + 1;

            worksheet.Cell(linhaCabecalho, 1)
                .Value = "Data";

            worksheet.Cell(linhaCabecalho, 2)
                .Value = "Fornecedor";

            worksheet.Cell(linhaCabecalho, 3)
                .Value = "Produto";

            worksheet.Cell(linhaCabecalho, 4)
                .Value = "Marca";

            worksheet.Cell(linhaCabecalho, 5)
                .Value = "Tamanho";

            worksheet.Cell(linhaCabecalho, 6)
                .Value = "Cor";

            worksheet.Cell(linhaCabecalho, 7)
                .Value = "Quantidade";

            worksheet.Cell(linhaCabecalho, 8)
                .Value = "Preço compra";

            worksheet.Cell(linhaCabecalho, 9)
                .Value = "Total";

            worksheet.Cell(linhaCabecalho, 10)
                .Value = "Observação";


            var cabecalho = worksheet.Range(
                linhaCabecalho,
                1,
                linhaCabecalho,
                10);

            cabecalho.Style.Font.Bold = true;

            cabecalho.Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;


            // =========================================
            // DADOS
            // =========================================

            int linha = linhaCabecalho + 1;

            foreach (var item in compras)
            {
                decimal totalItem =
                    item.Quantidade *
                    item.PrecoCompraUnitario;


                worksheet.Cell(linha, 1)
                    .Value = item.Data;

                worksheet.Cell(linha, 2)
                    .Value =
                    item.Fornecedor?.Nome ??
                    "Não informado";

                worksheet.Cell(linha, 3)
                    .Value =
                    item.Produto?.Nome ??
                    "Produto não encontrado";

                worksheet.Cell(linha, 4)
                    .Value =
                    item.Marca?.Nome ??
                    "Marca não encontrada";

                worksheet.Cell(linha, 5)
                    .Value = item.Tamanho;

                worksheet.Cell(linha, 6)
                    .Value = item.Cor;

                worksheet.Cell(linha, 7)
                    .Value = item.Quantidade;

                worksheet.Cell(linha, 8)
                    .Value = item.PrecoCompraUnitario;

                worksheet.Cell(linha, 9)
                    .Value = totalItem;

                worksheet.Cell(linha, 10)
                    .Value =
                    string.IsNullOrWhiteSpace(item.Observacao)
                        ? "-"
                        : item.Observacao;

                linha++;
            }


            // =========================================
            // FORMATAÇÃO
            // =========================================

            worksheet.Column(1)
                .Style.DateFormat.Format =
                "dd/MM/yyyy HH:mm";

            worksheet.Column(8)
                .Style.NumberFormat.Format =
                "R$ #,##0.00";

            worksheet.Column(9)
                .Style.NumberFormat.Format =
                "R$ #,##0.00";


            // =========================================
            // AJUSTAR COLUNAS
            // =========================================

            worksheet.Columns()
                .AdjustToContents();


            // =========================================
            // CONGELAR CABEÇALHO
            // =========================================

            worksheet.SheetView
                .FreezeRows(linhaCabecalho);


            // =========================================
            // GERAR ARQUIVO
            // =========================================

            using var stream = new MemoryStream();

            workbook.SaveAs(stream);

            var arquivo = stream.ToArray();


            return File(
                arquivo,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Relatorio_Compras_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
        }

        // =========================================
        // EXPORTAR RELATÓRIO DE COMPRAS PARA PDF
        // =========================================

        [HttpGet]
        public async Task<IActionResult> ExportarComprasPdf(
            DateTime? dataInicial,
            DateTime? dataFinal,
            int? fornecedorId,
            int? produtoId,
            int? marcaId)
        {
            var query = _context.MovimentacoesEstoque
                .Include(m => m.Fornecedor)
                .Include(m => m.Produto)
                .Include(m => m.Marca)
                .Where(m => m.Tipo == TipoMovimentacao.Entrada)
                .AsQueryable();


            // DATA INICIAL

            if (dataInicial.HasValue)
            {
                query = query.Where(m =>
                    m.Data >= dataInicial.Value);
            }


            // DATA FINAL

            if (dataFinal.HasValue)
            {
                var dataFim = dataFinal.Value.Date.AddDays(1);

                query = query.Where(m =>
                    m.Data < dataFim);
            }


            // FORNECEDOR

            if (fornecedorId.HasValue)
            {
                query = query.Where(m =>
                    m.FornecedorId == fornecedorId.Value);
            }


            // PRODUTO

            if (produtoId.HasValue)
            {
                query = query.Where(m =>
                    m.ProdutoId == produtoId.Value);
            }


            // MARCA

            if (marcaId.HasValue)
            {
                query = query.Where(m =>
                    m.MarcaId == marcaId.Value);
            }


            var compras = await query
                .OrderByDescending(m => m.Data)
                .ToListAsync();


            // =========================================
            // CRIAR PDF
            // =========================================

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());

                    page.Margin(25);

                    page.DefaultTextStyle(
                        x => x.FontSize(7));


                    // =================================
                    // CABEÇALHO
                    // =================================

                    page.Header()
                        .Column(column =>
                        {
                            column.Item()
                                .Text("LojaWeb")
                                .Bold()
                                .FontSize(22);

                            column.Item()
                                .Text("Relatório de Compras")
                                .Bold()
                                .FontSize(16);

                            column.Item()
                                .Text(
                                    $"Gerado em: {DateTime.Now:dd/MM/yyyy HH:mm}")
                                .FontSize(9);


                            if (dataInicial.HasValue ||
                                dataFinal.HasValue)
                            {
                                column.Item()
                                    .Text(
                                        $"Período: " +
                                        $"{(dataInicial.HasValue ? dataInicial.Value.ToString("dd/MM/yyyy") : "...")} " +
                                        $"até " +
                                        $"{(dataFinal.HasValue ? dataFinal.Value.ToString("dd/MM/yyyy") : "...")}")
                                    .FontSize(8);
                            }


                            if (fornecedorId.HasValue)
                            {
                                column.Item()
                                    .Text(
                                        $"Fornecedor ID: {fornecedorId.Value}")
                                    .FontSize(8);
                            }


                            if (produtoId.HasValue)
                            {
                                column.Item()
                                    .Text(
                                        $"Produto ID: {produtoId.Value}")
                                    .FontSize(8);
                            }


                            if (marcaId.HasValue)
                            {
                                column.Item()
                                    .Text(
                                        $"Marca ID: {marcaId.Value}")
                                    .FontSize(8);
                            }
                        });


                    // =================================
                    // TABELA
                    // =================================

                    page.Content()
                        .PaddingVertical(15)
                        .Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(65);
                                columns.RelativeColumn(1.5f);
                                columns.RelativeColumn(1.7f);
                                columns.RelativeColumn(1.2f);
                                columns.ConstantColumn(45);
                                columns.RelativeColumn(1f);
                                columns.ConstantColumn(55);
                                columns.RelativeColumn(1f);
                                columns.RelativeColumn(1f);
                                columns.RelativeColumn(1.8f);
                            });


                            // CABEÇALHO

                            table.Header(header =>
                            {
                                header.Cell()
                                    .Background(Colors.Grey.Lighten2)
                                    .Padding(4)
                                    .Text("Data")
                                    .Bold();

                                header.Cell()
                                    .Background(Colors.Grey.Lighten2)
                                    .Padding(4)
                                    .Text("Fornecedor")
                                    .Bold();

                                header.Cell()
                                    .Background(Colors.Grey.Lighten2)
                                    .Padding(4)
                                    .Text("Produto")
                                    .Bold();

                                header.Cell()
                                    .Background(Colors.Grey.Lighten2)
                                    .Padding(4)
                                    .Text("Marca")
                                    .Bold();

                                header.Cell()
                                    .Background(Colors.Grey.Lighten2)
                                    .Padding(4)
                                    .Text("Tam.")
                                    .Bold();

                                header.Cell()
                                    .Background(Colors.Grey.Lighten2)
                                    .Padding(4)
                                    .Text("Cor")
                                    .Bold();

                                header.Cell()
                                    .Background(Colors.Grey.Lighten2)
                                    .Padding(4)
                                    .Text("Qtd.")
                                    .Bold();

                                header.Cell()
                                    .Background(Colors.Grey.Lighten2)
                                    .Padding(4)
                                    .Text("Preço")
                                    .Bold();

                                header.Cell()
                                    .Background(Colors.Grey.Lighten2)
                                    .Padding(4)
                                    .Text("Total")
                                    .Bold();

                                header.Cell()
                                    .Background(Colors.Grey.Lighten2)
                                    .Padding(4)
                                    .Text("Observação")
                                    .Bold();
                            });


                            // DADOS

                            foreach (var item in compras)
                            {
                                decimal totalItem =
                                    item.Quantidade *
                                    item.PrecoCompraUnitario;


                                table.Cell()
                                    .Padding(4)
                                    .Text(
                                        item.Data.ToString(
                                            "dd/MM/yyyy HH:mm"));


                                table.Cell()
                                    .Padding(4)
                                    .Text(
                                        item.Fornecedor?.Nome ??
                                        "Não informado");


                                table.Cell()
                                    .Padding(4)
                                    .Text(
                                        item.Produto?.Nome ??
                                        "Produto não encontrado");


                                table.Cell()
                                    .Padding(4)
                                    .Text(
                                        item.Marca?.Nome ??
                                        "Marca não encontrada");


                                table.Cell()
                                    .Padding(4)
                                    .Text(
                                        item.Tamanho ?? "-");


                                table.Cell()
                                    .Padding(4)
                                    .Text(
                                        item.Cor ?? "-");


                                table.Cell()
                                    .Padding(4)
                                    .AlignCenter()
                                    .Text(
                                        item.Quantidade.ToString());


                                table.Cell()
                                    .Padding(4)
                                    .Text(
                                        item.PrecoCompraUnitario
                                            .ToString("C"));


                                table.Cell()
                                    .Padding(4)
                                    .Text(
                                        totalItem.ToString("C"));


                                table.Cell()
                                    .Padding(4)
                                    .Text(
                                        string.IsNullOrWhiteSpace(
                                            item.Observacao)
                                            ? "-"
                                            : item.Observacao);
                            }
                        });


                    // =================================
                    // RODAPÉ
                    // =================================

                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span(
                                "LojaWeb - Relatório de Compras | Página ");

                            text.CurrentPageNumber();

                            text.Span(" de ");

                            text.TotalPages();
                        });
                });
            });


            var pdf = document.GeneratePdf();


            return File(
                pdf,
                "application/pdf",
                $"Relatorio_Compras_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
        }

        // =========================================
        // EXPORTAR RELATÓRIO FINANCEIRO PARA EXCEL
        // =========================================

        [HttpGet]
        public async Task<IActionResult> ExportarFinanceiroExcel(
           DateTime? dataInicio,
            DateTime? dataFim,
            TipoLancamentoFinanceiro? tipo,
            StatusLancamentoFinanceiro? status,
            string? formaPagamento)
        {
            // =========================================
            // PERÍODO
            // =========================================

            var inicio = dataInicio ??
                new DateTime(
                    DateTime.Today.Year,
                    DateTime.Today.Month,
                    1);

            var fim = dataFim ??
                inicio.AddMonths(1).AddDays(-1);

            var query =
    _context
        .LancamentosFinanceiros
        .Where(l =>
            l.Data >= inicio &&
            l.Data < fim.Date.AddDays(1));


            // =========================================
            // FILTRO POR TIPO
            // =========================================

            if (tipo.HasValue)
            {
                query = query.Where(l =>
                    l.Tipo == tipo.Value);
            }


            // =========================================
            // FILTRO POR STATUS
            // =========================================

            if (status.HasValue)
            {
                query = query.Where(l =>
                    l.Status == status.Value);
            }


            // =========================================
            // FILTRO POR FORMA DE PAGAMENTO
            // =========================================

            if (!string.IsNullOrWhiteSpace(formaPagamento))
            {
                query = query.Where(l =>
                    l.FormaPagamento == formaPagamento);
            }


            var lancamentos =
                await query
                    .OrderByDescending(l => l.Data)
                    .ToListAsync();

            // =========================================
            // TOTAIS
            // =========================================

            var totalReceitas =
                lancamentos
                    .Where(l =>
                        l.Tipo ==
                            TipoLancamentoFinanceiro.Receita &&
                        l.Status ==
                            StatusLancamentoFinanceiro.Pago)
                    .Sum(l => l.Valor);


            var totalDespesas =
                lancamentos
                    .Where(l =>
                        l.Tipo ==
                            TipoLancamentoFinanceiro.Despesa &&
                        l.Status ==
                            StatusLancamentoFinanceiro.Pago)
                    .Sum(l => l.Valor);


            var saldo =
                totalReceitas - totalDespesas;


            // =========================================
            // CRIAR EXCEL
            // =========================================

            using var workbook = new XLWorkbook();

            var worksheet =
                workbook.Worksheets.Add("Financeiro");


            // =========================================
            // TÍTULO
            // =========================================

            worksheet.Cell(1, 1)
                .Value = "RELATÓRIO FINANCEIRO";

            worksheet.Range(1, 1, 1, 7)
                .Merge();

            worksheet.Cell(1, 1)
                .Style.Font.Bold = true;

            worksheet.Cell(1, 1)
                .Style.Font.FontSize = 16;


            // =========================================
            // DATA DE GERAÇÃO
            // =========================================

            worksheet.Cell(2, 1)
                .Value =
                $"Gerado em: {DateTime.Now:dd/MM/yyyy HH:mm}";

            worksheet.Range(2, 1, 2, 7)
                .Merge();


            // =========================================
            // PERÍODO
            // =========================================

            worksheet.Cell(3, 1)
                .Value =
                $"Período: {inicio:dd/MM/yyyy} até {fim:dd/MM/yyyy}";

            worksheet.Range(3, 1, 3, 7)
                .Merge();


            // =========================================
            // RESUMO FINANCEIRO
            // =========================================

            worksheet.Cell(5, 1)
                .Value = "RESUMO FINANCEIRO";

            worksheet.Cell(6, 1)
                .Value = "Total de Receitas";

            worksheet.Cell(6, 2)
                .Value = totalReceitas;

            worksheet.Cell(7, 1)
                .Value = "Total de Despesas";

            worksheet.Cell(7, 2)
                .Value = totalDespesas;

            worksheet.Cell(8, 1)
                .Value = "Saldo";

            worksheet.Cell(8, 2)
                .Value = saldo;


            worksheet.Cell(5, 1)
                .Style.Font.Bold = true;

            worksheet.Range(6, 1, 8, 1)
                .Style.Font.Bold = true;


            // =========================================
            // CABEÇALHO DOS LANÇAMENTOS
            // =========================================

            worksheet.Cell(10, 1)
                .Value = "Data";

            worksheet.Cell(10, 2)
                .Value = "Descrição";

            worksheet.Cell(10, 3)
                .Value = "Categoria";

            worksheet.Cell(10, 4)
                .Value = "Forma de pagamento";

            worksheet.Cell(10, 5)
                .Value = "Tipo";

            worksheet.Cell(10, 6)
                .Value = "Status";

            worksheet.Cell(10, 7)
                .Value = "Valor";


            var cabecalho =
                worksheet.Range(10, 1, 10, 7);

            cabecalho.Style.Font.Bold = true;

            cabecalho.Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;


            // =========================================
            // DADOS
            // =========================================

            int linha = 11;

            foreach (var item in lancamentos)
            {
                worksheet.Cell(linha, 1)
                    .Value = item.Data;

                worksheet.Cell(linha, 2)
                    .Value = item.Descricao;

                worksheet.Cell(linha, 3)
                    .Value = item.Categoria;

                worksheet.Cell(linha, 4)
                    .Value = item.FormaPagamento ?? "-";

                worksheet.Cell(linha, 5)
                    .Value =
                    item.Tipo ==
                        TipoLancamentoFinanceiro.Receita
                        ? "Receita"
                        : "Despesa";

                worksheet.Cell(linha, 6)
                    .Value =
                    item.Status ==
                        StatusLancamentoFinanceiro.Pago
                        ? "Pago"
                        : item.Status ==
                            StatusLancamentoFinanceiro.Pendente
                            ? "Pendente"
                            : "Cancelado";

                worksheet.Cell(linha, 7)
                    .Value = item.Valor;

                linha++;
            }


            // =========================================
            // FORMATAÇÃO
            // =========================================

            worksheet.Column(1)
                .Style.DateFormat.Format =
                "dd/MM/yyyy HH:mm";

            worksheet.Column(7)
                .Style.NumberFormat.Format =
                "R$ #,##0.00";

            worksheet.Cell(6, 2)
                .Style.NumberFormat.Format =
                "R$ #,##0.00";

            worksheet.Cell(7, 2)
                .Style.NumberFormat.Format =
                "R$ #,##0.00";

            worksheet.Cell(8, 2)
                .Style.NumberFormat.Format =
                "R$ #,##0.00";


            // =========================================
            // AJUSTAR COLUNAS
            // =========================================

            worksheet.Columns()
                .AdjustToContents();


            // =========================================
            // CONGELAR CABEÇALHO
            // =========================================

            worksheet.SheetView
                .FreezeRows(10);


            // =========================================
            // GERAR ARQUIVO
            // =========================================

            using var stream = new MemoryStream();

            workbook.SaveAs(stream);

            var arquivo = stream.ToArray();


            return File(
                arquivo,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Relatorio_Financeiro_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
        }

        // =========================================
        // EXPORTAR RELATÓRIO FINANCEIRO PARA PDF
        // =========================================

        [HttpGet]
        public async Task<IActionResult> ExportarFinanceiroPdf(
              DateTime? dataInicio,
                DateTime? dataFim,
                TipoLancamentoFinanceiro? tipo,
                StatusLancamentoFinanceiro? status,
                string? formaPagamento)
        {
            // =========================================
            // PERÍODO
            // =========================================

            var inicio = dataInicio ??
                new DateTime(
                    DateTime.Today.Year,
                    DateTime.Today.Month,
                    1);

            var fim = dataFim ??
                inicio.AddMonths(1).AddDays(-1);

            var query =
                _context
                    .LancamentosFinanceiros
                    .Where(l =>
                        l.Data >= inicio &&
                        l.Data < fim.Date.AddDays(1));


            // =========================================
            // FILTRO POR TIPO
            // =========================================

            if (tipo.HasValue)
            {
                query = query.Where(l =>
                    l.Tipo == tipo.Value);
            }


            // =========================================
            // FILTRO POR STATUS
            // =========================================

            if (status.HasValue)
            {
                query = query.Where(l =>
                    l.Status == status.Value);
            }


            // =========================================
            // FILTRO POR FORMA DE PAGAMENTO
            // =========================================

            if (!string.IsNullOrWhiteSpace(formaPagamento))
            {
                query = query.Where(l =>
                    l.FormaPagamento == formaPagamento);
            }


            var lancamentos =
                await query
                    .OrderByDescending(l => l.Data)
                    .ToListAsync();


            // =========================================
            // TOTAIS
            // =========================================

            var totalReceitas =
                lancamentos
                    .Where(l =>
                        l.Tipo ==
                            TipoLancamentoFinanceiro.Receita &&
                        l.Status ==
                            StatusLancamentoFinanceiro.Pago)
                    .Sum(l => l.Valor);


            var totalDespesas =
                lancamentos
                    .Where(l =>
                        l.Tipo ==
                            TipoLancamentoFinanceiro.Despesa &&
                        l.Status ==
                            StatusLancamentoFinanceiro.Pago)
                    .Sum(l => l.Valor);


            var saldo =
                totalReceitas - totalDespesas;


            // =========================================
            // CRIAR PDF
            // =========================================

            var documento =
                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(30);

                        page.DefaultTextStyle(
                            x => x.FontSize(9));


                        // =================================
                        // CABEÇALHO
                        // =================================

                        page.Header()
                            .Column(column =>
                            {
                                column.Item()
                                    .AlignCenter()
                                    .Text("LOJAWEB")
                                    .Bold()
                                    .FontSize(20);

                                column.Item()
                                    .AlignCenter()
                                    .Text("RELATÓRIO FINANCEIRO")
                                    .Bold()
                                    .FontSize(15);

                                column.Item()
                                    .AlignCenter()
                                    .Text(
                                        $"Período: {inicio:dd/MM/yyyy} até {fim:dd/MM/yyyy}");

                                column.Item()
                                    .AlignCenter()
                                    .Text(
                                        $"Gerado em: {DateTime.Now:dd/MM/yyyy HH:mm}");

                                column.Item()
                                    .PaddingTop(10)
                                    .LineHorizontal(1);
                            });


                        // =================================
                        // CONTEÚDO
                        // =================================

                        page.Content()
                            .PaddingTop(15)
                            .Column(column =>
                            {
                                // =============================
                                // RESUMO
                                // =============================

                                column.Item()
                                    .Text("RESUMO FINANCEIRO")
                                    .Bold()
                                    .FontSize(12);

                                column.Item()
                                    .PaddingTop(8)
                                    .Table(table =>
                                    {
                                        table.ColumnsDefinition(columns =>
                                        {
                                            columns.RelativeColumn();
                                            columns.RelativeColumn();
                                            columns.RelativeColumn();
                                        });


                                        table.Header(header =>
                                        {
                                            header.Cell()
                                                .Element(CellHeader)
                                                .Text("Receitas");

                                            header.Cell()
                                                .Element(CellHeader)
                                                .Text("Despesas");

                                            header.Cell()
                                                .Element(CellHeader)
                                                .Text("Saldo");
                                        });


                                        table.Cell()
                                            .Element(CellBody)
                                            .Text(
                                                totalReceitas.ToString("C"));

                                        table.Cell()
                                            .Element(CellBody)
                                            .Text(
                                                totalDespesas.ToString("C"));

                                        table.Cell()
                                            .Element(CellBody)
                                            .Text(
                                                saldo.ToString("C"));
                                    });


                                // =============================
                                // ESPAÇAMENTO
                                // =============================

                                column.Item()
                                    .PaddingTop(20);


                                // =============================
                                // LANÇAMENTOS
                                // =============================

                                column.Item()
                                    .Text("LANÇAMENTOS")
                                    .Bold()
                                    .FontSize(12);


                                column.Item()
                                    .PaddingTop(8)
                                    .Table(table =>
                                    {
                                        table.ColumnsDefinition(columns =>
                                        {
                                            columns.ConstantColumn(60);   // Data
                                            columns.RelativeColumn(2);    // Descrição
                                            columns.RelativeColumn(1.2f); // Categoria
                                            columns.ConstantColumn(60);   // Tipo
                                            columns.ConstantColumn(80);   // Pagamento
                                            columns.ConstantColumn(65);   // Status
                                            columns.ConstantColumn(65);   // Valor
                                        });
                                        table.Header(header =>
                                        {
                                            header.Cell()
                                                .Element(CellHeader)
                                                .Text("Data");

                                            header.Cell()
                                                .Element(CellHeader)
                                                .Text("Descrição");

                                            header.Cell()
                                                .Element(CellHeader)
                                                .Text("Categoria");

                                            header.Cell()
                                                .Element(CellHeader)
                                                .Text("Tipo");

                                            header.Cell()
                                                .Element(CellHeader)
                                                .Text("Pagamento");

                                            header.Cell()
                                                .Element(CellHeader)
                                                .Text("Status");

                                            header.Cell()
                                                .Element(CellHeader)
                                                .Text("Valor");
                                        });


                                        foreach (var item in lancamentos)
                                        {
                                            table.Cell()
                                                .Element(CellBody)
                                                .Text(
                                                    item.Data.ToString(
                                                        "dd/MM/yyyy"));

                                            table.Cell()
                                                .Element(CellBody)
                                                .Text(
                                                    item.Descricao);

                                            table.Cell()
                                                .Element(CellBody)
                                                .Text(
                                                    item.Categoria);

                                            table.Cell()
                                                .Element(CellBody)
                                                .Text(
                                                    item.Tipo ==
                                                        TipoLancamentoFinanceiro.Receita
                                                        ? "Receita"
                                                        : "Despesa");

                                            table.Cell()
                                                .Element(CellBody)
                                                .Text(
                                                    item.FormaPagamento ?? "-");

                                            table.Cell()
                                                .Element(CellBody)
                                                .Text(
                                                    item.Status ==
                                                        StatusLancamentoFinanceiro.Pago
                                                        ? "Pago"
                                                        : item.Status ==
                                                            StatusLancamentoFinanceiro.Pendente
                                                            ? "Pendente"
                                                            : "Cancelado");

                                            table.Cell()
                                                .Element(CellBody)
                                                .AlignRight()
                                                .Text(
                                                    item.Valor.ToString("C"));
                                        }
                                    });
                            });


                        // =================================
                        // RODAPÉ
                        // =================================

                        page.Footer()
                            .AlignCenter()
                            .Text(text =>
                            {
                                text.Span("LojaWeb - Relatório Financeiro | ");

                                text.CurrentPageNumber();

                                text.Span(" / ");

                                text.TotalPages();
                            });
                    });
                });


            // =========================================
            // GERAR ARQUIVO
            // =========================================

            var pdfBytes =
                documento.GeneratePdf();


            return File(
                pdfBytes,
                "application/pdf",
                $"Relatorio_Financeiro_{DateTime.Now:yyyyMMdd_HHmm}.pdf");


            // =========================================
            // ESTILOS DAS CÉLULAS
            // =========================================

            static IContainer CellHeader(
                IContainer container)
            {
                return container
                    .Background("#E9ECEF")
                    .Border(1)
                    .BorderColor("#CED4DA")
                    .Padding(5)
                    .AlignMiddle();
            }


            static IContainer CellBody(
                IContainer container)
            {
                return container
                    .BorderBottom(1)
                    .BorderColor("#DEE2E6")
                    .Padding(5)
                    .AlignMiddle();
            }
        }
    }
}