using System.ComponentModel.DataAnnotations;

namespace SGIF.Models
{
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
