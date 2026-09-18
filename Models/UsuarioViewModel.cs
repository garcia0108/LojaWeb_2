namespace LojaWeb_2.Models
{
    public class UsuarioViewModel
    {
        public string Id { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Perfil { get; set; } = string.Empty;

        public bool Ativo {  get; set; }    
    }
}
