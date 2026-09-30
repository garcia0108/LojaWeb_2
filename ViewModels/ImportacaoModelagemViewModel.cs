using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace LojaWeb_2.ViewModels
{
    public class ImportacaoModelagemViewModel
    {
        [Required(ErrorMessage = "Selecione uma modelagem.")]
        public int ModelagemId { get; set; }

        [Required(ErrorMessage = "Selecione um arquivo Excel.")]
        public IFormFile? ArquivoExcel { get; set; }

        public List<SelectListItem> Modelagens { get; set; } = new();

        // Prévia da planilha
        public List<LinhaImportacaoViewModel> Linhas { get; set; } = new();
    }

    public class LinhaImportacaoViewModel
    {
        public string Tamanho { get; set; } = "";
        public decimal Torax { get; set; }
        public decimal Comprimento { get; set; }
        public decimal Barra { get; set; }
        public decimal Ombro { get; set; }
        public decimal ComprimentoManga { get; set; }
        public decimal AberturaManga { get; set; }
    }
}