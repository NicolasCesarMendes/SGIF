using System.Data;
using Microsoft.Data.SqlClient;
using SGIF.Models;

namespace SGIF.Classes
{
    // Regra do sistema para as leituras (RF11): grava a leitura e, se ela passou dos
    // limites do sensor, cria o alerta. Serve para o simulador e para qualquer outra
    // entrada de dados (sensores reais, arquivo, API...).
    public class ProcessadorLeituras
    {
        public const string NivelAtencao = "Atencao";
        public const string NivelCritico = "Critico";

        private readonly Leituras _leituras;
        private readonly Alertas _alertas;

        public ProcessadorLeituras(Leituras leituras, Alertas alertas)
        {
            _leituras = leituras;
            _alertas = alertas;
        }

        // Devolve o alerta criado (ou null, se não houve alerta novo)
        public Alerta? Registrar(Sensor sensor, decimal valor, DateTime dataHora)
        {
            // Sensor desativado não recebe leituras (o histórico antigo é preservado)
            if (!sensor.Ativo)
            {
                return null;
            }

            Leitura leitura = new Leitura
            {
                IdSensor = sensor.IdSensor,
                Valor = valor,
                DataHora = dataHora
            };

            _leituras.Inserir(leitura);

            string? nivel = NivelDoAlerta(sensor, valor);

            // Sem alerta, ou o sensor já tem um alerta deste nível ainda não resolvido
            // (evita criar o mesmo alerta a cada leitura enquanto o problema continua)
            if (nivel == null || _alertas.ExisteNaoResolvidoParaSensor(sensor.IdSensor, nivel))
            {
                return null;
            }

            Alerta alerta = new Alerta
            {
                IdLeitura = leitura.IdLeitura,
                Nivel = nivel,
                Descricao = MontarDescricao(sensor, valor, nivel),
                DataHora = dataHora
            };

            _alertas.Inserir(alerta);

            return alerta;
        }

        // Regra: valor >= limite crítico = Crítico; valor >= limite de atenção = Atenção; senão, normal (null).
        public static string? NivelDoAlerta(Sensor sensor, decimal valor)
        {
            if (sensor.LimiteCritico.HasValue && valor >= sensor.LimiteCritico.Value)
            {
                return NivelCritico;
            }

            if (sensor.LimiteAtencao.HasValue && valor >= sensor.LimiteAtencao.Value)
            {
                return NivelAtencao;
            }

            return null;
        }

        private static string MontarDescricao(Sensor sensor, decimal valor, string nivel)
        {
            string unidade = TiposSensor.Unidade(sensor.Tipo);
            decimal limite = nivel == NivelCritico ? sensor.LimiteCritico!.Value : sensor.LimiteAtencao!.Value;
            string nomeLimite = nivel == NivelCritico ? "crítico" : "de atenção";

            string valorTexto = (EntradaNumerica.Formatar(valor) + " " + unidade).Trim();
            string limiteTexto = (EntradaNumerica.Formatar(limite) + " " + unidade).Trim();

            return TiposSensor.Nome(sensor.Tipo) + " em " + valorTexto
                 + ": atingiu o limite " + nomeLimite + " (" + limiteTexto + ").";
        }
    }

    // Gera leituras de teste enquanto não existem sensores reais.
    // Liga/desliga em appsettings.json (seção "Simulador").
    // Cada sensor oscila em torno de um valor normal e, de vez em quando, entra num
    // "episódio de falha": o valor sobe além do limite de atenção (ou crítico) por alguns
    // ciclos e depois volta ao normal. Assim aparecem alertas de verdade.
    public class SimuladorLeituras : BackgroundService
    {
        private readonly IServiceScopeFactory _escopos;
        private readonly IConfiguration _config;
        private readonly ILogger<SimuladorLeituras> _logger;
        private readonly Random _aleatorio = new Random();
        private readonly double _chanceFalha;

        // Situação de cada sensor entre uma leitura e a seguinte
        private readonly Dictionary<int, EstadoSensor> _estados = new();

        private class EstadoSensor
        {
            public double Valor;
            public int CiclosDeFalha;
            public double AlvoDaFalha;
        }

