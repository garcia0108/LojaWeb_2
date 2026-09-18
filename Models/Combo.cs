using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LojaWeb_2.Models
{
    public class Combo
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Informe o nome do combo.")]
        [StringLength(100)]
        [Display(Name = "Nome do Combo")]
        public string Nome { get; set; } = string.Empty;


        [StringLength(500)]
        [Display(Name = "Descrição")]
        public string? Descricao { get; set; }


        [Required(ErrorMessage = "Informe o preço do combo.")]
        [Range(0.01, 999999.99,
            ErrorMessage = "Informe um preço válido.")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Preço do Combo")]
        public decimal Preco { get; set; }


        [Display(Name = "Imagem")]
        public string? Imagem { get; set; }


        [Display(Name = "Ativo")]
        public bool Ativo { get; set; } = true;

        [Display(Name = "Em promoção")]
        public bool EmPromocao { get; set; } = false;

        // =========================================
        // ITENS DO COMBO
        // =========================================

        public List<ComboItem> Itens { get; set; }
            = new List<ComboItem>();

        // =========================================
        // VALORES CALCULADOS DO COMBO
        // =========================================

        [NotMapped]
        public decimal ValorOriginal
        {
            get
            {
                return Itens?
                    .Where(i => i.Produto != null)
                    .Sum(i => i.Produto!.Preco * i.Quantidade)
                    ?? 0;
            }
        }


        [NotMapped]
        public decimal PercentualDesconto
        {
            get
            {
                if (ValorOriginal <= 0 || Preco >= ValorOriginal)
                    return 0;

                return ((ValorOriginal - Preco) / ValorOriginal) * 100;
            }
        }


        [NotMapped]
        public bool FreteGratis
        {
            get
            {
                return Preco > 250;
            }
        }
    }
}