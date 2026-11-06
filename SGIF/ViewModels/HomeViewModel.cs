namespace SGIF.Models
{
    // Tela Inicial (painel operacional): indicadores, ativos que exigem atenção e últimos alertas
    public class HomeViewModel
    {
        public string Nome { get; set; } = string.Empty;

        public int TotalLocomotivas { get; set; }
        public int TotalVagoes { get; set; }
        public int EmManutencao { get; set; }           // locomotivas + vagões com estado "Em manutencao"
        public int AlertasCriticos { get; set; }         // ainda não resolvidos
        public int AlertasAtencao { get; set; }          // ainda não resolvidos

        public List<AtivoAtencaoItemViewModel> AtivosEmAtencao { get; set; } = new();
        public List<AlertaItemViewModel> UltimosAlertas { get; set; } = new();

        public int TotalAtivos => TotalLocomotivas + TotalVagoes;
    }

    // Uma linha da lista "Ativos que exigem atenção"
    public class AtivoAtencaoItemViewModel
    {
        public string TipoAtivo { get; set; } = string.Empty;   // Locomotiva ou Vagao (para o link da Ficha)
        public int IdAtivo { get; set; }
        public string Ativo { get; set; } = string.Empty;       // "Locomotiva 1234"
        public int AlertasAbertos { get; set; }
        public int Criticos { get; set; }
    }
}