        public SimuladorLeituras(IServiceScopeFactory escopos, IConfiguration config, ILogger<SimuladorLeituras> logger)
        {
            _escopos = escopos;
            _config = config;
            _logger = logger;
            _chanceFalha = Math.Clamp(config.GetValue("Simulador:ChanceFalha", 0.03), 0.0, 1.0);
        }

        protected override async Task ExecuteAsync(CancellationToken parada)
        {
            if (!_config.GetValue<bool>("Simulador:Ativo"))
            {
                return;
            }

            int segundos = Math.Max(1, _config.GetValue("Simulador:IntervaloSegundos", 10));

            _logger.LogInformation("Simulador de leituras ligado (uma rodada a cada {Segundos} s).", segundos);

            try
            {
                // Dá tempo da aplicação e do banco ficarem prontos
                await Task.Delay(TimeSpan.FromSeconds(5), parada);

                using PeriodicTimer relogio = new PeriodicTimer(TimeSpan.FromSeconds(segundos));

                do
                {
                    GerarLeituras();
                }
                while (await relogio.WaitForNextTickAsync(parada));
            }
            catch (OperationCanceledException)
            {
                // Aplicação encerrando
            }
        }

        private void GerarLeituras()
        {
            try
            {
                using IServiceScope escopo = _escopos.CreateScope();

                List<Sensor> sensores = escopo.ServiceProvider.GetRequiredService<Sensores>().ListarAtivos();
                ProcessadorLeituras processador = escopo.ServiceProvider.GetRequiredService<ProcessadorLeituras>();

                foreach (Sensor sensor in sensores)
                {
                    decimal valor = ProximoValor(sensor);

                    Alerta? alerta = processador.Registrar(sensor, valor, DateTime.Now);

                    if (alerta != null)
                    {
                        _logger.LogInformation("Alerta {Nivel} criado para o sensor {IdSensor}.", alerta.Nivel, sensor.IdSensor);
                    }
                }
            }
            catch (Exception ex)
            {
                // Banco fora do ar, por exemplo: registra e tenta de novo na próxima rodada
                _logger.LogWarning("Falha ao gerar leituras simuladas: {Mensagem}", ex.Message);
            }
        }

        private decimal ProximoValor(Sensor sensor)
        {
            // Referências do sensor: "topo" é onde o valor vira crítico (ou o máximo da faixa)
            double topo = (double)(sensor.LimiteCritico ?? sensor.LimiteAtencao ?? sensor.ValorMaximo ?? 100m);
            if (topo <= 0)
            {
                topo = 100;
            }

            double inicioAtencao = sensor.LimiteAtencao.HasValue ? (double)sensor.LimiteAtencao.Value : topo * 0.85;
            double normal = topo * 0.55;

            if (!_estados.TryGetValue(sensor.IdSensor, out EstadoSensor? estado))
            {
                estado = new EstadoSensor { Valor = normal };
                _estados[sensor.IdSensor] = estado;
            }

            // De vez em quando começa um episódio de falha (dura de 4 a 9 rodadas)
            if (estado.CiclosDeFalha == 0 && _aleatorio.NextDouble() < _chanceFalha)
            {
                estado.CiclosDeFalha = _aleatorio.Next(4, 10);
                estado.AlvoDaFalha = inicioAtencao + _aleatorio.NextDouble() * (topo * 1.1 - inicioAtencao);
            }

            double alvo = estado.CiclosDeFalha > 0 ? estado.AlvoDaFalha : normal;

            if (estado.CiclosDeFalha > 0)
            {
                estado.CiclosDeFalha--;
            }

            // Aproxima-se do alvo e acrescenta um pequeno ruído (±3% do topo)
            double ruido = (_aleatorio.NextDouble() - 0.5) * 0.06 * topo;
            estado.Valor = estado.Valor + (alvo - estado.Valor) * 0.35 + ruido;

            // Respeita a faixa de medição do sensor, se ele tiver uma
            if (sensor.ValorMinimo.HasValue)
            {
                estado.Valor = Math.Max(estado.Valor, (double)sensor.ValorMinimo.Value);
            }

            if (sensor.ValorMaximo.HasValue)
            {
                estado.Valor = Math.Min(estado.Valor, (double)sensor.ValorMaximo.Value);
            }

            return Math.Round((decimal)estado.Valor, 2);
        }
    }
}
