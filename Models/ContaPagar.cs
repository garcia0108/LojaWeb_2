using System.ComponentModel.DataAnnotations;

namespace LojaWeb_2.Models
{
    public class ContaPagar
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Descrição")]
        [StringLength(200)]
        public string Descricao { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Valor")]
        public decimal Valor { get; set; }

        [Required]
        [Display(Name = "Vencimento")]
        public DateTime DataVencimento { get; set; } = DateTime.Today;

        [Display(Name = "Data de pagamento")]
        public DateTime? DataPagamento { get; set; }

        [Display(Name = "Forma de pagamento")]
        [StringLength(50)]
        public string? FormaPagamento { get; set; }

        [Display(Name = "Status")]
        public StatusContaPagar Status { get; set; }
            = StatusContaPagar.Pendente;

        // Fornecedor relacionado
        public int? FornecedorId { get; set; }

        public Fornecedor? Fornecedor { get; set; }

        [Required]
        [Display(Name = "Categoria")]
        [StringLength(100)]
        public string Categoria { get; set; } = string.Empty;

        [Display(Name = "Observação")]
        [StringLength(500)]
        public string? Observacao { get; set; }
    }


    public enum StatusContaPagar
    {
        Pendente = 1,
        Pago = 2,
        Cancelado = 3
    }
}