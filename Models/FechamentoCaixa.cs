using System.ComponentModel.DataAnnotations;

namespace LojaWeb_2.Models
{
    public class FechamentoCaixa
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Data de abertura")]
        public DateTime DataAbertura { get; set; } = DateTime.Now;

        [Display(Name = "Data de fechamento")]
        public DateTime? DataFechamento { get; set; }

        [Required]
        [Display(Name = "Saldo inicial")]
        public decimal SaldoInicial { get; set; }

        [Display(Name = "Total de entradas")]
        public decimal TotalEntradas { get; set; }

        [Display(Name = "Total de saídas")]
        public decimal TotalSaidas { get; set; }

        [Display(Name = "Saldo esperado")]
        public decimal SaldoEsperado { get; set; }

        [Display(Name = "Saldo informado")]
        public decimal? SaldoInformado { get; set; }

        [Display(Name = "Diferença")]
        public decimal? Diferenca { get; set; }

        [Display(Name = "Status")]
        public StatusCaixa Status { get; set; }
            = StatusCaixa.Aberto;

        [Display(Name = "Observação")]
        [StringLength(500)]
        public string? Observacao { get; set; }
    }


    public enum StatusCaixa
    {
        Aberto = 1,
        Fechado = 2
    }
}