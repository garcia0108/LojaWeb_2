using System.ComponentModel.DataAnnotations;

namespace LojaWeb_2.Models
{
    public class UsuarioCriarViewModel
    {
        [Required(ErrorMessage = "Informe o e-mail.")]
        [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe o nome de usuário.")]
        [StringLength(50, MinimumLength = 3,
           ErrorMessage = "O nome de usuário deve ter entre 3 e 50 caracteres.")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe a senha.")]
        [MinLength(6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres.")]
        public string Senha { get; set; } = string.Empty;

        [Required(ErrorMessage = "Selecione um perfil.")]
        public string Perfil { get; set; } = "Funcionário";
    }
}
