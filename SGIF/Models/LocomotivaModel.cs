namespace SGIF.Models
{
    public class Locomotiva
    {
        public int IdLocomotiva { get; set; }
        public string NumeroIdentificacao { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string? Fabricante { get; set; }
        public int? AnoFabricacao { get; set; }
        public string Status { get; set; } = "Ativa";
        public int? IdTrem { get; set; }
    }
}
