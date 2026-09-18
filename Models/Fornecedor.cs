using System.ComponentModel.DataAnnotations;

namespace LojaWeb_2.Models
{
    public class Fornecedor
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Informe o nome ou razão social.")]
        [StringLength(150, ErrorMessage = "O nome deve ter no máximo 150 caracteres.")]
        [Display(Name = "Nome / Razão Social")]
        public string Nome { get; set; } = string.Empty;

        [StringLength(150, ErrorMessage = "O nome fantasia deve ter no máximo 150 caracteres.")]
        [Display(Name = "Nome Fantasia")]
        public string? NomeFantasia { get; set; }

        [StringLength(18)]
        [Display(Name = "CNPJ")]
        public string? Cnpj { get; set; }

        [StringLength(20)]
        public string? Telefone { get; set; }

        [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
        [StringLength(150)]
        public string? Email { get; set; }

        [StringLength(10)]
        public string? Cep { get; set; }

        [StringLength(150)]
        public string? Endereco { get; set; }

        [StringLength(10)]
        public string? Numero { get; set; }

        [StringLength(100)]
        public string? Bairro { get; set; }

        [StringLength(100)]
        public string? Cidade { get; set; }

        [StringLength(2)]
        public string? Estado { get; set; }

        [StringLength(500)]
        public string? Observacoes { get; set; }

        [Display(Name = "Fornecedor ativo")]
        public bool Ativo { get; set; } = true;
    }
}