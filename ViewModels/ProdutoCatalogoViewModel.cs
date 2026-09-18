using LojaWeb_2.Models;

namespace LojaWeb_2.ViewModels
{
    public class ProdutoCatalogoViewModel
    {
        public Produto Produto { get; set; } = null!;

        public ProdutoCor? Cor { get; set; }

        public string? Imagem { get; set; }
    }
}