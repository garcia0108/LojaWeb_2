using System.ComponentModel.DataAnnotations;

namespace LojaWeb_2.Models
{
    public class ConfiguracaoLoja
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Nome da loja")]
        [StringLength(150)]
        public string NomeLoja { get; set; } = "LojaWeb";

        [Display(Name = "Logo")]
        [StringLength(300)]
        public string? Logo { get; set; }

        [Display(Name = "CNPJ/CPF")]
        [StringLength(30)]
        public string? CnpjCpf { get; set; }

        [Display(Name = "E-mail")]
        [EmailAddress]
        [StringLength(150)]
        public string? Email { get; set; }

        [Display(Name = "Telefone")]
        [StringLength(30)]
        public string? Telefone { get; set; }

        [Display(Name = "CEP")]
        [StringLength(15)]
        public string? Cep { get; set; }

        [Display(Name = "Endereço")]
        [StringLength(200)]
        public string? Endereco { get; set; }

        [Display(Name = "Número")]
        [StringLength(20)]
        public string? Numero { get; set; }

        [Display(Name = "Bairro")]
        [StringLength(100)]
        public string? Bairro { get; set; }

        [Display(Name = "Cidade")]
        [StringLength(100)]
        public string? Cidade { get; set; }

        [Display(Name = "Estado")]
        [StringLength(50)]
        public string? Estado { get; set; }

        [Display(Name = "Instagram")]
        [StringLength(200)]
        public string? Instagram { get; set; }

        [Display(Name = "Facebook")]
        [StringLength(200)]
        public string? Facebook { get; set; }

        [Display(Name = "WhatsApp")]
        [StringLength(30)]
        public string? WhatsApp { get; set; }

        [Display(Name = "Texto do rodapé")]
        [StringLength(500)]
        public string? TextoRodape { get; set; }
    }
}