using System.ComponentModel.DataAnnotations;

namespace LojaWeb_2.Models
{
    public class UsuarioAlterarSenhaViewModel
    {

        public string Id { get; set; } = string.Empty;    

        public string UserName { get; set; } = string.Empty;
        
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe a nova senha.")]
        [StringLength(100, MinimumLength = 6,
            ErrorMessage = "A senha deve ter no mínimo 6 caracteres.")]
        [DataType(DataType.Password)]
        public string NovaSenha { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirme a nova senha.")]
        [DataType(DataType.Password)]
        [Compare("NovaSenha", ErrorMessage = "As senhas não coincidem.")]
        public string ConfirmarSenha { get; set; } = string.Empty;  
    }
}
