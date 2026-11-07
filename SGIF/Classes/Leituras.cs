using System.Data;
using Microsoft.Data.SqlClient;
using SGIF.Models;

namespace SGIF.Classes
{
    public class Leituras : BaseDados
    {
        public Leituras(IConfiguration configuration) : base(configuration)
        {
        }

        public int Inserir(Leitura leitura)
        {
            const string sql = @"
                INSERT INTO Leituras (id_sensor, valor, data_hora)
                OUTPUT INSERTED.id_leitura
                VALUES (@id_sensor, @valor, @data_hora)";

            leitura.IdLeitura = ExecutarEscalar(sql, p =>
            {
                p.AddWithValue("@id_sensor", leitura.IdSensor);
                p.AddWithValue("@valor", leitura.Valor);
                p.Add("@data_hora", SqlDbType.DateTime2).Value = leitura.DataHora;
            });
            return leitura.IdLeitura;
        }

        public Leitura? BuscarPorId(int idLeitura)
        {
            const string sql = "SELECT * FROM Leituras WHERE id_leitura = @id_leitura";

            return ConsultarUm(sql, Mapear, p => p.AddWithValue("@id_leitura", idLeitura));
        }

        // Histórico de um sensor, com período opcional
        public List<Leitura> ListarPorSensor(int idSensor, DateTime? inicio = null, DateTime? fim = null)
        {
            const string sql = @"
                SELECT * FROM Leituras
                WHERE id_sensor = @id_sensor
                  AND (@inicio IS NULL OR data_hora >= @inicio)
                  AND (@fim IS NULL OR data_hora <= @fim)
                ORDER BY data_hora";

            return Consultar(sql, Mapear, p =>
            {
                p.AddWithValue("@id_sensor", idSensor);
                p.Add("@inicio", SqlDbType.DateTime2).Value = ValorOuNulo(inicio);
                p.Add("@fim", SqlDbType.DateTime2).Value = ValorOuNulo(fim);
            });
        }

        public Leitura? BuscarUltimaPorSensor(int idSensor)
        {
            const string sql = @"
                SELECT TOP 1 * FROM Leituras
                WHERE id_sensor = @id_sensor
                ORDER BY data_hora DESC";

            return ConsultarUm(sql, Mapear, p => p.AddWithValue("@id_sensor", idSensor));
        }

        // As últimas leituras de um sensor (mais recentes primeiro), a partir de uma data opcional
        public List<Leitura> ListarRecentesPorSensor(int idSensor, int quantidade, DateTime? inicio = null)
        {
            const string sql = @"
                SELECT TOP (@quantidade) * FROM Leituras
                WHERE id_sensor = @id_sensor
                  AND (@inicio IS NULL OR data_hora >= @inicio)
                ORDER BY data_hora DESC, id_leitura DESC";

            return Consultar(sql, Mapear, p =>
            {
                p.AddWithValue("@id_sensor", idSensor);
                p.AddWithValue("@quantidade", quantidade);
                p.Add("@inicio", SqlDbType.DateTime2).Value = ValorOuNulo(inicio);
            });
        }

        // Quantidade, mínimo, máximo e média das leituras de um sensor a partir de uma data
        public LeituraResumo ResumirPorSensor(int idSensor, DateTime inicio)
        {
            const string sql = @"
                SELECT COUNT(*) AS quantidade, MIN(valor) AS minimo, MAX(valor) AS maximo, AVG(valor) AS media,
                       CAST(STDEV(valor) AS decimal(18,4)) AS desvio
                FROM Leituras
                WHERE id_sensor = @id_sensor AND data_hora >= @inicio";

            return Consultar(sql, r => new LeituraResumo
            {
                Quantidade = r.GetInt32(r.GetOrdinal("quantidade")),
                Minimo = r.GetDecimalNulo("minimo"),
                Maximo = r.GetDecimalNulo("maximo"),
                Media = r.GetDecimalNulo("media"),
                Desvio = r.GetDecimalNulo("desvio")
            }, p =>
            {
                p.AddWithValue("@id_sensor", idSensor);
                p.Add("@inicio", SqlDbType.DateTime2).Value = inicio;
            }).First();
        }

