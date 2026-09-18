using System.ComponentModel.DataAnnotations;

namespace LojaWeb_2.Models
{
    public class LancamentoFinanceiro
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Data")]
        public DateTime Data { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Descrição")]
        [StringLength(200)]
        public string Descricao { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Tipo")]
        public TipoLancamentoFinanceiro Tipo { get; set; }

        [Required]
        [Display(Name = "Categoria")]
        [StringLength(100)]
        public string Categoria { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Valor")]
        public decimal Valor { get; set; }

        [Display(Name = "Forma de pagamento")]
        [StringLength(50)]
        public string? FormaPagamento { get; set; }

        [Display(Name = "Observação")]
        [StringLength(500)]
        public string? Observacao { get; set; }

        [Display(Name = "Data de pagamento")]
        public DateTime? DataPagamento { get; set; }

        [Display(Name = "Status")]
        public StatusLancamentoFinanceiro Status { get; set; }
            = StatusLancamentoFinanceiro.Pendente;
    }


    public enum TipoLancamentoFinanceiro
    {
        Receita = 1,
        Despesa = 2
    }


    public enum StatusLancamentoFinanceiro
    {
        Pendente = 1,
        Pago = 2,
        Cancelado = 3
    }
}