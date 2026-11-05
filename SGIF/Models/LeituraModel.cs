namespace SGIF.Models
{
    public class Leitura
    {
        public int IdLeitura { get; set; }
        public int IdSensor { get; set; }
        public decimal Valor { get; set; }
        public DateTime DataHora { get; set; } = DateTime.Now;
    }
}
