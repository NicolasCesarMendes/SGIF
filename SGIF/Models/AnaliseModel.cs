namespace SGIF.Models
{
    // Um ponto de uma série temporal: as leituras de um intervalo (ex.: 15 minutos) agrupadas
    public class PontoSerie
    {
        public DateTime Inicio { get; set; }
        public decimal Media { get; set; }
        public decimal Minimo { get; set; }
        public decimal Maximo { get; set; }
        public int Quantidade { get; set; }
    }

    // Tendência de um sensor no período (regressão linear sobre a série)
    public class TendenciaSensor
    {
        public string Direcao { get; set; } = "Insuficiente";   // Subindo, Descendo, Estavel ou Insuficiente
        public double InclinacaoPorHora { get; set; }            // quanto o valor muda por hora
        public double R2 { get; set; }                           // 0 a 1: o quanto a reta explica os dados
        public double? HorasAteAtencao { get; set; }             // só quando está subindo e há limite de atenção
        public double? HorasAteCritico { get; set; }             // só quando está subindo e há limite crítico
    }

    // Leituras fora do padrão do próprio sensor (afastadas da média por mais de N desvios-padrão)
    public class AnomaliasSensor
    {
        public int Quantidade { get; set; }
        public int TotalLeituras { get; set; }                   // leituras do período usadas no cálculo
        public double EsperadasPorAcaso { get; set; }            // quantas cairiam fora só por acaso (dados normais)
        public decimal? Media { get; set; }
        public decimal? Desvio { get; set; }
        public decimal? LimiteInferior { get; set; }
        public decimal? LimiteSuperior { get; set; }
        public List<Leitura> Ultimas { get; set; } = new();      // as mais recentes
    }

    // Risco de falha de um ativo (pontuação de 0 a 100, regra inicial a ser validada pela equipe)
    public class RiscoAtivo
    {
        public string TipoAtivo { get; set; } = string.Empty;    // Locomotiva ou Vagao
        public int IdAtivo { get; set; }
        public string Numero { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;    // modelo (locomotiva) ou tipo (vagão)
        public string Estado { get; set; } = string.Empty;
        public int Pontuacao { get; set; }
        public string Classificacao { get; set; } = "Baixo";     // Baixo, Medio, Alto ou Critico
        public List<string> Motivos { get; set; } = new();
    }

    // Filtros das consultas de análise e relatórios (RF13). Tudo opcional.
    public class FiltroAnalise
    {
        public string? TipoAtivo { get; set; }     // Locomotiva ou Vagao (null = todos)
        public int? IdAtivo { get; set; }          // exige TipoAtivo
        public string? TipoSensor { get; set; }    // Temperatura ou Vibracao (null = todos)
    }

    // Estatísticas das leituras de um tipo de sensor no período
    public class EstatisticaTipo
    {
        public string TipoSensor { get; set; } = string.Empty;
        public int Sensores { get; set; }
        public int Quantidade { get; set; }
        public decimal? Minimo { get; set; }
        public decimal? Maximo { get; set; }
        public decimal? Media { get; set; }
        public decimal? Desvio { get; set; }
    }

    // Leituras de um tipo de sensor em um mês
    public class ResumoMensal
    {
        public int Ano { get; set; }
        public int Mes { get; set; }
        public string TipoSensor { get; set; } = string.Empty;
        public int Quantidade { get; set; }
        public decimal Media { get; set; }
        public decimal Maximo { get; set; }
    }

    // Alertas agrupados por nível e situação no período
    public class ResumoAlertas
    {
        public int Total { get; set; }
        public int Criticos { get; set; }
        public int Atencao { get; set; }
        public int Abertos { get; set; }
        public int EmAnalise { get; set; }
        public int Resolvidos { get; set; }
        public double? HorasMediasParaResolver { get; set; }
    }
}
