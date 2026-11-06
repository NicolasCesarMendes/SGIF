using System.Data;
using Microsoft.Data.SqlClient;
using SGIF.Models;

namespace SGIF.Classes
{
    public class Alertas : BaseDados
    {
        public Alertas(IConfiguration configuration) : base(configuration)
        {
        }

        public int Inserir(Alerta alerta)
        {
            const string sql = @"
                INSERT INTO Alertas (id_leitura, nivel, descricao, data_hora, status)
                OUTPUT INSERTED.id_alerta
                VALUES (@id_leitura, @nivel, @descricao, @data_hora, @status)";

            alerta.IdAlerta = ExecutarEscalar(sql, p =>
            {
                p.AddWithValue("@id_leitura", alerta.IdLeitura);
                p.AddWithValue("@nivel", alerta.Nivel);
                p.AddWithValue("@descricao", ValorOuNulo(alerta.Descricao));
                p.Add("@data_hora", SqlDbType.DateTime2).Value = alerta.DataHora;
                p.AddWithValue("@status", alerta.Status);
            });
            return alerta.IdAlerta;
        }

        public bool AlterarStatus(int idAlerta, string status, int idUsuario)
        {
            // Aberto: sem responsável. Em analise/Resolvido: quem mexeu. Só Resolvido tem data.
            const string sql = @"
            UPDATE Alertas SET
                status = @status,
                id_usuario_responsavel = CASE WHEN @status = 'Aberto' THEN NULL ELSE @id_usuario END,
                data_resolucao = CASE WHEN @status = 'Resolvido' THEN @agora ELSE NULL END
            WHERE id_alerta = @id_alerta";

            return Executar(sql, p =>
            {
                p.AddWithValue("@id_alerta", idAlerta);
                p.AddWithValue("@status", status);
                p.AddWithValue("@id_usuario", idUsuario);
                p.Add("@agora", SqlDbType.DateTime2).Value = DateTime.Now;
            });
        }

        public bool Excluir(int idAlerta)
        {
            const string sql = "DELETE FROM Alertas WHERE id_alerta = @id_alerta";

            return Executar(sql, p => p.AddWithValue("@id_alerta", idAlerta));
        }

        public Alerta? BuscarPorId(int idAlerta)
        {
            const string sql = "SELECT * FROM Alertas WHERE id_alerta = @id_alerta";

            return ConsultarUm(sql, Mapear, p => p.AddWithValue("@id_alerta", idAlerta));
        }

        // Fila de alertas: críticos primeiro, depois os mais recentes. status nulo = todos.
        public List<Alerta> ListarPorStatus(string? status)
        {
            const string sql = @"
                SELECT * FROM Alertas
                WHERE (@status IS NULL OR status = @status)
                ORDER BY CASE nivel WHEN 'Critico' THEN 0 ELSE 1 END, data_hora DESC";

            return Consultar(sql, Mapear, p =>
            {
                p.Add("@status", SqlDbType.VarChar, 50).Value =
                    string.IsNullOrWhiteSpace(status) ? (object)DBNull.Value : status;
            });
        }

        // O sensor já tem um alerta deste nível que ainda não foi resolvido?
        public bool ExisteNaoResolvidoParaSensor(int idSensor, string nivel)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Alertas a
                JOIN Leituras l ON l.id_leitura = a.id_leitura
                WHERE l.id_sensor = @id_sensor
                  AND a.nivel = @nivel
                  AND a.status <> 'Resolvido'";

            return ExecutarEscalar(sql, p =>
            {
                p.AddWithValue("@id_sensor", idSensor);
                p.AddWithValue("@nivel", nivel);
            }) > 0;
        }

        private static Alerta Mapear(SqlDataReader r)
        {
            return new Alerta
            {
                IdAlerta = r.GetInt32(r.GetOrdinal("id_alerta")),
                IdLeitura = r.GetInt32(r.GetOrdinal("id_leitura")),
                Nivel = r.GetString(r.GetOrdinal("nivel")),
                Descricao = r.GetStringNulo("descricao"),
                DataHora = r.GetDateTime(r.GetOrdinal("data_hora")),
                Status = r.GetString(r.GetOrdinal("status")),
                IdUsuarioResponsavel = r.GetIntNulo("id_usuario_responsavel"),
                DataResolucao = r.GetDateTimeNulo("data_resolucao")
            };
        }
    }
}
