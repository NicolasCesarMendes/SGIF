using SGIF.Models;

namespace SGIF.Classes
{
    // Análises dos dados dos sensores (RF10, RF12): séries para gráficos, tendências,
    // anomalias e risco de falha dos ativos.
    // São regras simples e explicáveis (estatística descritiva e regressão linear), pensadas para a equipe
    // poder justificar cada resultado. Os valores marcados como "parâmetros" devem ser validados com a
    // equipe de manutenção.
    public class AnaliseSensores
    {
        // ---------- Parâmetros das regras ----------
        // Limite de 3 desvios-padrão (k = 3): padrão aceito na indústria (NIST/SEMATECH e-Handbook, cap. 6.3).
        // Com dados normais, ~1 leitura em cada 370 passa dele só por acaso (falso alarme).
        public const double LimiarAnomalia = 3.0;
        public const double UmFalsoAlarmeACada = 370.4;
        public const int MinimoLeiturasParaAnomalia = 30;
        public const int HorasDaAnaliseDeRisco = 24;        // janela usada no plano de Manutenção

        private const int MinimoPontosParaTendencia = 6;
        private const double R2Minimo = 0.30;               // a reta precisa explicar pelo menos 30% da variação
        // A mudança no período precisa ser relevante: 25% do limite de atenção (adaptação do critério de
        // "mudança significativa" da ISO 20816, que usa 25% do limite da zona B/C); sem limite, 5% da média.
        private const double VariacaoMinimaDoLimite = 0.25;
        private const double VariacaoMinimaDaMedia = 0.05;
        private const double HorizonteMaximoHoras = 24 * 7; // não projeta além de 7 dias

        private readonly Leituras _leituras;
        private readonly Sensores _sensores;
        private readonly Alertas _alertas;
        private readonly Locomotivas _locomotivas;
        private readonly Vagoes _vagoes;

        public AnaliseSensores(Leituras leituras, Sensores sensores, Alertas alertas, Locomotivas locomotivas, Vagoes vagoes)
        {
            _leituras = leituras;
            _sensores = sensores;
            _alertas = alertas;
            _locomotivas = locomotivas;
            _vagoes = vagoes;
        }

        // ---------- Períodos e séries ----------

        // Aceita só os códigos conhecidos (1h, 24h, 7d, 30d); qualquer outro valor usa o padrão
        public static (string Codigo, int Horas) EscolherPeriodo(string? codigo)
        {
            var escolhido = FichaViewModel.Periodos.FirstOrDefault(p => p.Codigo == codigo);

            if (escolhido.Codigo == null)
            {
                escolhido = FichaViewModel.Periodos.First(p => p.Codigo == FichaViewModel.PeriodoPadrao);
            }

            return (escolhido.Codigo, escolhido.Horas);
        }

        // Tamanho do intervalo de cada ponto do gráfico: mantém algumas dezenas ou poucas centenas de pontos
        public static int MinutosPorPonto(int horas)
        {
            if (horas <= 1) return 1;
            if (horas <= 24) return 15;
            if (horas <= 24 * 7) return 60;
            if (horas <= 24 * 30) return 360;
            return 1440;
        }

        public List<PontoSerie> Serie(int idSensor, int horas)
        {
            return _leituras.SerieAgregada(idSensor, DateTime.Now.AddHours(-horas), MinutosPorPonto(horas));
        }

        // Média da frota inteira, por tipo de sensor (gráficos do painel)
        public List<PontoSerie> SerieDaFrota(string tipoSensor, int horas)
        {
            return _leituras.SerieAgregadaPorTipo(tipoSensor, DateTime.Now.AddHours(-horas), MinutosPorPonto(horas));
        }

        // ---------- Tendência ----------

        // Ajusta uma reta às médias da série. Se o valor está subindo e o sensor tem limites,
        // estima em quantas horas ele chegaria a cada limite, mantido o ritmo atual.
        public static TendenciaSensor CalcularTendencia(Sensor sensor, List<PontoSerie> pontos)
        {
            TendenciaSensor tendencia = new TendenciaSensor();

            if (pontos.Count < MinimoPontosParaTendencia)
            {
                return tendencia;   // Insuficiente
            }

            DateTime origem = pontos[0].Inicio;
            double[] x = pontos.Select(p => (p.Inicio - origem).TotalHours).ToArray();
            double[] y = pontos.Select(p => (double)p.Media).ToArray();

            Estatistica.RegressaoLinear(x, y, out double inclinacao, out double intercepto, out double r2);

            double horasObservadas = x[^1] - x[0];
            double mediaGeral = y.Average();
            double variacao = Math.Abs(inclinacao * horasObservadas);

            // Quanto o valor precisa mudar no período para a tendência valer a pena
            double variacaoMinima = sensor.LimiteAtencao.HasValue
                ? Math.Abs((double)sensor.LimiteAtencao.Value) * VariacaoMinimaDoLimite
                : Math.Abs(mediaGeral) * VariacaoMinimaDaMedia;

            tendencia.InclinacaoPorHora = inclinacao;
            tendencia.R2 = r2;

            if (r2 >= R2Minimo && variacao >= variacaoMinima && variacao > 0)
            {
                tendencia.Direcao = inclinacao > 0 ? "Subindo" : "Descendo";
            }
            else
            {
                tendencia.Direcao = "Estavel";
            }

            if (tendencia.Direcao == "Subindo")
            {
                double valorAgora = intercepto + inclinacao * x[^1];

                tendencia.HorasAteAtencao = HorasAteLimite(sensor.LimiteAtencao, valorAgora, inclinacao);
                tendencia.HorasAteCritico = HorasAteLimite(sensor.LimiteCritico, valorAgora, inclinacao);
            }

            return tendencia;
        }

