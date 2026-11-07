namespace SGIF.Models
{
    // Página Relatórios: estatísticas e comparativos mensais (RF12), com filtros (RF13)
    public class RelatorioViewModel
    {
        // Períodos das estatísticas e dos alertas: código na URL, texto e quantos dias voltam no tempo
        public static readonly (string Codigo, string Texto, int Dias)[] Periodos =
        {
            ("7d", "7 dias", 7),
            ("30d", "30 dias", 30),
            ("90d", "90 dias", 90)
        };

        public const string PeriodoPadrao = "30d";

        // Filtros aplicados (já validados)
        public string? TipoAtivo { get; set; }
        public int? IdAtivo { get; set; }
        public string? TipoSensor { get; set; }
        public string Periodo { get; set; } = PeriodoPadrao;
        public int Meses { get; set; } = 6;
        public DateTime GeradoEm { get; set; }

        public List<EstatisticaTipo> Estatisticas { get; set; } = new();
        public ResumoAlertas Alertas { get; set; } = new();
        public List<ComparativoMes> ComparativoMensal { get; set; } = new();
    }

    // Um mês do comparativo, com os dados de cada tipo de sensor
    public class ComparativoMes
    {
        public int Ano { get; set; }
        public int Mes { get; set; }
        public string Rotulo { get; set; } = string.Empty;      // "11/2026"
        public int AlertasCriticos { get; set; }
        public int AlertasAtencao { get; set; }
        public List<ComparativoTipo> Tipos { get; set; } = new();
    }

    public class ComparativoTipo
    {
        public string TipoSensor { get; set; } = string.Empty;
        public int Quantidade { get; set; }
        public decimal Media { get; set; }
        public decimal Maximo { get; set; }
        public decimal? VariacaoMediaPercentual { get; set; }    // em relação ao último mês com dados do mesmo tipo
    }
}
