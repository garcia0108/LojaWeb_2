namespace LojaWeb_2.Models
{
    public class ItemCarrinho
    {
        public int ProdutoId { get; set; }

        public int? ComboId { get; set; }

        public bool EhCombo { get; set; }

        public string NomeProduto { get; set; } = string.Empty;

        public string? Imagem { get; set; }

        public string Cor { get; set; } = string.Empty;

        public string Tamanho { get; set; } = string.Empty;

        public decimal PrecoUnitario { get; set; }

        public int Quantidade { get; set; }

        public decimal Subtotal =>
            PrecoUnitario * Quantidade;
    }
}