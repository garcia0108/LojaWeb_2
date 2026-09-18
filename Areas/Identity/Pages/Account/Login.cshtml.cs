using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LojaWeb_2.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        public LoginModel(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? ReturnUrl { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Informe o e-mail.")]
            [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Informe a senha.")]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            public bool RememberMe { get; set; }
        }

        public void OnGet(string? returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(
            string? returnUrl = null)
        {
            ReturnUrl = returnUrl;

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Procura o usuário pelo e-mail
            var usuario = await _userManager.FindByEmailAsync(
                Input.Email);

            if (usuario == null)
            {
                ModelState.AddModelError(
                    "",
                    "E-mail ou senha inválidos.");

                return Page();
            }

            // Faz o login usando o usuário encontrado
            var resultado = await _signInManager.PasswordSignInAsync(
                usuario,
                Input.Password,
                Input.RememberMe,
                lockoutOnFailure: false);

            if (resultado.Succeeded)
            {
                if (!string.IsNullOrEmpty(ReturnUrl) &&
                    Url.IsLocalUrl(ReturnUrl)) 
                {
                    return LocalRedirect(ReturnUrl);
                }

                return RedirectToPage("/Index");
            }

            ModelState.AddModelError(
                "",
                "E-mail ou senha inválidos.");

            return Page();
        }
    }
}
