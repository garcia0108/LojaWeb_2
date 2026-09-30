using System.ComponentModel.DataAnnotations;

namespace LojaWeb_2.Models
{
    public class PerfilImportacao
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nome { get; set; } = string.Empty;

        public bool TamanhosNaHorizontal { get; set; }

        public bool UsaLetrasAF { get; set; }

        [StringLength(50)]
        public string CampoTorax { get; set; } = "Torax";

        [StringLength(50)]
        public string CampoComprimento { get; set; } = "Comprimento";

        [StringLength(50)]
        public string CampoBarra { get; set; } = "Barra";

        [StringLength(50)]
        public string CampoOmbro { get; set; } = "Ombro";

        [StringLength(50)]
        public string CampoManga { get; set; } = "Manga";

        [StringLength(50)]
        public string CampoAbertura { get; set; } = "Abertura";
    }
}