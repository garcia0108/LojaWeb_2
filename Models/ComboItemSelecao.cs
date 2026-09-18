using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LojaWeb_2.Models
{
    public class ComboItemSelecao
    {
        public int Id { get; set; }

        // =========================================
        // ITEM DO COMBO
        // =========================================

        [Required]
        public int ComboItemId { get; set; }

        [ForeignKey("ComboItemId")]
        public ComboItem ComboItem { get; set; } = null!;


        // =========================================
        // COR ESCOLHIDA
        // =========================================

        [Required]
        [StringLength(50)]
        public string Cor { get; set; } = string.Empty;


        // =========================================
        // TAMANHO ESCOLHIDO
        // =========================================

        [Required]
        [StringLength(10)]
        public string Tamanho { get; set; } = string.Empty;
    }
}