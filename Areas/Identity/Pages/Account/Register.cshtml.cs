using LojaWeb_2.Data;
using LojaWeb_2.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace LojaWeb_2.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;

        public RegisterModel(UserManager<IdentityUser> userManager, 
            ApplicationDbContext context)
        {

            _userManager = userManager;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required(ErrorMessage = "Informe o nome de usuário.")]
            [Display(Name = "Nome de usuário")]
            public string UserName { get; set; } = string.Empty;


            [Required(ErrorMessage = "Informe o e-mail.")]
            [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
            [Display(Name = "E-mail")]
            public string Email { get; set; } = string.Empty;


            [Required(ErrorMessage = "Informe a senha.")]
            [StringLength(
                100,
                MinimumLength = 6,
                ErrorMessage = "A senha deve ter no mínimo 6 caracteres.")]
            [DataType(DataType.Password)]
            [Display(Name = "Senha")]
            public string Password { get; set; } = string.Empty;


            [Required(ErrorMessage = "Confirme a senha.")]
            [DataType(DataType.Password)]
            [Compare(
                "Password",
                ErrorMessage = "As senhas não coincidem.")]
            [Display(Name = "Confirmar senha")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }


        public void OnGet()
        {
        }


        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }


            var usuarioExistente =
                await _userManager.FindByEmailAsync(Input.Email);

            if (usuarioExistente != null)
            {
                ModelState.AddModelError(
                    "Input.Email",
                    "Já existe uma conta cadastrada com este e-mail.");

                return Page();
            }


            var usernameExistente =
                await _userManager.FindByNameAsync(Input.UserName);

            if (usernameExistente != null)
            {
                ModelState.AddModelError(
                    "Input.UserName",
                    "Este nome de usuário já está sendo utilizado.");

                return Page();
            }


            var usuario = new IdentityUser
            {
                UserName = Input.UserName,
                Email = Input.Email,
                EmailConfirmed = true
            };


            var resultado = await _userManager.CreateAsync(
                usuario,
                Input.Password);


            if (!resultado.Succeeded)
            {
                foreach (var erro in resultado.Errors)
                {
                    ModelState.AddModelError(
                        "",
                        erro.Description);
                }

                return Page();
            }

            // =========================================
            // ATRIBUIR ROLE CLIENTE
            // =========================================

            var resultadoRole = await _userManager.AddToRoleAsync(
                usuario,
                "Cliente");

            if (!resultadoRole.Succeeded)
            {
                await _userManager.DeleteAsync(usuario);

                foreach (var erro in resultadoRole.Errors)
                {
                    ModelState.AddModelError(
                        "",
                        erro.Description);
                }

                return Page();
            }

            // =========================================
            // CRIAR CLIENTE
            // =========================================

            var clienteExistente = await _context.Clientes
                .FirstOrDefaultAsync(c =>
                    c.Email == Input.Email);

            if (clienteExistente == null)
            {
                var cliente = new Cliente
                {
                    Nome = Input.UserName,
                    Email = Input.Email,
                    DataCadastro = DateTime.Now,
                    Ativo = true
                };

                _context.Clientes.Add(cliente);

                await _context.SaveChangesAsync();
            }

            TempData["Sucesso"] =
                "Conta criada com sucesso! Agora você pode entrar.";

            return RedirectToPage("/Account/Login");
        }
    }
}