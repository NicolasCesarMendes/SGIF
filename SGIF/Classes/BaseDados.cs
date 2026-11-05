using Microsoft.Data.SqlClient;

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
}
