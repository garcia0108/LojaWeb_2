using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace LojaWeb_2.Models
{
    public class Modelagem
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Informe o nome da modelagem.")]
        [StringLength(50)]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Selecione uma marca.")]
        public int MarcaId { get; set; }

        [ValidateNever]
        public Marca Marca { get; set; } = null!;

        [Required(ErrorMessage = "Selecione uma categoria.")]
        public int CategoriaId { get; set; }

        [ValidateNever]
        public Categoria Categoria { get; set; } = null!;

        [StringLength(300)]
        public string? Descricao { get; set; }

        [StringLength(300)]
        public string? ImagemReferencia { get; set; }

        [Required]
        [StringLength(20)]
        public string ModoIlustracao { get; set; } = "Svg";

        public List<MedidaModelagem> Medidas { get; set; } = new();

        public virtual ICollection<Produto> Produtos { get; set; } = new List<Produto>();
    }
}