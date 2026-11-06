using SGIF.Classes;

namespace SGIF.Models
{
    // Tela Alertas: fila paginada, com filtro por situação
    public class AlertasViewModel
    {
        // Situações dos alertas (os valores gravados no banco não têm acento)
        public static readonly string[] Situacoes = { "Aberto", "Em analise", "Resolvido" };

        public static string NomeSituacao(string situacao)
        {
            return situacao == "Em analise" ? "Em análise" : situacao;
        }

        public static string NomeNivel(string nivel)
        {
            return nivel == "Atencao" ? "Atenção" : nivel == "Critico" ? "Crítico" : nivel;
        }

        public List<AlertaItemViewModel> Itens { get; set; } = new();
        public string? Status { get; set; }               // filtro atual (null = todos)
        public int Pagina { get; set; } = 1;
        public int TotalPaginas { get; set; } = 1;
        public int TotalFiltrado { get; set; }
        public bool PodeAtualizar { get; set; }            // Administrador e Manutenção

        // Quantos alertas há em cada situação (números dos botões de filtro)
        public Dictionary<string, int> Contagens { get; set; } = new();
        public int ContagemDe(string situacao) => Contagens.TryGetValue(situacao, out int n) ? n : 0;
        public int ContagemTotal => Contagens.Values.Sum();
    }

    // Uma linha da fila (textos já prontos para exibir)
    public class AlertaItemViewModel
    {
        public int IdAlerta { get; set; }
        public string Nivel { get; set; } = string.Empty;       // Atencao ou Critico
        public string Status { get; set; } = string.Empty;      // Aberto, Em analise ou Resolvido
        public string Descricao { get; set; } = string.Empty;
        public string DataHora { get; set; } = string.Empty;
        public string TipoAtivo { get; set; } = string.Empty;   // Locomotiva ou Vagao (para o link da Ficha)
        public int IdAtivo { get; set; }
        public string Ativo { get; set; } = string.Empty;       // "Locomotiva 1234"
        public string Sensor { get; set; } = string.Empty;      // "Temperatura - Modelo X"
        public string ValorLido { get; set; } = string.Empty;   // "105,2 °C"
        public string Responsavel { get; set; } = string.Empty;
        public string? DataResolucao { get; set; }

        // Transforma o registro do banco em textos prontos para a tela
        // (usado na tela Alertas e na Ficha do ativo)
        public static AlertaItemViewModel De(AlertaDetalhe d)
        {
            bool locomotiva = d.IdLocomotiva.HasValue;

            return new AlertaItemViewModel
            {
                IdAlerta = d.IdAlerta,
                Nivel = d.Nivel,
                Status = d.Status,
                Descricao = d.Descricao ?? "-",
                DataHora = d.DataHora.ToString("dd/MM/yyyy HH:mm:ss"),
                TipoAtivo = locomotiva ? "Locomotiva" : "Vagao",
                IdAtivo = locomotiva ? d.IdLocomotiva!.Value : d.IdVagao ?? 0,
                Ativo = locomotiva ? "Locomotiva " + d.NumeroLocomotiva : "Vagão " + d.NumeroVagao,
                Sensor = TiposSensor.Nome(d.TipoSensor) + " - " + d.ModeloSensor,
                ValorLido = (EntradaNumerica.Formatar(d.Valor) + " " + TiposSensor.Unidade(d.TipoSensor)).Trim(),
                Responsavel = d.NomeResponsavel ?? "-",
                DataResolucao = d.DataResolucao?.ToString("dd/MM/yyyy HH:mm:ss")
            };
        }
    }

    // Conteúdo do modal de um alerta (detalhes e, para quem pode, a troca de situação)
    public class AlertaFormViewModel
    {
        public AlertaItemViewModel Alerta { get; set; } = new();
        public bool PodeAtualizar { get; set; }
    }
}
