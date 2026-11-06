namespace SGIF.Models
{
    // Ficha do ativo (locomotiva ou vagão): dados do ativo e seus sensores
    public class FichaViewModel
    {
        public string Tipo { get; set; } = "Locomotiva";     // Locomotiva ou Vagao
        public int Id { get; set; }
        public string Numero { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public bool PodeEditar { get; set; }                  // Administrador e Manutenção

        public List<(string Rotulo, string Valor)> Detalhes { get; set; } = new();
        public List<SensorItemViewModel> Sensores { get; set; } = new();
        public List<AlertaItemViewModel> Alertas { get; set; } = new();   // alertas mais recentes do ativo

        public string NomeTipo => Tipo == "Vagao" ? "Vagão" : "Locomotiva";

        // Períodos do histórico (RF8/RF13): código na URL, texto do botão e quantas horas voltam no tempo
        public static readonly (string Codigo, string Texto, int Horas)[] Periodos =
        {
            ("1h", "Última hora", 1),
            ("24h", "24 horas", 24),
            ("7d", "7 dias", 24 * 7),
            ("30d", "30 dias", 24 * 30)
        };

        public const string PeriodoPadrao = "24h";

        public string Periodo { get; set; } = PeriodoPadrao;
    }

    // Uma linha da tabela de sensores da ficha (textos já prontos para exibir)
    public class SensorItemViewModel
    {
        public int Id { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string Faixa { get; set; } = string.Empty;
        public string LimiteAtencao { get; set; } = string.Empty;
        public string LimiteCritico { get; set; } = string.Empty;

        // Última leitura registrada (vazio se o sensor ainda não tem leituras)
        public string UltimaLeitura { get; set; } = "-";
        public string? UltimaLeituraEm { get; set; }
        public string NivelLeitura { get; set; } = string.Empty;   // "", Atencao ou Critico (cor do valor)

        // Últimas leituras do sensor (mais recentes primeiro)
        public List<LeituraItemViewModel> Leituras { get; set; } = new();

        // Estatísticas do período escolhido (texto pronto; "-" se não houver leituras)
        public int QuantidadeNoPeriodo { get; set; }
        public string Minimo { get; set; } = "-";
        public string Maximo { get; set; } = "-";
        public string Media { get; set; } = "-";
    }

    // Uma linha do histórico de leituras de um sensor
    public class LeituraItemViewModel
    {
        public string Valor { get; set; } = string.Empty;
        public string DataHora { get; set; } = string.Empty;
        public string Nivel { get; set; } = string.Empty;   // "", Atencao ou Critico (cor do valor)
    }
}
