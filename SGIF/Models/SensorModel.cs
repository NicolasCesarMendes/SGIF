namespace SGIF.Models
{
    public class Sensor
    {
        public int IdSensor { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public decimal? ValorMinimo { get; set; }
        public decimal? ValorMaximo { get; set; }
        public decimal? LimiteAtencao { get; set; }
        public decimal? LimiteCritico { get; set; }
        public int? IdLocomotiva { get; set; }
        public int? IdVagao { get; set; }
    }
}
