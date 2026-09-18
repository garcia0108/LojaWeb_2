using System.ComponentModel.DataAnnotations;

namespace LojaWeb_2.Models
{
    public class ContaReceber
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


        [Display(Name = "Data de recebimento")]
        public DateTime? DataRecebimento { get; set; }

        [Required]
        [Display(Name = "Forma de pagamento")]
        [StringLength(50)]
        public string FormaPagamento { get; set; } = string.Empty;

        [Display(Name = "Status")]
        public StatusContaReceber Status { get; set; }
            = StatusContaReceber.Pendente;

        // Venda relacionada
        public int? VendaId { get; set; }

        public Venda? Venda { get; set; }

        [Display(Name = "Observação")]
        [StringLength(500)]
        public string? Observacao { get; set; }
    }


    public enum StatusContaReceber
    {
        Pendente = 1,
        Recebido = 2,
        Cancelado = 3
    }
}