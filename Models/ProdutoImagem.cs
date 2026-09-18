namespace LojaWeb_2.Models
{
    public class ProdutoImagem
    {

        public int Id { get; set; }

        public string Caminho { get; set; } = string.Empty; 


        // =========================================
        // PRODUTO
        // =========================================
        public int ProdutoId { get; set; }

        public Produto? Produto { get; set; }

        // =========================================
        // COR
        // =========================================
        public int? ProdutoCorId { get; set; }

        public ProdutoCor? ProdutoCor { get; set; }
    }
}
