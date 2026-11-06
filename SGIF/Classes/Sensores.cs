using Microsoft.Data.SqlClient;
using SGIF.Models;

namespace SGIF.Classes
{
    public class Sensores : BaseDados
    {
        public Sensores(IConfiguration configuration) : base(configuration)
        {
        }

        public int Inserir(Sensor sensor)
        {
            const string sql = @"
                INSERT INTO Sensores
                    (tipo, modelo, valor_minimo, valor_maximo,
                     limite_atencao, limite_critico, id_locomotiva, id_vagao)
                OUTPUT INSERTED.id_sensor
                VALUES
                    (@tipo, @modelo, @valor_minimo, @valor_maximo,
                     @limite_atencao, @limite_critico, @id_locomotiva, @id_vagao)";

            sensor.IdSensor = ExecutarEscalar(sql, p => AdicionarParametros(p, sensor));
            return sensor.IdSensor;
        }

        public bool Atualizar(Sensor sensor)
        {
            const string sql = @"
                UPDATE Sensores SET
                    tipo = @tipo,
                    modelo = @modelo,
                    valor_minimo = @valor_minimo,
                    valor_maximo = @valor_maximo,
                    limite_atencao = @limite_atencao,
                    limite_critico = @limite_critico,
                    id_locomotiva = @id_locomotiva,
                    id_vagao = @id_vagao
                WHERE id_sensor = @id_sensor";

            return Executar(sql, p =>
            {
                p.AddWithValue("@id_sensor", sensor.IdSensor);
                AdicionarParametros(p, sensor);
            });
        }

        // Exclui o sensor junto com o histórico dele (alertas e leituras).
        // Tudo numa transação: se algum passo falhar, nada é apagado.
        public bool Excluir(int idSensor)
        {
            using var conn = CriarConexao();
            conn.Open();

            using var transacao = conn.BeginTransaction();

            try
            {
                // 1) alertas gerados pelas leituras do sensor (os alertas apontam para as leituras)
                ExecutarNaTransacao(conn, transacao, @"
                    DELETE FROM Alertas
                    WHERE id_leitura IN (SELECT id_leitura FROM Leituras WHERE id_sensor = @id_sensor)", idSensor);

                // 2) leituras do sensor
                ExecutarNaTransacao(conn, transacao, "DELETE FROM Leituras WHERE id_sensor = @id_sensor", idSensor);

                // 3) o próprio sensor
                int excluidos = ExecutarNaTransacao(conn, transacao, "DELETE FROM Sensores WHERE id_sensor = @id_sensor", idSensor);

                transacao.Commit();
                return excluidos > 0;
            }
            catch
            {
                transacao.Rollback();
                throw;
            }
        }

        private static int ExecutarNaTransacao(SqlConnection conn, SqlTransaction transacao, string sql, int idSensor)
        {
            using var cmd = new SqlCommand(sql, conn, transacao);
            cmd.Parameters.AddWithValue("@id_sensor", idSensor);
            return cmd.ExecuteNonQuery();
        }

        public Sensor? BuscarPorId(int idSensor)
        {
            const string sql = "SELECT * FROM Sensores WHERE id_sensor = @id_sensor";

            return ConsultarUm(sql, Mapear, p => p.AddWithValue("@id_sensor", idSensor));
        }

        public List<Sensor> ListarTodos()
        {
            const string sql = "SELECT * FROM Sensores ORDER BY id_sensor";

            return Consultar(sql, Mapear);
        }

        public List<Sensor> ListarPorLocomotiva(int idLocomotiva)
        {
            const string sql = "SELECT * FROM Sensores WHERE id_locomotiva = @id ORDER BY id_sensor";

            return Consultar(sql, Mapear, p => p.AddWithValue("@id", idLocomotiva));
        }

        public List<Sensor> ListarPorVagao(int idVagao)
        {
            const string sql = "SELECT * FROM Sensores WHERE id_vagao = @id ORDER BY id_sensor";

            return Consultar(sql, Mapear, p => p.AddWithValue("@id", idVagao));
        }

        private void AdicionarParametros(SqlParameterCollection p, Sensor s)
        {
            p.AddWithValue("@tipo", s.Tipo);
            p.AddWithValue("@modelo", s.Modelo);
            p.AddWithValue("@valor_minimo", ValorOuNulo(s.ValorMinimo));
            p.AddWithValue("@valor_maximo", ValorOuNulo(s.ValorMaximo));
            p.AddWithValue("@limite_atencao", ValorOuNulo(s.LimiteAtencao));
            p.AddWithValue("@limite_critico", ValorOuNulo(s.LimiteCritico));
            p.AddWithValue("@id_locomotiva", ValorOuNulo(s.IdLocomotiva));
            p.AddWithValue("@id_vagao", ValorOuNulo(s.IdVagao));
        }

        private static Sensor Mapear(SqlDataReader r)
        {
            return new Sensor
            {
                IdSensor = r.GetInt32(r.GetOrdinal("id_sensor")),
                Tipo = r.GetString(r.GetOrdinal("tipo")),
                Modelo = r.GetString(r.GetOrdinal("modelo")),
                ValorMinimo = r.GetDecimalNulo("valor_minimo"),
                ValorMaximo = r.GetDecimalNulo("valor_maximo"),
                LimiteAtencao = r.GetDecimalNulo("limite_atencao"),
                LimiteCritico = r.GetDecimalNulo("limite_critico"),
                IdLocomotiva = r.GetIntNulo("id_locomotiva"),
                IdVagao = r.GetIntNulo("id_vagao")
            };
        }
    }
}
