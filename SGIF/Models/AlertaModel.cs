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
        public int? IdUsuarioResponsavel { get; set; }
        public DateTime? DataResolucao { get; set; }
    }

    // Um alerta junto com os dados que a tela de Alertas precisa mostrar
    // (leitura, sensor, ativo e responsável), vindos de um único SELECT com JOIN.
    public class AlertaDetalhe
    {
        public int IdAlerta { get; set; }
        public string Nivel { get; set; } = string.Empty;       // 'Atencao' ou 'Critico'
        public string? Descricao { get; set; }
        public DateTime DataHora { get; set; }
        public string Status { get; set; } = string.Empty;       // 'Aberto', 'Em analise' ou 'Resolvido'
        public DateTime? DataResolucao { get; set; }

        public int? IdUsuarioResponsavel { get; set; }
        public string? NomeResponsavel { get; set; }

        public decimal Valor { get; set; }                       // valor da leitura que gerou o alerta

        public int IdSensor { get; set; }
        public string TipoSensor { get; set; } = string.Empty;
        public string ModeloSensor { get; set; } = string.Empty;

        public int? IdLocomotiva { get; set; }
        public int? IdVagao { get; set; }
        public string? NumeroLocomotiva { get; set; }
        public string? NumeroVagao { get; set; }
    }

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
