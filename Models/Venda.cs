using System.ComponentModel.DataAnnotations;

namespace LojaWeb_2.Models
{
    public class Venda
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Data da venda")]
        public DateTime DataVenda {  get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Cliente")]
        public int ClienteId {  get; set; }

        public Cliente? Cliente { get; set; }

        [Display(Name = "Total")]
        public decimal Total { get; set; }

        // =========================================
        // FRETE / ENTREGA
        // =========================================

        [Display(Name = "Tipo de frete")]
        public string? TipoFrete { get; set; }

        [Display(Name = "Valor do frete")]
        public decimal ValorFrete { get; set; }

        [Display(Name = "CEP de entrega")]
        public string? CepEntrega { get; set; }

        [Display(Name = "Prazo de entrega")]
        public int? PrazoEntregaDias { get; set; }

        [Required]
        [Display(Name = "Forma de pagamento")]
        public string FormaPagamento { get; set; } = string.Empty;

        public DateTime? DataCancelamento { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; } = "Concluída";


        // =========================================
        // INFORMAÇÕES DE ENVIO
        // =========================================

        [Display(Name = "Transportadora")]
        [StringLength(100)]
        public string? Transportadora { get; set; }


        [Display(Name = "Código de rastreamento")]
        [StringLength(100)]
        public string? CodigoRastreamento { get; set; }


        [Display(Name = "Data de envio")]
        public DateTime? DataEnvio { get; set; }

        public ICollection<ItemVenda> Itens { get; set; } = new List<ItemVenda>();
    }
}
