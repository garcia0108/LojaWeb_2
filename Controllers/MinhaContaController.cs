using LojaWeb_2.Data;
using LojaWeb_2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace LojaWeb_2.Controllers
{
    [Authorize(Roles = "Cliente")]
    public class MinhaContaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public MinhaContaController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        // =========================================
        // MINHA CONTA
        // =========================================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }


            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c =>
                    c.Email == usuario.Email);


            if (cliente == null)
            {
                TempData["Erro"] =
                    "Cadastro de cliente não encontrado.";

                return RedirectToAction(
                    "Index",
                    "Home");
            }


            return View(cliente);
        }

        // =========================================
        // SALVAR DADOS
        // =========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(Cliente model)
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }


            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c =>
                    c.Email == usuario.Email);


            if (cliente == null)
            {
                TempData["Erro"] =
                    "Cadastro de cliente não encontrado.";

                return RedirectToAction(
                    "Index",
                    "Home");
            }


            // =====================================
            // VALIDAR DADOS
            // =====================================

            if (!ModelState.IsValid)
            {
                return View(cliente);
            }


            // =====================================
            // ATUALIZAR CLIENTE
            // =====================================

            cliente.Nome = model.Nome;
            cliente.CpfCnpj = model.CpfCnpj;
            cliente.Telefone = model.Telefone;

            cliente.Endereco = model.Endereco;
            cliente.Numero = model.Numero;
            cliente.Bairro = model.Bairro;
            cliente.Cidade = model.Cidade;
            cliente.Estado = model.Estado;
            cliente.Cep = model.Cep;


            await _context.SaveChangesAsync();


            TempData["Sucesso"] =
                "Seus dados foram atualizados com sucesso!";


            return RedirectToAction(
                nameof(Index));
        }

        // =========================================
        // MEUS PEDIDOS
        // =========================================
        [HttpGet]
        public async Task<IActionResult> Pedidos()
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c =>
                    c.Email == usuario.Email);

            if (cliente == null)
            {
                TempData["Erro"] =
                    "Cadastro de cliente não encontrado.";

                return RedirectToAction(nameof(Index));
            }

            var pedidos = await _context.Vendas
                .Where(v => v.ClienteId == cliente.Id)
                .OrderByDescending(v => v.DataVenda)
                .ToListAsync();

            return View(pedidos);
        }

        // =========================================
        // DETALHES DO PEDIDO
        // =========================================
        [HttpGet]
        public async Task<IActionResult> Pedido(int id)
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c =>
                    c.Email == usuario.Email);

            if (cliente == null)
            {
                TempData["Erro"] =
                    "Cadastro de cliente não encontrado.";

                return RedirectToAction(nameof(Index));
            }

            var pedido = await _context.Vendas
                .Include(v => v.Itens)
                    .ThenInclude(i => i.Produto)
                .FirstOrDefaultAsync(v =>
                    v.Id == id &&
                    v.ClienteId == cliente.Id);

            if (pedido == null)
            {
                TempData["Erro"] =
                    "Pedido não encontrado.";

                return RedirectToAction(nameof(Pedidos));
            }

            return View(pedido);
        }

        // =========================================
        // COMPROVANTE DO PEDIDO - PDF
        // =========================================
        [HttpGet]
        public async Task<IActionResult> ComprovantePdf(int id)
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            // =========================================
            // CLIENTE LOGADO
            // =========================================

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c =>
                    c.Email == usuario.Email);

            if (cliente == null)
            {
                TempData["Erro"] =
                    "Cadastro de cliente não encontrado.";

                return RedirectToAction(nameof(Index));
            }

            // =========================================
            // PEDIDO
            // =========================================

            var pedido = await _context.Vendas
                .Include(v => v.Itens)
                    .ThenInclude(i => i.Produto)
                .FirstOrDefaultAsync(v =>
                    v.Id == id &&
                    v.ClienteId == cliente.Id);

            if (pedido == null)
            {
                TempData["Erro"] =
                    "Pedido não encontrado.";

                return RedirectToAction(nameof(Pedidos));
            }

            // =========================================
            // GERAÇÃO DO PDF
            // =========================================

            var documento = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);

                    page.Margin(40);

                    page.DefaultTextStyle(x =>
                        x.FontSize(10));

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
                                .FontSize(22);

                            column.Item()
                                .AlignCenter()
                                .Text("COMPROVANTE DE PEDIDO")
                                .Bold()
                                .FontSize(14);

                            column.Item()
                                .PaddingTop(5)
                                .LineHorizontal(1);
                        });


                    // =================================
                    // CONTEÚDO
                    // =================================

                    page.Content()
                        .Column(column =>
                        {
                            // ---------------------------------
                            // INFORMAÇÕES DO PEDIDO
                            // ---------------------------------

                            column.Item()
                                .PaddingTop(20)
                                .Text($"Pedido #{pedido.Id}")
                                .Bold()
                                .FontSize(16);

                            column.Item()
                                .PaddingTop(8)
                                .Text(
                                    $"Data do pedido: {pedido.DataVenda:dd/MM/yyyy HH:mm}");

                            column.Item()
                                .Text(
                                    $"Forma de pagamento: {pedido.FormaPagamento}");

                            column.Item()
                                .Text(
                                    $"Status: {pedido.Status}");


                            // ---------------------------------
                            // CLIENTE
                            // ---------------------------------

                            column.Item()
                                .PaddingTop(20)
                                .Text("DADOS DO CLIENTE")
                                .Bold()
                                .FontSize(13);

                            column.Item()
                                .PaddingTop(5)
                                .Text(
                                    $"Nome: {cliente.Nome}");

                            column.Item()
                                .Text(
                                    $"CPF/CNPJ: {cliente.CpfCnpj}");

                            column.Item()
                                .Text(
                                    $"E-mail: {cliente.Email}");

                            column.Item()
                                .Text(
                                    $"Telefone: {cliente.Telefone}");

                            column.Item()
                                .Text(
                                    $"Endereço: {cliente.Endereco}, {cliente.Numero}");

                            column.Item()
                                .Text(
                                    $"Bairro: {cliente.Bairro}");

                            column.Item()
                                .Text(
                                    $"{cliente.Cidade} - {cliente.Estado}");

                            column.Item()
                                .Text(
                                    $"CEP: {cliente.Cep}");


                            // ---------------------------------
                            // PRODUTOS
                            // ---------------------------------

                            column.Item()
                                .PaddingTop(25)
                                .Text("PRODUTOS DO PEDIDO")
                                .Bold()
                                .FontSize(13);


                            column.Item()
                                .PaddingTop(8)
                                .Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(3);
                                        columns.RelativeColumn(1);
                                        columns.RelativeColumn(1);
                                        columns.RelativeColumn(1.2f);
                                        columns.RelativeColumn(1.3f);
                                    });


                                    // CABEÇALHO

                                    table.Header(header =>
                                    {
                                        header.Cell()
                                            .Element(CellHeader)
                                            .Text("Produto");

                                        header.Cell()
                                            .Element(CellHeader)
                                            .Text("Qtd.");

                                        header.Cell()
                                            .Element(CellHeader)
                                            .Text("Tamanho");

                                        header.Cell()
                                            .Element(CellHeader)
                                            .Text("Valor unit.");

                                        header.Cell()
                                            .Element(CellHeader)
                                            .Text("Subtotal");
                                    });


                                    // PRODUTOS

                                    foreach (var item in pedido.Itens)
                                    {
                                        table.Cell()
                                            .Element(CellBody)
                                            .Text(
                                                item.Produto?.Nome ?? "-");

                                        table.Cell()
                                            .Element(CellBody)
                                            .Text(
                                                item.Quantidade.ToString());

                                        table.Cell()
                                            .Element(CellBody)
                                            .Text(
                                                item.Tamanho ?? "-");

                                        table.Cell()
                                            .Element(CellBody)
                                            .Text(
                                                item.PrecoUnitario
                                                    .ToString("C"));

                                        table.Cell()
                                            .Element(CellBody)
                                            .Text(
                                                (item.Quantidade *
                                                 item.PrecoUnitario)
                                                .ToString("C"));
                                    }


                                    static IContainer CellHeader(
                                        IContainer container)
                                    {
                                        return container
                                            .Background(Colors.Grey.Lighten2)
                                            .BorderBottom(1)
                                            .BorderColor(Colors.Grey.Medium)
                                            .Padding(5)
                                            .DefaultTextStyle(
                                                x => x.Bold());
                                    }


                                    static IContainer CellBody(
                                        IContainer container)
                                    {
                                        return container
                                            .BorderBottom(1)
                                            .BorderColor(Colors.Grey.Lighten3)
                                            .Padding(5);
                                    }
                                });


                            // ---------------------------------
                            // TOTAL
                            // ---------------------------------

                            column.Item()
                                .PaddingTop(20)
                                .AlignRight()
                                .Text(
                                    $"TOTAL DO PEDIDO: {pedido.Total:C}")
                                .Bold()
                                .FontSize(15);
                        });


                    // =================================
                    // RODAPÉ
                    // =================================

                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span("LojaWeb - Comprovante de Pedido | ");

                            text.Span(
                                DateTime.Now
                                    .ToString("dd/MM/yyyy HH:mm"));
                        });
                });
            });


            // =========================================
            // RETORNAR PDF
            // =========================================

            var pdf = documento.GeneratePdf();

            return File(
                pdf,
                "application/pdf",
                $"Pedido_{pedido.Id}.pdf");
        }

    }
}