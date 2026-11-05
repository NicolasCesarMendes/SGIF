namespace SGIF.Models
{
    public class Usuario
    {
        public int IdUsuario { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
        public int NivelAcesso { get; set; } = 3;
        public bool Ativo { get; set; } = true;
    }
}
