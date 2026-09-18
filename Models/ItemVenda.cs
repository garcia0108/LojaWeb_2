using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LojaWeb_2.Models
{
    public class ItemVenda
    {
        public int Id { get; set; }


        // =========================================
        // VENDA
        // =========================================

        [Required]
        public int VendaId { get; set; }

        public Venda? Venda { get; set; }


        // =========================================
        // PRODUTO
        // =========================================

        [Required]
        [Display(Name = "Produto")]
        public int ProdutoId { get; set; }

        public Produto? Produto { get; set; }


        // =========================================
        // COMBO
        // =========================================

        // Será preenchido somente quando
        // o produto fizer parte de um Combo.

        public int? ComboId { get; set; }

        public Combo? Combo { get; set; }


        // =========================================
        // MARCA
        // =========================================

        [Required]
        public int MarcaId { get; set; }

        public Marca? Marca { get; set; }


        // =========================================
        // VARIAÇÃO
        // =========================================

        [Required]
        [StringLength(10)]
        public string Tamanho { get; set; } = string.Empty;


        [Required]
        [StringLength(50)]
        public string Cor { get; set; } = string.Empty;


        // =========================================
        // QUANTIDADE
        // =========================================

        [Required]
        [Range(1, int.MaxValue,
            ErrorMessage = "A quantidade deve ser maior que zero.")]
        public int Quantidade { get; set; }


        // =========================================
        // VALORES
        // =========================================

        [Required]
        [Range(0.01, double.MaxValue,
            ErrorMessage = "O preço deve ser maior que zero.")]
        [Display(Name = "Preço unitário")]
        public decimal PrecoUnitario { get; set; }


        [Column(TypeName = "decimal(18,2)")]
        public decimal CustoUnitario { get; set; }


        [Display(Name = "Subtotal")]
        public decimal Subtotal { get; set; }
    }
}