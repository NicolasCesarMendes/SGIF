using System.ComponentModel.DataAnnotations;

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
        public bool Ativo { get; set; } = true;   // inativo: mantém o histórico, mas não recebe leituras
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

    // Formulário de cadastro/edição de sensor (RF5 e RF6)
    public class SensorFormViewModel
    {
        public int IdSensor { get; set; }   // 0 = novo

        [Required(ErrorMessage = "Selecione o tipo")]
        public string? Tipo { get; set; }

        [Required(ErrorMessage = "Informe o modelo")]
        [StringLength(100, ErrorMessage = "Máximo de 100 caracteres")]
        public string Modelo { get; set; } = string.Empty;

        // Números como texto (aceita vírgula ou ponto): convertidos para decimal no controller
        public string? ValorMinimo { get; set; }
        public string? ValorMaximo { get; set; }
        public string? LimiteAtencao { get; set; }
        public string? LimiteCritico { get; set; }

        // Ativo ao qual o sensor pertence (RF6), no formato "Locomotiva:3" ou "Vagao:5"
        [Required(ErrorMessage = "Selecione o ativo")]
        public string? Ativo { get; set; }

        // Opções do select de ativos (preenchidas pelo controller, não vêm do formulário)
        public List<OpcaoAtivo> Locomotivas { get; set; } = new();
        public List<OpcaoAtivo> Vagoes { get; set; } = new();
    }

    public class OpcaoAtivo
    {
        public string Valor { get; set; } = string.Empty;   // "Locomotiva:3"
        public string Texto { get; set; } = string.Empty;   // "1234 - Modelo X"
    }
}
