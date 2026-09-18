using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LojaWeb_2.Models
{
    public class CarrinhoItem
    {
        public int Id { get; set; }

        // Cliente dono do item
        public int ClienteId { get; set; }

        [ForeignKey("ClienteId")]
        public Cliente Cliente { get; set; } = null!;

        // Produto
        public int ProdutoId { get; set; }

        [ForeignKey("ProdutoId")]
        public Produto Produto { get; set; } = null!;

        // Variação escolhida
        public string Cor { get; set; } = string.Empty;

        public string Tamanho { get; set; } = string.Empty;

        // Quantidade
        public int Quantidade { get; set; }

        // Preço no momento em que foi colocado no carrinho
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecoUnitario { get; set; }

        // Data em que entrou no carrinho
        public DateTime DataAdicao { get; set; } = DateTime.Now;
    }
}