using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LojaWeb_2.Models
{
    public class Estoque
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Selecione um produto.")]
        public int ProdutoId { get; set; }

        public Produto? Produto { get; set; }

        [Required(ErrorMessage = "Selecione uma marca.")]
        public int MarcaId { get; set; }

        public Marca? Marca { get; set; }

        [Required(ErrorMessage = "Informe o tamanho.")]
        [StringLength(10, ErrorMessage = "O tamanho deve ter no máximo 10 caracteres.")]
        public string Tamanho { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe a cor.")]
        [StringLength(50, ErrorMessage = "A cor deve ter no máximo 50 caracteres.")]
        public string Cor { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe a quantidade.")]
        [Range(1, 999999, ErrorMessage = "A quantidade deve ser maior que zero.")]
        public int Quantidade { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Custo médio")]
        public decimal CustoMedio { get; set; }
    }
}