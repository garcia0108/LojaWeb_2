using System.ComponentModel.DataAnnotations;

namespace LojaWeb_2.Models
{
    public class Marca
    {
        [Key]
        public int Id { get; set; } 

        [Required(ErrorMessage = "O nome da Marca é obrigatória.")]
        [StringLength(100)]
        [Display(Name = "Nome da Marca")]
        public string Nome { get; set; } = string.Empty;

        [StringLength(250)]
        [Display(Name = "Descrição")]
        public string? Descricao { get; set; }

        public ICollection<Produto> Produtos { get; set; } = new List<Produto>();
    }
}
