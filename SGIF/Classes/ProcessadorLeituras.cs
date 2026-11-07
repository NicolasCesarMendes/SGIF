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
}
