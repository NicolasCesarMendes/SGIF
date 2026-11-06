namespace SGIF.Models
{
    // Ativo com alertas ainda não resolvidos (lista "ativos que exigem atenção" da Tela Inicial)
    public class AtivoAtencao
    {
        public int? IdLocomotiva { get; set; }
        public int? IdVagao { get; set; }
        public string? NumeroLocomotiva { get; set; }
        public string? NumeroVagao { get; set; }
        public int AlertasAbertos { get; set; }
        public int Criticos { get; set; }
    }
}