        // 0 = já passou do limite; null = sem limite ou longe demais para projetar
        private static double? HorasAteLimite(decimal? limite, double valorAgora, double inclinacao)
        {
            if (!limite.HasValue)
            {
                return null;
            }

            double faltam = (double)limite.Value - valorAgora;

            if (faltam <= 0)
            {
                return 0;
            }

            double horas = faltam / inclinacao;

            return horas <= HorizonteMaximoHoras ? horas : null;
        }

        // ---------- Anomalias ----------

        // Leituras fora do padrão do próprio sensor no período (média ± 3 desvios-padrão).
        // É diferente do alerta: o alerta compara com os limites cadastrados; aqui a comparação é com o histórico.
        public AnomaliasSensor Anomalias(int idSensor, int horas, int quantidadeDeExemplos = 5)
        {
            DateTime inicio = DateTime.Now.AddHours(-horas);
            LeituraResumo resumo = _leituras.ResumirPorSensor(idSensor, inicio);

            AnomaliasSensor anomalias = new AnomaliasSensor
            {
                Media = resumo.Media,
                Desvio = resumo.Desvio,
                TotalLeituras = resumo.Quantidade,
                EsperadasPorAcaso = resumo.Quantidade / UmFalsoAlarmeACada
            };

            if (resumo.Quantidade < MinimoLeiturasParaAnomalia || !resumo.Media.HasValue
                || !resumo.Desvio.HasValue || resumo.Desvio.Value <= 0)
            {
                return anomalias;
            }

            decimal faixa = (decimal)LimiarAnomalia * resumo.Desvio.Value;
            decimal inferior = resumo.Media.Value - faixa;
            decimal superior = resumo.Media.Value + faixa;

            anomalias.LimiteInferior = inferior;
            anomalias.LimiteSuperior = superior;
            anomalias.Quantidade = _leituras.ContarForaDaFaixa(idSensor, inicio, inferior, superior);

            if (quantidadeDeExemplos > 0 && anomalias.Quantidade > 0)
            {
                anomalias.Ultimas = _leituras.ListarForaDaFaixa(idSensor, inicio, inferior, superior, quantidadeDeExemplos);
            }

            return anomalias;
        }

        // ---------- Risco de falha (plano de Manutenção) ----------

        // Pontuação de 0 a 100 por ativo, somando:
        //   alertas não resolvidos (até 50) + leitura atual (até 20) + tendência de alta (até 20) + anomalias (até 10).
        // Ativos inativos ficam de fora. Ordena do maior para o menor risco.
        public List<RiscoAtivo> RankingDeRisco()
        {
            var alertasAbertos = _alertas.ContarNaoResolvidosPorAtivo();
            List<RiscoAtivo> ranking = new List<RiscoAtivo>();

            foreach (Locomotiva l in _locomotivas.ListarTodos().Where(l => l.Status != "Inativo"))
            {
                ranking.Add(AvaliarAtivo("Locomotiva", l.IdLocomotiva, l.NumeroIdentificacao, l.Modelo, l.Status,
                    _sensores.ListarPorLocomotiva(l.IdLocomotiva).Where(s => s.Ativo).ToList(), alertasAbertos));
            }

            foreach (Vagao v in _vagoes.ListarTodos().Where(v => v.Status != "Inativo"))
            {
                ranking.Add(AvaliarAtivo("Vagao", v.IdVagao, v.NumeroIdentificacao, v.Tipo, v.Status,
                    _sensores.ListarPorVagao(v.IdVagao).Where(s => s.Ativo).ToList(), alertasAbertos));
            }

            return ranking.OrderByDescending(r => r.Pontuacao).ThenBy(r => r.Numero).ToList();
        }

