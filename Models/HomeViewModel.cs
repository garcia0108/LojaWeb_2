using LojaWeb_2.Models;

namespace LojaWeb_2.ViewModels
{
    public class HomeViewModel
    {
        public List<Produto> ProdutosDestaque { get; set; }
            = new List<Produto>();

        public List<Produto> MaisVendidos { get; set; }
            = new List<Produto>();
    }
}