namespace SGIF.Models
{
    public class Locomotiva
    {
        public int IdLocomotiva { get; set; }
        public string NumeroIdentificacao { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string? Fabricante { get; set; }
        public int? AnoFabricacao { get; set; }
        public string Status { get; set; } = "Ativo";
        public int? IdTrem { get; set; }
    }

    public class Vagao
    {
        public int IdVagao { get; set; }
        public string NumeroIdentificacao { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public decimal? Capacidade { get; set; }
        public string Status { get; set; } = "Ativo";
        public int? IdTrem { get; set; }
    }

    public class Trem
    {
        public int IdTrem { get; set; }
        public string Identificacao { get; set; } = string.Empty;
    }
}
