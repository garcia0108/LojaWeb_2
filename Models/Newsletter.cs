using System.ComponentModel.DataAnnotations;

namespace LojaWeb_2.Models
{
    public class Newsletter
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Informe seu e-mail.")]
        [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        public DateTime DataCadastro { get; set; } = DateTime.Now;

        public bool Ativo { get; set; } = true;
    }
}