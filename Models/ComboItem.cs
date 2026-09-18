using System.ComponentModel.DataAnnotations;

namespace LojaWeb_2.Models
{
    public class ComboItem
    {
        public int Id { get; set; }


        // =========================================
        // COMBO
        // =========================================

        [Required]
        public int ComboId { get; set; }

        public Combo? Combo { get; set; }


        // =========================================
        // PRODUTO
        // =========================================

        [Required]
        public int ProdutoId { get; set; }

        public Produto? Produto { get; set; }


        // =========================================
        // VARIAÇÃO DO PRODUTO
        // =========================================

        [StringLength(50)]
        [Display(Name = "Cor")]
        public string? Cor { get; set; }


        [StringLength(10)]
        [Display(Name = "Tamanho")]
        public string? Tamanho { get; set; }


        // =========================================
        // QUANTIDADE
        // =========================================

        [Range(1, int.MaxValue,
            ErrorMessage = "A quantidade deve ser maior que zero.")]
        [Display(Name = "Quantidade")]
        public int Quantidade { get; set; }
    }
}