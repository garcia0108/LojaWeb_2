using LojaWeb_2.Data;
using LojaWeb_2.Models;
using LojaWeb_2.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LojaWeb_2.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class NewslettersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;

        public NewslettersController(
                ApplicationDbContext context,
                EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // =========================================
        // LISTAGEM
        // =========================================
        public async Task<IActionResult> Index(string? pesquisa)
        {
            var query = _context.Newsletters.AsQueryable();

            // PESQUISA
            if (!string.IsNullOrWhiteSpace(pesquisa))
            {
                pesquisa = pesquisa.Trim();

                query = query.Where(n =>
                    n.Email.Contains(pesquisa));
            }

            var newsletters = await query
                .OrderByDescending(n => n.DataCadastro)
                .ToListAsync();

            ViewBag.Pesquisa = pesquisa;

            return View(newsletters);
        }

        // =========================================
        // ATIVAR / DESATIVAR NEWSLETTER
        // =========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlternarStatus(int id)
        {
            var newsletter = await _context.Newsletters.FindAsync(id);

            if (newsletter == null)
            {
                TempData["Erro"] = "Inscrição não encontrada.";
                return RedirectToAction(nameof(Index));
            }

            newsletter.Ativo = !newsletter.Ativo;

            await _context.SaveChangesAsync();

            TempData["Sucesso"] = newsletter.Ativo
                ? "Inscrição ativada com sucesso!"
                : "Inscrição desativada com sucesso!";

            return RedirectToAction(nameof(Index));
        }

        // =========================================
        // EXCLUIR INSCRIÇÃO
        // =========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var newsletter = await _context.Newsletters.FindAsync(id);

            if (newsletter == null)
            {
                TempData["Erro"] = "Inscrição não encontrada.";
                return RedirectToAction(nameof(Index));
            }

            _context.Newsletters.Remove(newsletter);

            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Inscrição excluída com sucesso!";

            return RedirectToAction(nameof(Index));
        }

        // =========================================
        // CAMPANHAS - LISTAGEM
        // =========================================

        public async Task<IActionResult> Campanhas()
        {
            var campanhas = await _context.CampanhasNewsletter
                .OrderByDescending(c => c.DataCriacao)
                .ToListAsync();

            return View(campanhas);
        }


        // =========================================
        // CRIAR CAMPANHA - GET
        // =========================================

        public IActionResult CriarCampanha()
        {
            return View();
        }


        // =========================================
        // CRIAR CAMPANHA - POST
        // =========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CriarCampanha(
            CampanhaNewsletter campanha)
        {
            if (!ModelState.IsValid)
            {
                return View(campanha);
            }

            campanha.DataCriacao = DateTime.Now;
            campanha.Enviada = false;
            campanha.DataEnvio = null;

            _context.CampanhasNewsletter.Add(campanha);

            await _context.SaveChangesAsync();

            TempData["Sucesso"] =
                "Campanha criada com sucesso!";

            return RedirectToAction(nameof(Campanhas));
        }

        // =========================================
        // DETALHES DA CAMPANHA
        // =========================================
        public async Task<IActionResult> DetalhesCampanha(int? id)
        {
            if (id == null)
                return NotFound();

            var campanha = await _context.CampanhasNewsletter
                .FirstOrDefaultAsync(c => c.Id == id);

            if (campanha == null)
                return NotFound();

            return View(campanha);
        }

        // =========================================
        // EDITAR CAMPANHA - GET
        // =========================================

        public async Task<IActionResult> EditarCampanha(int? id)
        {
            if (id == null)
                return NotFound();

            var campanha = await _context.CampanhasNewsletter
                .FindAsync(id);

            if (campanha == null)
                return NotFound();

            // Não permite editar campanha já enviada
            if (campanha.Enviada)
            {
                TempData["Erro"] =
                    "Uma campanha já enviada não pode ser editada.";

                return RedirectToAction(nameof(Campanhas));
            }

            return View(campanha);
        }


        // =========================================
        // EDITAR CAMPANHA - POST
        // =========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarCampanha(
            int id,
            CampanhaNewsletter campanha)
        {
            if (id != campanha.Id)
                return NotFound();

            var campanhaBanco = await _context.CampanhasNewsletter
                .FindAsync(id);

            if (campanhaBanco == null)
                return NotFound();

            // Segurança: não permite editar campanha enviada
            if (campanhaBanco.Enviada)
            {
                TempData["Erro"] =
                    "Uma campanha já enviada não pode ser editada.";

                return RedirectToAction(nameof(Campanhas));
            }

            if (!ModelState.IsValid)
                return View(campanha);

            campanhaBanco.Assunto = campanha.Assunto;
            campanhaBanco.Mensagem = campanha.Mensagem;

            await _context.SaveChangesAsync();

            TempData["Sucesso"] =
                "Campanha atualizada com sucesso!";

            return RedirectToAction(nameof(Campanhas));
        }

        // =========================================
        // EXCLUIR CAMPANHA
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExcluirCampanha(int id)
        {
            var campanha = await _context.CampanhasNewsletter
                .FindAsync(id);

            if (campanha == null)
            {
                TempData["Erro"] = "Campanha não encontrada.";
                return RedirectToAction(nameof(Campanhas));
            }

            // Segurança: não permite excluir campanha enviada
            if (campanha.Enviada)
            {
                TempData["Erro"] =
                    "Uma campanha já enviada não pode ser excluída.";

                return RedirectToAction(nameof(Campanhas));
            }

            _context.CampanhasNewsletter.Remove(campanha);

            await _context.SaveChangesAsync();

            TempData["Sucesso"] =
                "Campanha excluída com sucesso!";

            return RedirectToAction(nameof(Campanhas));
        }

        // =========================================
        // ENVIAR CAMPANHA
        // =========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnviarCampanha(int id)
        {
            var campanha = await _context.CampanhasNewsletter
                .FindAsync(id);

            if (campanha == null)
            {
                TempData["Erro"] = "Campanha não encontrada.";
                return RedirectToAction(nameof(Campanhas));
            }

            // Segurança: evita enviar duas vezes
            if (campanha.Enviada)
            {
                TempData["Erro"] =
                    "Esta campanha já foi enviada.";

                return RedirectToAction(nameof(Campanhas));
            }

            // Busca todos os inscritos ativos
            var inscritos = await _context.Newsletters
                .Where(n => n.Ativo)
                .ToListAsync();

            if (!inscritos.Any())
            {
                TempData["Erro"] =
                    "Não existem inscritos ativos na newsletter.";

                return RedirectToAction(nameof(Campanhas));
            }

            try
            {
                // Envia para cada inscrito
                foreach (var inscrito in inscritos)
                {
                    await _emailService.EnviarEmailAsync(
                        inscrito.Email,
                        campanha.Assunto,
                        campanha.Mensagem
                    );
                }

                // Marca como enviada somente após concluir
                campanha.Enviada = true;
                campanha.DataEnvio = DateTime.Now;

                await _context.SaveChangesAsync();

                TempData["Sucesso"] =
                    $"Campanha enviada com sucesso para {inscritos.Count} inscrito(s)!";
            }
            catch (Exception ex)
            {
                TempData["Erro"] =
                    "Erro ao enviar a campanha: " + ex.Message;
            }

            return RedirectToAction(nameof(Campanhas));
        }
    }
}