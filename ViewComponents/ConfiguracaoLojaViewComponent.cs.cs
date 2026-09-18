using LojaWeb_2.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LojaWeb_2.ViewComponents
{
    public class ConfiguracaoLojaViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public ConfiguracaoLojaViewComponent(
            ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var configuracao =
                await _context.ConfiguracoesLoja
                    .AsNoTracking()
                    .FirstOrDefaultAsync();

            return View(configuracao);
        }
    }
}