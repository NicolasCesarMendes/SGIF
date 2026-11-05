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
