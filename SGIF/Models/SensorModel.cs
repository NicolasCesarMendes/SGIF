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
        public bool Ativo { get; set; } = true;   // inativo: mantém o histórico, mas não recebe novas leituras
    }

    // Tipos de sensor aceitos pelo sistema (RF5). O banco não valida o tipo:
    // quem garante é o select do formulário e a conferência no controller.
    // Os valores gravados não têm acento (mesma regra dos estados dos ativos).
    public static class TiposSensor
    {
        public const string Temperatura = "Temperatura";
        public const string Vibracao = "Vibracao";

        public static readonly string[] Todos = { Temperatura, Vibracao };

        // Nome para exibir na tela
        public static string Nome(string tipo)
        {
            return tipo == Vibracao ? "Vibração" : tipo;
        }

        // Unidade de medida exibida junto dos valores (ajuste aqui se o dataset usar outra)
        public static string Unidade(string tipo)
        {
            return tipo switch
            {
                Temperatura => "°C",
                Vibracao => "mm/s",
                _ => ""
            };
        }
    }
}
