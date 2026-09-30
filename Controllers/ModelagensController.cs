using ClosedXML.Excel;
using DocumentFormat.OpenXml.InkML;
using LojaWeb_2.Data;
using LojaWeb_2.Models;
using LojaWeb_2.Services.Importacao;
using LojaWeb_2.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using System.Globalization;
using QuestPDF.Infrastructure;
using QRCoder;
using System.IO;

namespace LojaWeb_2.Controllers
{
    public class ModelagensController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        private readonly GeradorSvgService _geradorSvg;
        private readonly GeradorSvgService _geradorSvgService;

        private readonly ImportacaoExcelService _importacaoExcelService;

        public ModelagensController(
                ApplicationDbContext context,
                IWebHostEnvironment environment,
                GeradorSvgService geradorSvgService,
                GeradorSvgService geradorSvg,
                ImportacaoExcelService importacaoExcelService)
        {
            _context = context;
            _environment = environment;
            _geradorSvgService = geradorSvgService;
            _geradorSvg = geradorSvg;
            _importacaoExcelService = importacaoExcelService;
        }

        // GET: Modelagens
        public async Task<IActionResult> Index()
        {
            var modelagens = await _context.Modelagens
                .Include(m => m.Marca)
                .Include(m => m.Categoria)
                .Include(m => m.Produtos)
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
        public async Task<IActionResult> Delete(int? id)
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
        public async Task<IActionResult> Medidas(int? id)
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

            return Redirect("/Modelagens");

        }


        // GET: Modelagens/Duplicar/5
        public async Task<IActionResult> Duplicar(int? id)
        {
            if (id == null)
                return NotFound();

            var original = await _context.Modelagens
                .AsNoTracking()
                .Include(m => m.Marca)
                .Include(m => m.Categoria)
                .Include(m => m.Medidas)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (original == null)
                return NotFound();

            var copia = new Modelagem
            {
                Id = original.Id,
                Nome = original.Nome + " (Cópia)",
                MarcaId = original.MarcaId,
                CategoriaId = original.CategoriaId,
                ModoIlustracao = original.ModoIlustracao,
                ImagemReferencia = original.ImagemReferencia,

                Medidas = original.Medidas
                    .OrderBy(x => x.Id)
                    .Select(x => new MedidaModelagem
                    {
                        Tamanho = x.Tamanho,
                        Torax = x.Torax,
                        Comprimento = x.Comprimento,
                        Barra = x.Barra,
                        Ombro = x.Ombro,
                        ComprimentoManga = x.ComprimentoManga,
                        AberturaManga = x.AberturaManga
                    })
                    .ToList()
            };

            ViewData["MarcaId"] = new SelectList(
                _context.Marcas.OrderBy(x => x.Nome),
                "Id",
                "Nome",
                copia.MarcaId);

            ViewData["CategoriaId"] = new SelectList(
                _context.Categorias.OrderBy(x => x.Nome),
                "Id",
                "Nome",
                copia.CategoriaId);

            return View(copia);
        }

