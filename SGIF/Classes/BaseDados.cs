using System.Data;
using System.Globalization;
using Microsoft.Data.SqlClient;
using SGIF.Models;

namespace SGIF.Classes
{
    // Código comum a todas as classes de acesso a dados: conexão e execução de comandos.
    public abstract class BaseDados
    {
        private readonly string _stringConexao;

        protected BaseDados(IConfiguration configuration)
        {
            _stringConexao = configuration.GetConnectionString("StringConexaoSQLServer")
                ?? throw new InvalidOperationException(
                    "A connection string 'StringConexaoSQLServer' não está configurada.");
        }

        protected SqlConnection CriarConexao()
        {
            return new SqlConnection(_stringConexao);
        }

        // Converte null do C# em DBNull do SQL
        protected static object ValorOuNulo(object? valor)
        {
            return valor ?? DBNull.Value;
        }

        // SELECT que devolve uma lista de objetos
        protected List<T> Consultar<T>(string sql, Func<SqlDataReader, T> mapear,
                                       Action<SqlParameterCollection>? parametros = null)
        {
            List<T> lista = new List<T>();

            using var conn = CriarConexao();
            using var cmd = new SqlCommand(sql, conn);
            parametros?.Invoke(cmd.Parameters);

            conn.Open();
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                lista.Add(mapear(r));
            }
            return lista;
        }

        // SELECT que devolve um único objeto (ou null)
        protected T? ConsultarUm<T>(string sql, Func<SqlDataReader, T> mapear,
                                    Action<SqlParameterCollection>? parametros = null) where T : class
        {
            return Consultar(sql, mapear, parametros).FirstOrDefault();
        }

        // INSERT com OUTPUT INSERTED.id: devolve o id gerado
        protected int ExecutarEscalar(string sql, Action<SqlParameterCollection> parametros)
        {
            using var conn = CriarConexao();
            using var cmd = new SqlCommand(sql, conn);
            parametros(cmd.Parameters);

            conn.Open();
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        // UPDATE / DELETE: true se alguma linha foi afetada
        protected bool Executar(string sql, Action<SqlParameterCollection> parametros)
        {
            using var conn = CriarConexao();
            using var cmd = new SqlCommand(sql, conn);
            parametros(cmd.Parameters);

            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }
    }

    // Leitura de colunas que aceitam NULL
    public static class LeitorExtensoes
    {
        public static int? GetIntNulo(this SqlDataReader r, string coluna)
        {
            int i = r.GetOrdinal(coluna);
            return r.IsDBNull(i) ? null : r.GetInt32(i);
        }

        public static decimal? GetDecimalNulo(this SqlDataReader r, string coluna)
        {
            int i = r.GetOrdinal(coluna);
            return r.IsDBNull(i) ? null : r.GetDecimal(i);
        }

        public static string? GetStringNulo(this SqlDataReader r, string coluna)
        {
            int i = r.GetOrdinal(coluna);
            return r.IsDBNull(i) ? null : r.GetString(i);
        }

        public static DateTime? GetDateTimeNulo(this SqlDataReader r, string coluna)
        {
            int i = r.GetOrdinal(coluna);
            return r.IsDBNull(i) ? null : r.GetDateTime(i);
        }
    }

    // Pedaço de SQL e parâmetros compartilhados pelas consultas filtradas (RF13).
    // Exige que a consulta tenha a tabela Sensores com o alias "s".
    public static class FiltroSql
    {
        public const string Condicao = @"
            AND (@tipo_sensor IS NULL OR s.tipo = @tipo_sensor)
            AND (@tipo_ativo IS NULL
                 OR (@tipo_ativo = 'Locomotiva' AND s.id_locomotiva IS NOT NULL
                     AND (@id_ativo IS NULL OR s.id_locomotiva = @id_ativo))
                 OR (@tipo_ativo = 'Vagao' AND s.id_vagao IS NOT NULL
                     AND (@id_ativo IS NULL OR s.id_vagao = @id_ativo)))";

        public static void Parametros(SqlParameterCollection p, FiltroAnalise f)
        {
            p.Add("@tipo_sensor", SqlDbType.VarChar, 50).Value = (object?)f.TipoSensor ?? DBNull.Value;
            p.Add("@tipo_ativo", SqlDbType.VarChar, 20).Value = (object?)f.TipoAtivo ?? DBNull.Value;
            p.Add("@id_ativo", SqlDbType.Int).Value = (object?)f.IdAtivo ?? DBNull.Value;
        }
    }

    // Leitura e exibição de números digitados em formulários (aceita vírgula ou ponto).
    public static class EntradaNumerica
    {
        private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

        // Vazio = sem valor (válido, devolve null). Respeita o limite do DECIMAL(10,2) do banco.
        public static bool TentarLerDecimal(string? texto, out decimal? valor)
        {
            valor = null;

            if (string.IsNullOrWhiteSpace(texto))
            {
                return true;
            }

            string normalizado = texto.Trim().Replace(',', '.');

            if (decimal.TryParse(normalizado, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign,
                                 CultureInfo.InvariantCulture, out decimal d)
                && d >= -99999999.99m && d <= 99999999.99m)
            {
                valor = Math.Round(d, 2);
                return true;
            }

            return false;
        }

        // 12.5 vira "12,5"; null vira texto vazio
        public static string Formatar(decimal? valor)
        {
            return valor.HasValue ? valor.Value.ToString("0.##", PtBr) : "";
        }
    }
}
