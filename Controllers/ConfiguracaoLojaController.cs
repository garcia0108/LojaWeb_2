using LojaWeb_2.Data;
using LojaWeb_2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LojaWeb_2.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class ConfiguracaoLojaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ConfiguracaoLojaController(
            ApplicationDbContext context)
        {
            _context = context;
        }


        // =========================================
        // GET: CONFIGURAÇÕES
        // =========================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var configuracao =
                await _context.ConfiguracoesLoja
                    .FirstOrDefaultAsync();

            // Se ainda não existir configuração,
            // cria uma configuração inicial.

            if (configuracao == null)
            {
                configuracao = new ConfiguracaoLoja
                {
                    NomeLoja = "LojaWeb"
                };

                _context.ConfiguracoesLoja.Add(configuracao);

                await _context.SaveChangesAsync();
            }

            return View(configuracao);
        }


        // =========================================
        // POST: SALVAR
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(
       ConfiguracaoLoja model,
       IFormFile? logoArquivo)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var configuracao =
                await _context.ConfiguracoesLoja
                    .FirstOrDefaultAsync();

            if (configuracao == null)
            {
                configuracao = new ConfiguracaoLoja
                {
                    NomeLoja = model.NomeLoja,
                    Logo = model.Logo,
                    CnpjCpf = model.CnpjCpf,
                    Email = model.Email,
                    Telefone = model.Telefone,
                    Cep = model.Cep,
                    Endereco = model.Endereco,
                    Numero = model.Numero,
                    Bairro = model.Bairro,
                    Cidade = model.Cidade,
                    Estado = model.Estado,
                    Instagram = model.Instagram,
                    Facebook = model.Facebook,
                    WhatsApp = model.WhatsApp,
                    TextoRodape = model.TextoRodape
                };

                _context.ConfiguracoesLoja.Add(configuracao);
            }
            else
            {
                configuracao.NomeLoja = model.NomeLoja;
                configuracao.CnpjCpf = model.CnpjCpf;
                configuracao.Email = model.Email;
                configuracao.Telefone = model.Telefone;
                configuracao.Cep = model.Cep;
                configuracao.Endereco = model.Endereco;
                configuracao.Numero = model.Numero;
                configuracao.Bairro = model.Bairro;
                configuracao.Cidade = model.Cidade;
                configuracao.Estado = model.Estado;
                configuracao.Instagram = model.Instagram;
                configuracao.Facebook = model.Facebook;
                configuracao.WhatsApp = model.WhatsApp;
                configuracao.TextoRodape = model.TextoRodape;
            }


            // =========================================
            // UPLOAD DA LOGO
            // =========================================

            if (logoArquivo != null &&
                logoArquivo.Length > 0)
            {
                var extensoesPermitidas = new[]
                {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        };

                var extensao =
                    Path.GetExtension(
                        logoArquivo.FileName)
                        .ToLowerInvariant();

                if (!extensoesPermitidas.Contains(extensao))
                {
                    ModelState.AddModelError(
                        "Logo",
                        "Formato de imagem não permitido.");

                    return View(model);
                }


                // =====================================
                // PASTA DA LOGO
                // =====================================

                var pastaUploads = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "uploads",
                    "logo");

                Directory.CreateDirectory(
                    pastaUploads);


                // =====================================
                // EXCLUIR LOGO ANTIGA
                // =====================================

                if (!string.IsNullOrWhiteSpace(
                        configuracao.Logo))
                {
                    var logoAntiga =
                        Path.Combine(
                            Directory.GetCurrentDirectory(),
                            "wwwroot",
                            configuracao.Logo
                                .TrimStart('/')
                                .Replace(
                                    '/',
                                    Path.DirectorySeparatorChar));

                    if (System.IO.File.Exists(
                            logoAntiga))
                    {
                        System.IO.File.Delete(
                            logoAntiga);
                    }
                }


                // =====================================
                // NOVO NOME
                // =====================================

                var nomeArquivo =
                    $"logo-loja{extensao}";

                var caminhoFisico =
                    Path.Combine(
                        pastaUploads,
                        nomeArquivo);


                // =====================================
                // SALVAR ARQUIVO
                // =====================================

                using (var stream =
                    new FileStream(
                        caminhoFisico,
                        FileMode.Create))
                {
                    await logoArquivo.CopyToAsync(stream);
                }


                // =====================================
                // SALVAR CAMINHO NO BANCO
                // =====================================

                configuracao.Logo =
                    $"/uploads/logo/{nomeArquivo}";
            }


            await _context.SaveChangesAsync();


            TempData["Sucesso"] =
                "Configurações da loja atualizadas com sucesso!";


            return RedirectToAction(
                nameof(Index));
        }
    }
}