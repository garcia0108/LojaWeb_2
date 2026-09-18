using System.ComponentModel.DataAnnotations;

namespace LojaWeb_2.Models
{
    public class Cliente
    {

        public int Id { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório.")]
        [StringLength(150)]
        [Display(Name = "Nome")]
        public string Nome { get; set; } = string.Empty;

        [StringLength(18)]
        [Display(Name = "CPF/CNPJ")]
        public string? CpfCnpj { get; set; }

        [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
        [StringLength(150)]
        [Display(Name = "E-mail")]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Informe um telefone válido.")]
        [StringLength(20)]
        [Display(Name = "Telefone")]
        public string? Telefone { get; set; }

        [StringLength(100)]
        [Display(Name = "Endereço")]
        public string? Endereco { get; set; }

        [StringLength(10)]
        [Display(Name = "Número")]
        public string? Numero { get; set; }

        [StringLength(100)]
        [Display(Name = "Bairro")]
        public string? Bairro { get; set; }

        [StringLength(100)]
        [Display(Name = "Cidade")]
        public string? Cidade { get; set; }

        [StringLength(2)]
        [Display(Name = "Estado")]
        public string? Estado { get; set; }

        [StringLength(9)]
        [Display(Name = "CEP")]
        public string? Cep { get; set; }

        [Display(Name = "Data de cadastro")]
        public DateTime DataCadastro { get; set; } = DateTime.Now;

        [Display(Name = "Ativo")]
        public bool Ativo { get; set; } = true;
    }
}

