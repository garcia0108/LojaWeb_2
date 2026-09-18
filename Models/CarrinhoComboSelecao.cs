using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LojaWeb_2.Models
{
    public class CarrinhoComboSelecao
    {
        public int Id { get; set; }


        // =========================================
        // CARRINHO
        // =========================================

        [Required]
        public int CarrinhoComboItemId { get; set; }

        [ForeignKey("CarrinhoComboItemId")]
        public CarrinhoComboItem CarrinhoComboItem { get; set; }
            = null!;


        // =========================================
        // ITEM DO COMBO
        // =========================================

        [Required]
        public int ComboItemId { get; set; }

        [ForeignKey("ComboItemId")]
        public ComboItem ComboItem { get; set; }
            = null!;


        // =========================================
        // PRODUTO
        // =========================================

        [Required]
        public int ProdutoId { get; set; }

        [ForeignKey("ProdutoId")]
        public Produto Produto { get; set; }
            = null!;


        // =========================================
        // COR
        // =========================================

        [Required]
        [StringLength(50)]
        public string Cor { get; set; }
            = string.Empty;


        // =========================================
        // TAMANHO
        // =========================================

        [Required]
        [StringLength(10)]
        public string Tamanho { get; set; }
            = string.Empty;
    }
}