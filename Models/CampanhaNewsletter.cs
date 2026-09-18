using System.ComponentModel.DataAnnotations;

namespace LojaWeb_2.Models
{
    public class CampanhaNewsletter
    {
        public int Id { get; set; }


        [Required(ErrorMessage = "Informe o assunto da campanha.")]
        [StringLength(150)]
        [Display(Name = "Assunto")]
        public string Assunto { get; set; } = string.Empty;


        [Required(ErrorMessage = "Informe a mensagem.")]
        [Display(Name = "Mensagem")]
        public string Mensagem { get; set; } = string.Empty;


        [Display(Name = "Data de criação")]
        public DateTime DataCriacao { get; set; } = DateTime.Now;


        [Display(Name = "Enviada")]
        public bool Enviada { get; set; } = false;


        [Display(Name = "Data de envio")]
        public DateTime? DataEnvio { get; set; }
    }
}