        // ---------- Análise (séries para gráficos, anomalias, relatórios) ----------

        // Início do intervalo de N minutos em que cada leitura cai (a conta é feita no banco)
        private const string IntervaloSql = @"
            CROSS APPLY (SELECT DATEADD(MINUTE,
                            (DATEDIFF(MINUTE, CONVERT(datetime2, '2000-01-01'), l.data_hora) / @minutos) * @minutos,
                            CONVERT(datetime2, '2000-01-01')) AS inicio) b";

        // Leituras de um sensor agrupadas em intervalos (média, mínimo, máximo e quantidade).
        // Mantém a quantidade de pontos pequena mesmo com muitas leituras (RNF3).
        public List<PontoSerie> SerieAgregada(int idSensor, DateTime inicio, int minutosPorPonto)
        {
            const string sql = @"
                SELECT b.inicio, AVG(l.valor) AS media, MIN(l.valor) AS minimo, MAX(l.valor) AS maximo, COUNT(*) AS quantidade
                FROM Leituras l" + IntervaloSql + @"
                WHERE l.id_sensor = @id_sensor AND l.data_hora >= @inicio
                GROUP BY b.inicio
                ORDER BY b.inicio";

            return Consultar(sql, MapearPonto, p =>
            {
                p.AddWithValue("@id_sensor", idSensor);
                p.AddWithValue("@minutos", minutosPorPonto);
                p.Add("@inicio", SqlDbType.DateTime2).Value = inicio;
            });
        }

        // O mesmo, juntando todos os sensores de um tipo (gráficos do painel: frota inteira)
        public List<PontoSerie> SerieAgregadaPorTipo(string tipoSensor, DateTime inicio, int minutosPorPonto)
        {
            const string sql = @"
                SELECT b.inicio, AVG(l.valor) AS media, MIN(l.valor) AS minimo, MAX(l.valor) AS maximo, COUNT(*) AS quantidade
                FROM Leituras l
                INNER JOIN Sensores s ON s.id_sensor = l.id_sensor" + IntervaloSql + @"
                WHERE s.tipo = @tipo AND l.data_hora >= @inicio
                GROUP BY b.inicio
                ORDER BY b.inicio";

            return Consultar(sql, MapearPonto, p =>
            {
                p.Add("@tipo", SqlDbType.VarChar, 50).Value = tipoSensor;
                p.AddWithValue("@minutos", minutosPorPonto);
                p.Add("@inicio", SqlDbType.DateTime2).Value = inicio;
            });
        }

        // Quantas leituras do sensor ficaram fora da faixa [inferior, superior] no período
        public int ContarForaDaFaixa(int idSensor, DateTime inicio, decimal inferior, decimal superior)
        {
            const string sql = @"
                SELECT COUNT(*) FROM Leituras
                WHERE id_sensor = @id_sensor AND data_hora >= @inicio
                  AND (valor < @inferior OR valor > @superior)";

            return ExecutarEscalar(sql, p =>
            {
                p.AddWithValue("@id_sensor", idSensor);
                p.Add("@inicio", SqlDbType.DateTime2).Value = inicio;
                AdicionarDecimal(p, "@inferior", inferior);
                AdicionarDecimal(p, "@superior", superior);
            });
        }

        // As leituras fora da faixa mais recentes
        public List<Leitura> ListarForaDaFaixa(int idSensor, DateTime inicio, decimal inferior, decimal superior, int quantidade)
        {
            const string sql = @"
                SELECT TOP (@quantidade) * FROM Leituras
                WHERE id_sensor = @id_sensor AND data_hora >= @inicio
                  AND (valor < @inferior OR valor > @superior)
                ORDER BY data_hora DESC, id_leitura DESC";

            return Consultar(sql, Mapear, p =>
            {
                p.AddWithValue("@quantidade", quantidade);
                p.AddWithValue("@id_sensor", idSensor);
                p.Add("@inicio", SqlDbType.DateTime2).Value = inicio;
                AdicionarDecimal(p, "@inferior", inferior);
                AdicionarDecimal(p, "@superior", superior);
            });
        }

