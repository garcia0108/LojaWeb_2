using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace LojaWeb_2.ViewModels
{
    public class ProdutoCorCadastroViewModel
    {
        [Required(ErrorMessage = "Informe o nome da cor.")]
        [StringLength(100)]
        public string Nome { get; set; } = string.Empty;

        public List<IFormFile>? Imagens { get; set; }
    }
}