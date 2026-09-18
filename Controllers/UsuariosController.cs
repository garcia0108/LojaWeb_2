using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using LojaWeb_2.Models;

namespace LojaWeb_2.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class UsuariosController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public UsuariosController(
             UserManager<IdentityUser> userManager,
             RoleManager<IdentityRole> roleManager,
             SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }

        // GET: Usuarios
        public async Task<IActionResult> Index()
        {
            var usuarios = _userManager.Users
                .OrderBy(u => u.Email)
                .ToList();

            var lista = new List<UsuarioViewModel>();

            foreach (var usuario in usuarios)
            {
                var roles = await _userManager.GetRolesAsync(usuario);

                lista.Add(new UsuarioViewModel
                {
                    Id = usuario.Id,
                    UserName = usuario.UserName ?? "",
                    Email = usuario.Email ?? "",
                    Ativo = !usuario.LockoutEnd.HasValue ||
                            usuario.LockoutEnd <= DateTimeOffset.Now,
                    Perfil = roles.FirstOrDefault() ?? "Sem perfil"
                });
            }

            return View(lista);
        }

        // GET: Usuarios/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Usuarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UsuarioCriarViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.Perfil != "Funcionário" &&
                   model.Perfil != "Administrador")
            {
                ModelState.AddModelError(
                    "Perfil",
                    "O perfil selecionado não é permitido.");

                return View(model);
            }

            var usuarioExistente = await _userManager.FindByEmailAsync(model.Email);

            if (usuarioExistente != null)
            {
                ModelState.AddModelError(
                    "Email",
                    "Já existe um usuário cadastrado com este e-mail.");

                return View(model);
            }

            var usernameExistente = await _userManager.FindByNameAsync(model.UserName);

            if (usernameExistente != null)
            {
                ModelState.AddModelError(
                    "UserName",
                    "Este nome de usuário já está sendo utilizado.");

                return View(model);
            }

            var usuario = new IdentityUser
            {
                UserName = model.UserName,
                Email = model.Email,
                EmailConfirmed = true
            };

            var resultado = await _userManager.CreateAsync(
                usuario,
                model.Senha);

            if (!resultado.Succeeded)
            {
                foreach (var erro in resultado.Errors)
                {
                    ModelState.AddModelError("", erro.Description);
                }

                return View(model);
            }

            // Garantir que o perfil exista
            if (!await _roleManager.RoleExistsAsync(model.Perfil))
            {
                await _roleManager.CreateAsync(
                    new IdentityRole(model.Perfil));
            }

            await _userManager.AddToRoleAsync(
                usuario,
                model.Perfil);

            TempData["Sucesso"] =
                "Usuário criado com sucesso!";

            return RedirectToAction(nameof(Index));
        }

        //GET: Usuário/Edit
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var usuario = await _userManager.FindByIdAsync(id);

            if (usuario == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(usuario);

            var model = new UsuarioEditarViewModel
            {
                Id = id,
                UserName = usuario.UserName ?? "",
                Email = usuario.Email ?? "",
                Perfil = roles.FirstOrDefault() ?? "Funcionário"
            };

            return View(model);
        }

        // POST: Usuário/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            UsuarioEditarViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.Perfil != "Funcionário" &&
                model.Perfil != "Administrador")
            {
                ModelState.AddModelError(
                    "Perfil",
                    "O perfil selecionado não é permitido.");

                return View(model);
            }

            var usuario = await _userManager.FindByIdAsync(model.Id);

            var rolesAtuais =
             await _userManager.GetRolesAsync(usuario);

            if (rolesAtuais.Contains("Cliente"))
            {
                TempData["Erro"] =
                    "Usuários com perfil Cliente não podem ser editados nesta área.";

                return RedirectToAction(nameof(Index));
            }

            if (usuario == null)
            {
                return NotFound();
            }

            // Verificar se outro usuário já utiliza o UserName
            var usuarioComMesmoNome =
                await _userManager.FindByNameAsync(model.UserName);

            if (usuarioComMesmoNome != null &&
                usuarioComMesmoNome.Id != usuario.Id)
            {
                ModelState.AddModelError(
                    "UserName",
                    "Este nome de usuário já está sendo utilizado.");

                return View(model);
            }

            // Verificar se outro usuário já utiliza o E-mail
            var usuarioComMesmoEmail =
                await _userManager.FindByEmailAsync(model.Email);

            if (usuarioComMesmoEmail != null &&
                usuarioComMesmoEmail.Id != usuario.Id)
            {
                ModelState.AddModelError(
                    "Email",
                    "Este e-mail já está sendo utilizado.");

                return View(model);
            }

            // Verificar se o perfil existe
            if (!await _roleManager.RoleExistsAsync(model.Perfil))
            {
                ModelState.AddModelError(
                    "Perfil",
                    "O perfil selecionado não existe.");

                return View(model);
            }

            // Atualizar nome de usuário
            usuario.UserName = model.UserName;

            // Atualizar e-mail
            usuario.Email = model.Email;

            var resultadoAtualizacao =
                await _userManager.UpdateAsync(usuario);

            if (!resultadoAtualizacao.Succeeded)
            {
                foreach (var erro in resultadoAtualizacao.Errors)
                {
                    ModelState.AddModelError(
                        "",
                        erro.Description);
                }

                return View(model);
            }

            // Atualizar perfil
            var perfilAtual =
                await _userManager.GetRolesAsync(usuario);

            if (perfilAtual.Any())
            {
                var resultadoRemover =
                    await _userManager.RemoveFromRolesAsync(
                        usuario,
                        perfilAtual);

                if (!resultadoRemover.Succeeded)
                {
                    foreach (var erro in resultadoRemover.Errors)
                    {
                        ModelState.AddModelError(
                            "",
                            erro.Description);
                    }

                    return View(model);
                }
            }

            var resultadoAdicionar =
                await _userManager.AddToRoleAsync(
                    usuario,
                    model.Perfil);

            if (!resultadoAdicionar.Succeeded)
            {
                foreach (var erro in resultadoAdicionar.Errors)
                {
                    ModelState.AddModelError(
                        "",
                        erro.Description);
                }

                return View(model);
            }

            await _signInManager.RefreshSignInAsync(usuario);

            TempData["Sucesso"] =
                "Usuário atualizado com sucesso!";

            return RedirectToAction(nameof(Index));
        }

        // GET: Usuarios/Delete
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var usuario = await _userManager.FindByIdAsync(id);

            if (usuario == null)
            {
                return NotFound();
            }

            // Não permitir excluir o próprio usuário
            var usuarioLogadoId = _userManager.GetUserId(User);

            if (usuario.Id == usuarioLogadoId)
            {
                TempData["Erro"] =
                    "Você não pode excluir o próprio usuário.";

                return RedirectToAction(nameof(Index));
            }

            var roles = await _userManager.GetRolesAsync(usuario);

            var model = new UsuarioViewModel
            {
                Id = usuario.Id,
                UserName = usuario.UserName ?? "",
                Email = usuario.Email ?? "",
                Perfil = roles.FirstOrDefault() ?? "Sem perfil",
                Ativo = !usuario.LockoutEnd.HasValue ||
                        usuario.LockoutEnd <= DateTimeOffset.Now
            };

            return View(model);
        }

        // POST: Usuarios/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var usuario = await _userManager.FindByIdAsync(id);

            if (usuario == null)
            {
                return NotFound();
            }

            // Não permitir excluir o próprio usuário
            var usuarioLogadoId = _userManager.GetUserId(User);

            if (usuario.Id == usuarioLogadoId)
            {
                TempData["Erro"] =
                    "Você não pode excluir o próprio usuário.";

                return RedirectToAction(nameof(Index));
            }

            var resultado = await _userManager.DeleteAsync(usuario);

            if (!resultado.Succeeded)
            {
                foreach (var erro in resultado.Errors)
                {
                    TempData["Erro"] =
                        erro.Description;
                }

                return RedirectToAction(nameof(Index));
            }

            TempData["Sucesso"] =
                "Usuário excluído com sucesso!";

            return RedirectToAction(nameof(Index));
        }

        // POST: Usuarios/AtivarInativar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AtivarInativar(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var usuario = await _userManager.FindByIdAsync(id);

            if (usuario == null)
            {
                return NotFound();
            }

            // Não permitir inativar o próprio usuário
            var usuarioLogadoId = _userManager.GetUserId(User);

            if (usuario.Id == usuarioLogadoId)
            {
                TempData["Erro"] =
                    "Você não pode inativar o próprio usuário.";

                return RedirectToAction(nameof(Index));
            }

            // Verificar se está ativo
            var estaAtivo =
                !usuario.LockoutEnd.HasValue ||
                usuario.LockoutEnd <= DateTimeOffset.Now;

            if (estaAtivo)
            {
                // INATIVAR
                usuario.LockoutEnd =
                    DateTimeOffset.UtcNow.AddYears(100);

                TempData["Sucesso"] =
                    "Usuário inativado com sucesso!";
            }
            else
            {
                // ATIVAR
                usuario.LockoutEnd = null;

                TempData["Sucesso"] =
                    "Usuário ativado com sucesso!";
            }

            await _userManager.UpdateAsync(usuario);

            return RedirectToAction(nameof(Index));
        }

        // GET: Usuarios/AlterarSenha
        [HttpGet]
        public async Task<IActionResult> AlterarSenha(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var usuario = await _userManager.FindByIdAsync(id);

            if (usuario == null)
            {
                return NotFound();
            }

            var model = new UsuarioAlterarSenhaViewModel
            {
                Id = usuario.Id,
                UserName = usuario.UserName ?? "",
                Email = usuario.Email ?? ""
            };

            return View(model);
        }


        // POST: Usuarios/AlterarSenha
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlterarSenha(
            UsuarioAlterarSenhaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var usuario = await _userManager.FindByIdAsync(model.Id);

            if (usuario == null)
            {
                return NotFound();
            }

            // Remove a senha atual
            var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);

            // Define a nova senha
            var resultado = await _userManager.ResetPasswordAsync(
                usuario,
                token,
                model.NovaSenha);

            if (!resultado.Succeeded)
            {
                foreach (var erro in resultado.Errors)
                {
                    ModelState.AddModelError(
                        "",
                        erro.Description);
                }

                return View(model);
            }

            TempData["Sucesso"] =
                "Senha alterada com sucesso!";

            return RedirectToAction(nameof(Index));
        }
    }
}
