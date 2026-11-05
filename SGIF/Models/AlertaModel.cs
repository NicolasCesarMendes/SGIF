namespace SGIF.Models
{
    public class Alerta
    {
        public int IdAlerta { get; set; }
        public int IdLeitura { get; set; }
        public string Nivel { get; set; } = string.Empty;   // 'Atencao' ou 'Critico'
        public string? Descricao { get; set; }
        public DateTime DataHora { get; set; } = DateTime.Now;
        public string Status { get; set; } = "Aberto";       // 'Aberto', 'Em analise' ou 'Resolvido'
    }
}
