using Microsoft.AspNetCore.Identity;

namespace LojaWeb_2.Data
{
    public static class IdentitySeed
    {
        public static async Task SeedAsync(
            IServiceProvider serviceProvider)
        {
            var roleManager =
                serviceProvider.GetRequiredService<
                    RoleManager<IdentityRole>>();

            var userManager =
                serviceProvider.GetRequiredService<
                    UserManager<IdentityUser>>();


            // =========================================
            // CRIAR ROLE ADMINISTRADOR
            // =========================================

            if (!await roleManager.RoleExistsAsync(
                "Administrador"))
            {
                await roleManager.CreateAsync(
                    new IdentityRole("Administrador"));
            }


            // =========================================
            // CRIAR ROLE FUNCIONÁRIO
            // =========================================

            if (!await roleManager.RoleExistsAsync(
                "Funcionario"))
            {
                await roleManager.CreateAsync(
                    new IdentityRole("Funcionario"));
            }


            // =========================================
            // CRIAR ROLE CLIENTE
            // =========================================

            if (!await roleManager.RoleExistsAsync(
                "Cliente"))
            {
                await roleManager.CreateAsync(
                    new IdentityRole("Cliente"));
            }


            // =========================================
            // PRIMEIRO USUÁRIO = ADMINISTRADOR
            // =========================================

            var primeiroUsuario =
                userManager.Users
                    .OrderBy(u => u.Id)
                    .FirstOrDefault();


            if (primeiroUsuario != null)
            {
                if (!await userManager.IsInRoleAsync(
                    primeiroUsuario,
                    "Administrador"))
                {
                    await userManager.AddToRoleAsync(
                        primeiroUsuario,
                        "Administrador");
                }
            }
        }
    }
}