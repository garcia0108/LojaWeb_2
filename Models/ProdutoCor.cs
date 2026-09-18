using System.ComponentModel.DataAnnotations;

namespace LojaWeb_2.Models
{
    public class ProdutoCor
    {
        public int Id { get; set; }

        [Required]
        public int ProdutoId { get; set; }

        public Produto? Produto { get; set; }

        // =========================================
        // COR
        // ======================================== 
        [Required(ErrorMessage = "Informe a cor.")]
        [StringLength(100)]
        [Display(Name = "Cor")]
        public string Nome { get; set; } = string.Empty;

        // Imagem principal escolhida para esta cor
        public int? ImagemPrincipalId { get; set; }

        public ProdutoImagem? ImagemPrincipal { get; set; }

        public List<ProdutoImagem> Imagens { get; set; }
            = new List<ProdutoImagem>();
    }
}