        // POST: Modelagens/Duplicar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Duplicar(
                Modelagem modelagem,
                bool copiarSomentePreenchidos = true,
                bool copiarImagem = true,
                bool manterTipoIlustracao = true)
        {
            var erros = ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .Select(x => new
                {
                    Campo = x.Key,
                    Erro = x.Value.Errors.First().ErrorMessage
                })
                .ToList();

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

                var modeloOriginal = await _context.Modelagens
                    .Include(m => m.Marca)
                    .Include(m => m.Categoria)
                    .Include(m => m.Medidas)
                    .FirstOrDefaultAsync(m => m.Id == modelagem.Id);

                return View(modeloOriginal);
            }

            await using var transacao = await _context.Database.BeginTransactionAsync();

            try
            {
                // Busca a modelagem original
                var original = await _context.Modelagens
                     .AsNoTracking()
                     .Include(m => m.Medidas)
                     .FirstOrDefaultAsync(m => m.Id == modelagem.Id);

                if (original == null)
                    return NotFound();

                // Cria a nova modelagem
                var novaModelagem = new Modelagem
                {
                    Nome = modelagem.Nome,
                    MarcaId = modelagem.MarcaId,
                    CategoriaId = modelagem.CategoriaId,
                    Descricao = original.Descricao,
                    ModoIlustracao = manterTipoIlustracao
                    ? original.ModoIlustracao
                    : "Svg",

                    ImagemReferencia = copiarImagem
                    ? original.ImagemReferencia
                    : null
                };

                novaModelagem.Medidas = new List<MedidaModelagem>();

                _context.Modelagens.Add(novaModelagem);

                await _context.SaveChangesAsync();

                // Busca se a nova modelagem já possui medidas
                var medidasNovaModelagem = await _context.MedidasModelagem
                    .Where(m => m.ModelagemId == novaModelagem.Id)
                    .ToListAsync();

                foreach (var medida in original.Medidas)
                {
                    bool possuiMedidas =
                        medida.Torax > 0 ||
                        medida.Comprimento > 0 ||
                        medida.Barra > 0 ||
                        medida.Ombro > 0 ||
                        medida.ComprimentoManga > 0 ||
                        medida.AberturaManga > 0;

                    // Procura o tamanho correspondente
                    var destino = medidasNovaModelagem
                        .FirstOrDefault(x => x.Tamanho == medida.Tamanho);

                    if (destino == null)
                    {
                        // Se não existir, cria o tamanho
                        _context.MedidasModelagem.Add(new MedidaModelagem
                        {
                            ModelagemId = novaModelagem.Id,
                            Tamanho = medida.Tamanho,
                            Torax = (copiarSomentePreenchidos && !possuiMedidas) ? 0 : medida.Torax,
                            Comprimento = (copiarSomentePreenchidos && !possuiMedidas) ? 0 : medida.Comprimento,
                            Barra = (copiarSomentePreenchidos && !possuiMedidas) ? 0 : medida.Barra,
                            Ombro = (copiarSomentePreenchidos && !possuiMedidas) ? 0 : medida.Ombro,
                            ComprimentoManga = (copiarSomentePreenchidos && !possuiMedidas) ? 0 : medida.ComprimentoManga,
                            AberturaManga = (copiarSomentePreenchidos && !possuiMedidas) ? 0 : medida.AberturaManga
                        });
                    }
                    else
                    {
                        // Se já existir, atualiza
                        destino.Torax = (copiarSomentePreenchidos && !possuiMedidas) ? 0 : medida.Torax;
                        destino.Comprimento = (copiarSomentePreenchidos && !possuiMedidas) ? 0 : medida.Comprimento;
                        destino.Barra = (copiarSomentePreenchidos && !possuiMedidas) ? 0 : medida.Barra;
                        destino.Ombro = (copiarSomentePreenchidos && !possuiMedidas) ? 0 : medida.Ombro;
                        destino.ComprimentoManga = (copiarSomentePreenchidos && !possuiMedidas) ? 0 : medida.ComprimentoManga;
                        destino.AberturaManga = (copiarSomentePreenchidos && !possuiMedidas) ? 0 : medida.AberturaManga;
                    }
                }

                await _context.SaveChangesAsync();

                await transacao.CommitAsync();

                TempData["Sucesso"] = "Modelagem duplicada com sucesso.";

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                await transacao.RollbackAsync();

                TempData["Erro"] = "Ocorreu um erro ao duplicar a modelagem.";

                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Modelagens/CopiarMedidas/5
        public async Task<IActionResult> CopiarMedidas(int? id)
        {
            if (id == null)
                return NotFound();

            var destino = await _context.Modelagens
                .Include(m => m.Marca)
                .Include(m => m.Categoria)
                .Include(m => m.Medidas) // <-- importante
                .FirstOrDefaultAsync(m => m.Id == id);

            if (destino == null)
                return NotFound();

            ViewBag.Destino = destino;

            ViewBag.ModelagensOrigem = new SelectList(
                await _context.Modelagens
                    .Include(m => m.Marca)
                    .Include(m => m.Categoria)
                    .Where(m => m.Id != destino.Id)
                    .OrderBy(m => m.Marca.Nome)
                    .ThenBy(m => m.Nome)
                    .Select(m => new
                    {
                        m.Id,
                        Nome = m.Marca.Nome + " • " + m.Categoria.Nome + " • " + m.Nome
                    })
                    .ToListAsync(),
                "Id",
                "Nome");

            return View();
        }

        // POST: Modelagens/CopiarMedidas
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CopiarMedidas(
            int destinoId,
            int origemId,
            bool substituir = true,
            bool apenasPreenchidos = true)
        {
            
            
           var medidasOrigem = await _context.MedidasModelagem
                .Where(x => x.ModelagemId == origemId)
                .ToListAsync();

            var medidasDestino = await _context.MedidasModelagem
                .Where(x => x.ModelagemId == destinoId)
                .ToListAsync();

            if (!medidasOrigem.Any())
            {
                TempData["Erro"] = "A modelagem de origem não possui medidas.";
                return RedirectToAction(nameof(Medidas), new { id = destinoId });
            }

            int atualizados = 0;

            foreach (var origem in medidasOrigem)
            {
                bool possuiMedidas =
                    origem.Torax > 0 ||
                    origem.Comprimento > 0 ||
                    origem.Barra > 0 ||
                    origem.Ombro > 0 ||
                    origem.ComprimentoManga > 0 ||
                    origem.AberturaManga > 0;

                // Só ignora linhas zeradas quando NÃO for substituir.
                if (apenasPreenchidos && !possuiMedidas && !substituir)
                    continue;

                var destino = medidasDestino.FirstOrDefault(x =>
                    string.Equals(
                        x.Tamanho?.Trim(),
                        origem.Tamanho?.Trim(),
                        StringComparison.OrdinalIgnoreCase));

                if (destino == null)
                    continue;

                if (!substituir)
                {
                    bool destinoJaTemMedidas =
                        destino.Torax > 0 ||
                        destino.Comprimento > 0 ||
                        destino.Barra > 0 ||
                        destino.Ombro > 0 ||
                        destino.ComprimentoManga > 0 ||
                        destino.AberturaManga > 0;

                    if (destinoJaTemMedidas)
                        continue;
                }

                destino.Torax = origem.Torax;
                destino.Comprimento = origem.Comprimento;
                destino.Barra = origem.Barra;
                destino.Ombro = origem.Ombro;
                destino.ComprimentoManga = origem.ComprimentoManga;
                destino.AberturaManga = origem.AberturaManga;

                atualizados++;
            }

            _context.MedidasModelagem.UpdateRange(medidasDestino);

            await _context.SaveChangesAsync();

            TempData["Sucesso"] = $"{atualizados} tamanhos copiados com sucesso.";

            return RedirectToAction(nameof(Medidas), new { id = destinoId });
        }

        // GET: Modelagens/ImportarExcel
        public IActionResult ImportarExcel()
        {
            var vm = new ImportacaoModelagemViewModel
            {
                Modelagens = _context.Modelagens
                    .Include(m => m.Marca)
                    .Include(m => m.Categoria)
                    .OrderBy(m => m.Marca.Nome)
                    .ThenBy(m => m.Nome)
                    .Select(m => new SelectListItem
                    {
                        Value = m.Id.ToString(),
                        Text = $"{m.Marca.Nome} • {m.Categoria.Nome} • {m.Nome}"
                    })
                    .ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportarExcel(ImportacaoModelagemViewModel vm)
        {
            vm.Modelagens = _context.Modelagens
                .Include(m => m.Marca)
                .Include(m => m.Categoria)
                .OrderBy(m => m.Marca.Nome)
                .ThenBy(m => m.Nome)
                .Select(m => new SelectListItem
                {
                    Value = m.Id.ToString(),
                    Text = $"{m.Marca.Nome} • {m.Categoria.Nome} • {m.Nome}"
                })
                .ToList();

            if (!ModelState.IsValid)
                return View(vm);

            using var workbook = new XLWorkbook(vm.ArquivoExcel!.OpenReadStream());

            var planilha = workbook.Worksheet(1);

            var perfilDetectado = await DetectarPerfilImportacao(planilha);

            var ultimaLinha = planilha.LastRowUsed()?.RowNumber() ?? 1;

            if (ultimaLinha < 2)
            {
                ModelState.AddModelError("", "A planilha não possui dados para importar.");
                return View(vm);
            }

            vm.Linhas = _importacaoExcelService.LerPlanilha(
                    vm.ArquivoExcel!.OpenReadStream());

            return View("PreviaImportacao", vm);
        }

        private decimal LerDecimal(IXLCell celula)
        {
            if (celula.IsEmpty())
                return 0;

            var texto = celula.GetString().Trim();

            if (string.IsNullOrWhiteSpace(texto))
                return 0;

            // Aceita vírgula ou ponto
            texto = texto.Replace(",", ".");

            if (decimal.TryParse(
                texto,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out decimal valor))
            {
                return valor;
            }

            // Se a célula já for numérica
            try
            {
                return celula.GetValue<decimal>();
            }
            catch
            {
                return 0;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarImportacao(ImportacaoModelagemViewModel vm)
        {
            var medidasDestino = await _context.MedidasModelagem
                .Where(x => x.ModelagemId == vm.ModelagemId)
                .ToListAsync();

            int atualizados = 0;
            int criados = 0;
            int ignorados = 0;

            foreach (var linha in vm.Linhas)
            {
                bool possuiMedidas =
                    linha.Torax > 0 ||
                    linha.Comprimento > 0 ||
                    linha.Barra > 0 ||
                    linha.Ombro > 0 ||
                    linha.ComprimentoManga > 0 ||
                    linha.AberturaManga > 0;

                if (!possuiMedidas)
                {
                    ignorados++;
                    continue;
                }

                var medida = medidasDestino
                    .FirstOrDefault(x => x.Tamanho.Trim().ToUpper() ==
                                         linha.Tamanho.Trim().ToUpper());

                if (medida != null)
                {
                    medida.Torax = linha.Torax;
                    medida.Comprimento = linha.Comprimento;
                    medida.Barra = linha.Barra;
                    medida.Ombro = linha.Ombro;
                    medida.ComprimentoManga = linha.ComprimentoManga;
                    medida.AberturaManga = linha.AberturaManga;

                    atualizados++;
                }
                else
                {
                    _context.MedidasModelagem.Add(new MedidaModelagem
                    {
                        ModelagemId = vm.ModelagemId,
                        Tamanho = linha.Tamanho.Trim().ToUpper(),
                        Torax = linha.Torax,
                        Comprimento = linha.Comprimento,
                        Barra = linha.Barra,
                        Ombro = linha.Ombro,
                        ComprimentoManga = linha.ComprimentoManga,
                        AberturaManga = linha.AberturaManga
                    });

                    criados++;
                }
            }

            await _context.SaveChangesAsync();

            TempData["Sucesso"] =
                $"Importação concluída! Atualizados: {atualizados} | Criados: {criados} | Ignorados: {ignorados}";

            return RedirectToAction(nameof(Medidas), new { id = vm.ModelagemId });
        }

        // GET: Modelagens/ExportarExcel/5
        public async Task<IActionResult> ExportarExcel(int? id)
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

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Modelagem");

            // Informações da modelagem
            ws.Cell("A1").Value = "Marca";
            ws.Cell("B1").Value = modelagem.Marca.Nome;

            ws.Cell("A2").Value = "Categoria";
            ws.Cell("B2").Value = modelagem.Categoria.Nome;

            ws.Cell("A3").Value = "Modelagem";
            ws.Cell("B3").Value = modelagem.Nome;

            // Cabeçalho da tabela
            int linha = 5;

            ws.Cell(linha, 1).Value = "Tamanho";
            ws.Cell(linha, 2).Value = "Tórax";
            ws.Cell(linha, 3).Value = "Comprimento";
            ws.Cell(linha, 4).Value = "Barra";
            ws.Cell(linha, 5).Value = "Ombro";
            ws.Cell(linha, 6).Value = "Manga";
            ws.Cell(linha, 7).Value = "Abertura";

            var cabecalho = ws.Range(linha, 1, linha, 7);
            cabecalho.Style.Font.Bold = true;
            cabecalho.Style.Font.FontColor = XLColor.White;
            cabecalho.Style.Fill.BackgroundColor = XLColor.Black;

            linha++;

            foreach (var medida in modelagem.Medidas.OrderBy(m => m.Id))
            {
                bool possuiMedidas =
                    medida.Torax > 0 ||
                    medida.Comprimento > 0 ||
                    medida.Barra > 0 ||
                    medida.Ombro > 0 ||
                    medida.ComprimentoManga > 0 ||
                    medida.AberturaManga > 0;

                if (!possuiMedidas)
                    continue;

                ws.Cell(linha, 1).Value = medida.Tamanho;
                ws.Cell(linha, 2).Value = medida.Torax;
                ws.Cell(linha, 3).Value = medida.Comprimento;
                ws.Cell(linha, 4).Value = medida.Barra;
                ws.Cell(linha, 5).Value = medida.Ombro;
                ws.Cell(linha, 6).Value = medida.ComprimentoManga;
                ws.Cell(linha, 7).Value = medida.AberturaManga;

                linha++;
            }

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            var nomeArquivo =
                $"Modelagem_{modelagem.Marca.Nome}_{modelagem.Nome}.xlsx";

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                nomeArquivo);
        }

        // GET: Modelagens/BaixarModeloExcel
        public IActionResult BaixarModeloExcel()
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Modelo");

            // Título
            ws.Cell("A1").Value = "MODELO OFICIAL - TABELA DE MEDIDAS";
            ws.Range("A1:G1").Merge();
            ws.Cell("A1").Style.Font.Bold = true;
            ws.Cell("A1").Style.Font.FontSize = 16;
            ws.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Instruções
            ws.Cell("A3").Value = "Preencha apenas os tamanhos utilizados. Linhas sem medidas podem permanecer zeradas.";

            // Cabeçalho
            int linha = 5;

            ws.Cell(linha, 1).Value = "Tamanho";
            ws.Cell(linha, 2).Value = "Tórax";
            ws.Cell(linha, 3).Value = "Comprimento";
            ws.Cell(linha, 4).Value = "Barra";
            ws.Cell(linha, 5).Value = "Ombro";
            ws.Cell(linha, 6).Value = "Manga";
            ws.Cell(linha, 7).Value = "Abertura";

            var cabecalho = ws.Range(linha, 1, linha, 7);
            cabecalho.Style.Font.Bold = true;
            cabecalho.Style.Font.FontColor = XLColor.White;
            cabecalho.Style.Fill.BackgroundColor = XLColor.Black;

            linha++;

            var exemplos = new[]
            {
        new { Tam="PP", Torax=0d, Compr=0d, Barra=0d, Ombro=0d, Manga=0d, Abertura=0d },
        new { Tam="P", Torax=54d, Compr=70d, Barra=55d, Ombro=16d, Manga=19d, Abertura=15d },
        new { Tam="M", Torax=56d, Compr=72d, Barra=57d, Ombro=17d, Manga=20d, Abertura=16d },
        new { Tam="G", Torax=58d, Compr=74d, Barra=59d, Ombro=18d, Manga=21d, Abertura=17d },
        new { Tam="GG", Torax=60d, Compr=76d, Barra=61d, Ombro=19d, Manga=22d, Abertura=18d },
        new { Tam="XGG", Torax=64d, Compr=80d, Barra=65d, Ombro=20d, Manga=23d, Abertura=19d }
    };

            foreach (var e in exemplos)
            {
                ws.Cell(linha, 1).Value = e.Tam;
                ws.Cell(linha, 2).Value = e.Torax;
                ws.Cell(linha, 3).Value = e.Compr;
                ws.Cell(linha, 4).Value = e.Barra;
                ws.Cell(linha, 5).Value = e.Ombro;
                ws.Cell(linha, 6).Value = e.Manga;
                ws.Cell(linha, 7).Value = e.Abertura;
                linha++;
            }

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Modelo_Tabela_Medidas_LojaWeb.xlsx");
        }

        private async Task<PerfilImportacao?> DetectarPerfilImportacao(IXLWorksheet planilha)
        {
            var cabecalhos = Enumerable.Range(
                    1,
                    planilha.LastColumnUsed()?.ColumnNumber() ?? 1)
                .Select(c => planilha.Cell(1, c).GetString().Trim().ToUpper())
                .ToList();

            var perfis = await _context.PerfisImportacao.ToListAsync();

            foreach (var perfil in perfis)
            {
                bool corresponde =
                    cabecalhos.Contains(perfil.CampoTorax.Trim().ToUpper()) &&
                    cabecalhos.Contains(perfil.CampoComprimento.Trim().ToUpper()) &&
                    cabecalhos.Contains(perfil.CampoBarra.Trim().ToUpper());

                if (corresponde)
                    return perfil;
            }

            return null;
        }

        private int EncontrarLinhaCabecalho(IXLWorksheet planilha)
        {
            var ultima = planilha.LastRowUsed()?.RowNumber() ?? 1;

            for (int linha = 1; linha <= ultima; linha++)
            {
                var primeira = planilha.Cell(linha, 1).GetString().Trim().ToUpper();

                if (primeira == "TAMANHO")
                    return linha;
            }

            return 1;
        }

        // GET: Modelagens/ExportarPdf/55
        public async Task<IActionResult> ExportarPdf(int? id)
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

            var configuracao = await _context.ConfiguracoesLoja
                .FirstOrDefaultAsync();

            // Caminho da imagem (quando a modelagem usa imagem)
            string? caminhoImagem = null;

            if (modelagem.ModoIlustracao == "Imagem" &&
                !string.IsNullOrWhiteSpace(modelagem.ImagemReferencia))
            {
                caminhoImagem = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    modelagem.ImagemReferencia
                        .TrimStart('/')
                        .Replace("/", Path.DirectorySeparatorChar.ToString()));

                if (!System.IO.File.Exists(caminhoImagem))
                    caminhoImagem = null;
            }

            byte[]? qrCodeBytes = null;

            try
            {
                var url = Url.Action(
                    "Details",
                    "Modelagens",
                    new { id = modelagem.Id },
                    Request.Scheme);

                if (!string.IsNullOrWhiteSpace(url))
                {
                    using var qrGenerator = new QRCodeGenerator();

                    using var qrData = qrGenerator.CreateQrCode(
                        url,
                        QRCodeGenerator.ECCLevel.Q);

                    var qrCode = new PngByteQRCode(qrData);

                    qrCodeBytes = qrCode.GetGraphic(8);
                }
            }
            catch
            {
                qrCodeBytes = null;
            }

            // SVG gerado pelo serviço (quando a modelagem usa SVG)
            string? svgConteudo = null;

            if (!string.Equals(modelagem.ModoIlustracao, "Imagem",
                StringComparison.OrdinalIgnoreCase))
            {
                svgConteudo = _geradorSvg.GerarSvg(modelagem.Categoria.TipoVisual);
            }

            QuestPDF.Settings.License =
                QuestPDF.Infrastructure.LicenseType.Community;

            var pdf = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(QuestPDF.Helpers.PageSizes.A4);
                    page.Margin(30);

                    page.Content().Column(col =>
                    {
                        // Cabeçalho
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("FICHA TÉCNICA")
                                    .Bold()
                                    .FontSize(22);

                                if (configuracao != null)
                                    c.Item().Text(configuracao.NomeLoja);
                            });

                            if (configuracao != null &&
                                !string.IsNullOrWhiteSpace(configuracao.Logo))
                            {
                                var caminhoLogo = Path.Combine(
                                    Directory.GetCurrentDirectory(),
                                    "wwwroot",
                                    configuracao.Logo
                                        .TrimStart('/')
                                        .Replace("/", Path.DirectorySeparatorChar.ToString()));

                                if (System.IO.File.Exists(caminhoLogo))
                                {
                                    row.ConstantItem(80)
                                        .Height(50)
                                        .Image(caminhoLogo);
                                }
                            }
                        });

                        col.Item().PaddingBottom(10);

                        col.Item().Text($"Marca: {modelagem.Marca.Nome}").SemiBold();
                        col.Item().Text($"Categoria: {modelagem.Categoria.Nome}");
                        col.Item().Text($"Modelagem: {modelagem.Nome}");

                        col.Item().PaddingVertical(12);

                        // Área do desenho técnico
                        col.Item()
                            .Border(1)
                            .Padding(10)
                            .Height(220)
                            .AlignCenter()
                            .AlignMiddle()
                            .Column(area =>
                            {
                                area.Item()
                                    .Text("DESENHO TÉCNICO")
                                    .SemiBold();

                                if (caminhoImagem != null)
                                {
                                    area.Item()
                                        .Height(170)
                                        .Image(caminhoImagem);
                                }
                                else
                                {
                                    area.Item()
                                        .Text("(Desenho não encontrado)")
                                        .FontSize(10)
                                        .FontColor(QuestPDF.Helpers.Colors.Grey.Medium);
                                }
                            });

                        col.Item().PaddingVertical(15);

                        // Tabela de medidas
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(70);

                                for (int i = 0; i < 6; i++)
                                    columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                static void Cabecalho(IContainer container, string texto)
                                {
                                    container
                                        .Background(QuestPDF.Helpers.Colors.Black)
                                        .Border(1)
                                        .BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten1)
                                        .PaddingVertical(6)
                                        .AlignCenter()
                                        .Text(texto)
                                        .FontColor(QuestPDF.Helpers.Colors.White)
                                        .Bold();
                                }

                                Cabecalho(header.Cell(), "Tam");
                                Cabecalho(header.Cell(), "A");
                                Cabecalho(header.Cell(), "B");
                                Cabecalho(header.Cell(), "C");
                                Cabecalho(header.Cell(), "D");
                                Cabecalho(header.Cell(), "E");
                                Cabecalho(header.Cell(), "F");
                            });

                            // 👇 O método fica sozinho aqui
                            static void Celula(IContainer container, string texto)
                            {
                                container
                                    .Border(1)
                                    .BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2)
                                    .PaddingVertical(5)
                                    .AlignCenter()
                                    .Text(texto);
                            }

                            // 👇 E o foreach vem depois
                            foreach (var medida in modelagem.Medidas.OrderBy(m => m.Id))
                            {
                                bool possuiMedidas =
                                    medida.Torax > 0 ||
                                    medida.Comprimento > 0 ||
                                    medida.Barra > 0 ||
                                    medida.Ombro > 0 ||
                                    medida.ComprimentoManga > 0 ||
                                    medida.AberturaManga > 0;

                                if (!possuiMedidas)
                                    continue;

                                Celula(table.Cell(), medida.Tamanho);
                                Celula(table.Cell(), medida.Torax.ToString("0.#"));
                                Celula(table.Cell(), medida.Comprimento.ToString("0.#"));
                                Celula(table.Cell(), medida.Barra.ToString("0.#"));
                                Celula(table.Cell(), medida.Ombro.ToString("0.#"));
                                Celula(table.Cell(), medida.ComprimentoManga.ToString("0.#"));
                                Celula(table.Cell(), medida.AberturaManga.ToString("0.#"));
                            }
                        });

                        col.Item().PaddingTop(20);

                        //Rodapé com QR Code
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Escaneie para abrir esta modelagem no sistema.")
                                    .FontSize(10);

                                c.Item().Text($"ID: {modelagem.Id}")
                                    .FontSize(9);
                            });

                            if (qrCodeBytes != null)
                            {
                                row.ConstantItem(70)
                                    .Height(70)
                                    .Image(qrCodeBytes);
                            }
                        });
                    });
                });
            }).GeneratePdf();

            return File(
                pdf,
                "application/pdf",
                $"FichaTecnica_{modelagem.Marca.Nome}_{modelagem.Nome}.pdf");
        }

        // GET: Modelagens/ProdutosVinculados/5
        public async Task<IActionResult> ProdutosVinculados(int? id)
        {
            if (id == null)
                return NotFound();

            var modelagem = await _context.Modelagens
                .Include(m => m.Marca)
                .Include(m => m.Categoria)
                .Include(m => m.Produtos)
                    .ThenInclude(p => p.Marca)
                .Include(m => m.Produtos)
                    .ThenInclude(p => p.Categoria)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (modelagem == null)
                return NotFound();

            return View(modelagem);
        }
    }

}