using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LojaWeb_2.Models
{
    public class CotacaoFrete
    {
        public int Id { get; set; }

        [Required]
        public int ClienteId { get; set; }

        public Cliente? Cliente { get; set; }

        [Required]
        [StringLength(8)]
        public string CepDestino { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        public string TipoFrete { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Valor { get; set; }

        public int PrazoDias { get; set; }

        public DateTime DataCotacao { get; set; } = DateTime.Now;
    }
}