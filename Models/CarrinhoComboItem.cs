using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LojaWeb_2.Models
{
    public class CarrinhoComboItem
    {
        public int Id { get; set; }

        // CLIENTE
        public int ClienteId { get; set; }

        [ForeignKey("ClienteId")]
        public Cliente Cliente { get; set; } = null!;

        // COMBO
        public int ComboId { get; set; }

        [ForeignKey("ComboId")]
        public Combo Combo { get; set; } = null!;

        // QUANTIDADE DE COMBOS
        public int Quantidade { get; set; }

        // PREÇO DO COMBO NO MOMENTO DA COMPRA
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecoUnitario { get; set; }

        // DATA
        public DateTime DataAdicao { get; set; } = DateTime.Now;

        public List<CarrinhoComboSelecao> Selecoes { get; set; }
           = new List<CarrinhoComboSelecao>();
    }
}