        // Estatísticas por tipo de sensor no período, respeitando os filtros (RF12, RF13)
        public List<EstatisticaTipo> EstatisticasPorTipo(FiltroAnalise filtro, DateTime inicio)
        {
            const string sql = @"
                SELECT s.tipo, COUNT(DISTINCT s.id_sensor) AS sensores, COUNT(*) AS quantidade,
                       MIN(l.valor) AS minimo, MAX(l.valor) AS maximo, AVG(l.valor) AS media,
                       CAST(STDEV(l.valor) AS decimal(18,4)) AS desvio
                FROM Leituras l
                INNER JOIN Sensores s ON s.id_sensor = l.id_sensor
                WHERE l.data_hora >= @inicio" + FiltroSql.Condicao + @"
                GROUP BY s.tipo
                ORDER BY s.tipo";

            return Consultar(sql, r => new EstatisticaTipo
            {
                TipoSensor = r.GetString(r.GetOrdinal("tipo")),
                Sensores = r.GetInt32(r.GetOrdinal("sensores")),
                Quantidade = r.GetInt32(r.GetOrdinal("quantidade")),
                Minimo = r.GetDecimalNulo("minimo"),
                Maximo = r.GetDecimalNulo("maximo"),
                Media = r.GetDecimalNulo("media"),
                Desvio = r.GetDecimalNulo("desvio")
            }, p =>
            {
                p.Add("@inicio", SqlDbType.DateTime2).Value = inicio;
                FiltroSql.Parametros(p, filtro);
            });
        }

        // Média, máximo e quantidade de leituras por mês e tipo de sensor (comparativo mensal)
        public List<ResumoMensal> ResumirPorMes(FiltroAnalise filtro, DateTime inicio)
        {
            const string sql = @"
                SELECT YEAR(l.data_hora) AS ano, MONTH(l.data_hora) AS mes, s.tipo,
                       COUNT(*) AS quantidade, AVG(l.valor) AS media, MAX(l.valor) AS maximo
                FROM Leituras l
                INNER JOIN Sensores s ON s.id_sensor = l.id_sensor
                WHERE l.data_hora >= @inicio" + FiltroSql.Condicao + @"
                GROUP BY YEAR(l.data_hora), MONTH(l.data_hora), s.tipo
                ORDER BY ano, mes, s.tipo";

            return Consultar(sql, r => new ResumoMensal
            {
                Ano = r.GetInt32(r.GetOrdinal("ano")),
                Mes = r.GetInt32(r.GetOrdinal("mes")),
                TipoSensor = r.GetString(r.GetOrdinal("tipo")),
                Quantidade = r.GetInt32(r.GetOrdinal("quantidade")),
                Media = r.GetDecimal(r.GetOrdinal("media")),
                Maximo = r.GetDecimal(r.GetOrdinal("maximo"))
            }, p =>
            {
                p.Add("@inicio", SqlDbType.DateTime2).Value = inicio;
                FiltroSql.Parametros(p, filtro);
            });
        }

        // Decimal com precisão fixa (evita que o driver deduza a escala pelo valor)
        private static void AdicionarDecimal(SqlParameterCollection p, string nome, decimal valor)
        {
            SqlParameter parametro = p.Add(nome, SqlDbType.Decimal);
            parametro.Precision = 18;
            parametro.Scale = 4;
            parametro.Value = Math.Round(valor, 4);
        }

        private static PontoSerie MapearPonto(SqlDataReader r)
        {
            return new PontoSerie
            {
                Inicio = r.GetDateTime(r.GetOrdinal("inicio")),
                Media = r.GetDecimal(r.GetOrdinal("media")),
                Minimo = r.GetDecimal(r.GetOrdinal("minimo")),
                Maximo = r.GetDecimal(r.GetOrdinal("maximo")),
                Quantidade = r.GetInt32(r.GetOrdinal("quantidade"))
            };
        }

        private static Leitura Mapear(SqlDataReader r)
        {
            return new Leitura
            {
                IdLeitura = r.GetInt32(r.GetOrdinal("id_leitura")),
                IdSensor = r.GetInt32(r.GetOrdinal("id_sensor")),
                Valor = r.GetDecimal(r.GetOrdinal("valor")),
                DataHora = r.GetDateTime(r.GetOrdinal("data_hora"))
            };
        }
    }
}
