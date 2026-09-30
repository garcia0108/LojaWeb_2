using LojaWeb_2.Models;
using Microsoft.EntityFrameworkCore;

namespace LojaWeb_2.Data
{
    public static class DbInitializer
    {
        public static async Task InicializarAsync(ApplicationDbContext context)
        {
            await context.Database.MigrateAsync();

            if (!await context.PerfisImportacao.AnyAsync())
            {
                context.PerfisImportacao.AddRange(
                    new PerfilImportacao
                    {
                        Nome = "LojaWeb Padrão",
                        TamanhosNaHorizontal = false,
                        UsaLetrasAF = false
                    },
                    new PerfilImportacao
                    {
                        Nome = "Mirante",
                        TamanhosNaHorizontal = true,
                        UsaLetrasAF = true
                    },
                    new PerfilImportacao
                    {
                        Nome = "Hering",
                        TamanhosNaHorizontal = false,
                        UsaLetrasAF = false,
                        CampoTorax = "Peito"
                    }
                );

                await context.SaveChangesAsync();
            }
        }
    }
}