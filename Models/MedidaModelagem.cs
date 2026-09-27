using System.ComponentModel.DataAnnotations;

namespace LojaWeb_2.Models
{
    public class MedidaModelagem
    {
        public int Id { get; set; }

        [Required]
        public int ModelagemId { get; set; }
        public Modelagem Modelagem { get; set; } = null!;

        [Required]
        [StringLength(10)]
        public string Tamanho { get; set; } = string.Empty;

        public decimal Torax { get; set; }
        public decimal Comprimento { get; set; }
        public decimal Barra { get; set; }
        public decimal Ombro { get; set; }
        public decimal ComprimentoManga { get; set; }
        public decimal AberturaManga { get; set; }
    }
}