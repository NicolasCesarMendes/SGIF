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

        public bool AlterarStatus(int idAlerta, string status)
        {
            const string sql = "UPDATE Alertas SET status = @status WHERE id_alerta = @id_alerta";

            return Executar(sql, p =>
            {
                p.AddWithValue("@id_alerta", idAlerta);
                p.AddWithValue("@status", status);
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

        private static Alerta Mapear(SqlDataReader r)
        {
            return new Alerta
            {
                IdAlerta = r.GetInt32(r.GetOrdinal("id_alerta")),
                IdLeitura = r.GetInt32(r.GetOrdinal("id_leitura")),
                Nivel = r.GetString(r.GetOrdinal("nivel")),
                Descricao = r.GetStringNulo("descricao"),
                DataHora = r.GetDateTime(r.GetOrdinal("data_hora")),
                Status = r.GetString(r.GetOrdinal("status"))
            };
        }
    }
}
