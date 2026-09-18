using System.ComponentModel.DataAnnotations;

namespace LojaWeb_2.Models
{
    public class CheckoutViewModel
    {
        // ================================
        // CLIENTE
        // ================================

        public int ClienteId { get; set; }

        [Required(ErrorMessage = "Informe o nome.")]
        [StringLength(150)]
        [Display(Name = "Nome")]
        public string Nome { get; set; } = string.Empty;


        [Required(ErrorMessage = "Informe o e-mail.")]
        [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
        [StringLength(150)]
        [Display(Name = "E-mail")]
        public string Email { get; set; } = string.Empty;


        [Phone(ErrorMessage = "Informe um telefone válido.")]
        [StringLength(20)]
        [Display(Name = "Telefone")]
        public string? Telefone { get; set; }


        // ================================
        // ENDEREÇO
        // ================================

        [Required(ErrorMessage = "Informe o endereço.")]
        [StringLength(100)]
        [Display(Name = "Endereço")]
        public string Endereco { get; set; } = string.Empty;


        [Required(ErrorMessage = "Informe o número.")]
        [StringLength(10)]
        [Display(Name = "Número")]
        public string Numero { get; set; } = string.Empty;


        [Required(ErrorMessage = "Informe o bairro.")]
        [StringLength(100)]
        [Display(Name = "Bairro")]
        public string Bairro { get; set; } = string.Empty;


        [Required(ErrorMessage = "Informe a cidade.")]
        [StringLength(100)]
        [Display(Name = "Cidade")]
        public string Cidade { get; set; } = string.Empty;


        [Required(ErrorMessage = "Informe o estado.")]
        [StringLength(2)]
        [Display(Name = "Estado")]
        public string Estado { get; set; } = string.Empty;


        [Required(ErrorMessage = "Informe o CEP.")]
        [StringLength(9)]
        [Display(Name = "CEP")]
        public string Cep { get; set; } = string.Empty;


        // ================================
        // PAGAMENTO
        // ================================

        [Required(ErrorMessage = "Selecione uma forma de pagamento.")]
        [Display(Name = "Forma de pagamento")]
        public string FormaPagamento { get; set; } = string.Empty;


        // ================================
        // TOTAL
        // ================================

        public decimal Total { get; set; }


        // ================================
        // ITENS
        // ================================

        public List<ItemCarrinho> Itens { get; set; }
            = new List<ItemCarrinho>();
    }
}