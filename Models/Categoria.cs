using System.ComponentModel.DataAnnotations;

namespace LojaWeb_2.Models
{
    public class Categoria
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Informe o nome da categoria")]
        [StringLength(100)]
        public string Nome { get; set; } = string.Empty;

        [StringLength(250)]
        public string? Descricao { get; set; }

        // =========================================
        // IMAGEM DA CATEGORIA
        // =========================================

        [StringLength(300)]
        [Display(Name = "Imagem")]
        public string? Imagem { get; set; }

        // =========================================
        // IMAGENS PARA OS CARROSSÉIS
        // =========================================

        [StringLength(300)]
        [Display(Name = "Imagem Masculina")]
        public string? ImagemMasculina { get; set; }


        [StringLength(300)]
        [Display(Name = "Imagem Feminina")]
        public string? ImagemFeminina { get; set; }

        public ICollection<Produto> Produtos { get; set; }
            = new List<Produto>();
    }
}