        private RiscoAtivo AvaliarAtivo(string tipoAtivo, int idAtivo, string numero, string descricao, string estado,
            List<Sensor> sensores, Dictionary<(string Tipo, int Id), (int Criticos, int Atencao)> alertasAbertos)
        {
            RiscoAtivo risco = new RiscoAtivo
            {
                TipoAtivo = tipoAtivo,
                IdAtivo = idAtivo,
                Numero = numero,
                Descricao = descricao,
                Estado = estado
            };

            int pontos = 0;

            // 1) Alertas ainda não resolvidos (até 50 pontos)
            alertasAbertos.TryGetValue((tipoAtivo, idAtivo), out var alertas);

            if (alertas.Criticos > 0)
            {
                pontos += Math.Min(35, alertas.Criticos * 15);
                risco.Motivos.Add(alertas.Criticos + " alerta(s) crítico(s) não resolvido(s)");
            }

            if (alertas.Atencao > 0)
            {
                pontos += Math.Min(15, alertas.Atencao * 5);
                risco.Motivos.Add(alertas.Atencao + " alerta(s) de atenção não resolvido(s)");
            }

            if (sensores.Count == 0)
            {
                risco.Motivos.Add("Sem sensores ativos: o risco não pode ser avaliado pelos dados");
            }

            int pontosLeitura = 0;
            int pontosTendencia = 0;
            int excessoDeAnomalias = 0;

            foreach (Sensor sensor in sensores)
            {
                string nome = TiposSensor.Nome(sensor.Tipo);
                string unidade = TiposSensor.Unidade(sensor.Tipo);

                // 2) Leitura mais recente (até 20 pontos)
                Leitura? ultima = _leituras.BuscarUltimaPorSensor(sensor.IdSensor);
                string? nivel = ultima != null ? ProcessadorLeituras.NivelDoAlerta(sensor, ultima.Valor) : null;

                if (nivel != null)
                {
                    int pontosDoNivel = nivel == ProcessadorLeituras.NivelCritico ? 20 : 10;
                    string nomeNivel = nivel == ProcessadorLeituras.NivelCritico ? "crítico" : "de atenção";

                    pontosLeitura = Math.Max(pontosLeitura, pontosDoNivel);
                    risco.Motivos.Add(nome + ": leitura atual no nível " + nomeNivel + " ("
                        + (EntradaNumerica.Formatar(ultima!.Valor) + " " + unidade).Trim() + ")");
                }

                // 3) Tendência de alta nas últimas horas (até 20 pontos)
                TendenciaSensor tendencia = CalcularTendencia(sensor, Serie(sensor.IdSensor, HorasDaAnaliseDeRisco));
                int pontosDaTendencia = PontosDaTendencia(tendencia);

                if (pontosDaTendencia > 0)
                {
                    pontosTendencia = Math.Max(pontosTendencia, pontosDaTendencia);
                    risco.Motivos.Add(nome + " em alta: " + TextoDaProjecao(tendencia));
                }

                // 4) Leituras fora do padrão (até 10 pontos). Só conta o que passa do esperado por acaso:
                //    com 3 sigma, ~1 leitura em 370 já cai fora mesmo com o equipamento normal.
                AnomaliasSensor anomalias = Anomalias(sensor.IdSensor, HorasDaAnaliseDeRisco, 0);

                if (anomalias.Quantidade >= 5 && anomalias.Quantidade > 2 * anomalias.EsperadasPorAcaso)
                {
                    excessoDeAnomalias += (int)Math.Round(anomalias.Quantidade - anomalias.EsperadasPorAcaso);
                }
            }

            pontos += pontosLeitura + pontosTendencia;

            if (excessoDeAnomalias >= 10)
            {
                pontos += Math.Min(10, excessoDeAnomalias / 10);
                risco.Motivos.Add(excessoDeAnomalias + " leituras fora do padrão, além do esperado, nas últimas " + HorasDaAnaliseDeRisco + " h");
            }

            if (estado == "Em manutencao")
            {
                risco.Motivos.Add("Ativo já está em manutenção");
            }

            risco.Pontuacao = Math.Min(100, pontos);
            risco.Classificacao = risco.Pontuacao >= 70 ? "Critico"
                                : risco.Pontuacao >= 40 ? "Alto"
                                : risco.Pontuacao >= 15 ? "Medio"
                                : "Baixo";

            return risco;
        }

        private static int PontosDaTendencia(TendenciaSensor t)
        {
            if (t.HorasAteCritico.HasValue)
            {
                if (t.HorasAteCritico.Value <= 24) return 20;
                if (t.HorasAteCritico.Value <= 72) return 12;
                return 6;
            }

            if (t.HorasAteAtencao.HasValue && t.HorasAteAtencao.Value <= 24)
            {
                return 8;
            }

            return 0;
        }

        private static string TextoDaProjecao(TendenciaSensor t)
        {
            if (t.HorasAteCritico.HasValue)
            {
                return "pode atingir o limite crítico " + TextoDeHoras(t.HorasAteCritico.Value);
            }

            return "pode atingir o limite de atenção " + TextoDeHoras(t.HorasAteAtencao ?? 0);
        }

        private static string TextoDeHoras(double horas)
        {
            if (horas <= 0) return "agora";
            if (horas < 1) return "em menos de 1 h";
            return "em cerca de " + Math.Round(horas) + " h";
        }
    }
}
