using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LojaWeb_2.Models
{
    public class MovimentacaoEstoque
    {
            public int Id { get; set; }

            [Required]
            public int ProdutoId { get; set; }

            public Produto? Produto { get; set; }

            [Required(ErrorMessage = "Selecione uma marca.")]
            public int MarcaId { get; set; }

            public Marca? Marca { get; set; }
  
            public int? FornecedorId { get; set; }

            public Fornecedor? Fornecedor { get; set; }

            [Required(ErrorMessage = "Informe o tamanho.")]
            [StringLength(10)]
            public string Tamanho { get; set; } = string.Empty;

            [Required(ErrorMessage = "Informe a cor.")]
            [StringLength(50)]
            public string Cor { get; set; } = string.Empty;

            [Required(ErrorMessage = "Informe a quantidade.")]
            [Range(1, 999999)]
            public int Quantidade { get; set; }

            [Required(ErrorMessage = "Informe o preço de compra.")]
            [Range(0.01, 999999.99,
               ErrorMessage = "Informe um preço de compra válido.")]
            [Column(TypeName = "decimal(18,2)")]
            [Display(Name = "Preço de compra")]
            public decimal PrecoCompraUnitario { get; set; }

            public TipoMovimentacao Tipo { get; set; }

            public DateTime Data { get; set; } = DateTime.Now;

            [StringLength(250)]
            public string? Observacao { get; set; }     

    }
}
