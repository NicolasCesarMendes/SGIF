namespace SGIF.Models
{
    // Estatísticas simples das leituras de um sensor em um período (calculadas no banco)
    public class LeituraResumo
    {
        public int Quantidade { get; set; }
        public decimal? Minimo { get; set; }
        public decimal? Maximo { get; set; }
        public decimal? Media { get; set; }
    }
}
