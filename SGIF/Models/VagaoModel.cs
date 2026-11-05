namespace SGIF.Models
{
    public class Vagao
    {
        public int IdVagao { get; set; }
        public string NumeroIdentificacao { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public decimal? Capacidade { get; set; }
        public string Status { get; set; } = "Ativo";
        public int? IdTrem { get; set; }
    }
}
