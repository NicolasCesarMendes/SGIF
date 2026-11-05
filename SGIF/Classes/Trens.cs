using Microsoft.Data.SqlClient;
using SGIF.Models;

namespace SGIF.Classes
{
    public class Trens : BaseDados
    {
        public Trens(IConfiguration configuration) : base(configuration)
        {
        }

        public int Inserir(Trem trem)
        {
            const string sql = @"
                INSERT INTO Trens (identificacao)
                OUTPUT INSERTED.id_trem
                VALUES (@identificacao)";

            trem.IdTrem = ExecutarEscalar(sql, p =>
            {
                p.AddWithValue("@identificacao", trem.Identificacao);
            });
            return trem.IdTrem;
        }

        public bool Atualizar(Trem trem)
        {
            const string sql = "UPDATE Trens SET identificacao = @identificacao WHERE id_trem = @id_trem";

            return Executar(sql, p =>
            {
                p.AddWithValue("@id_trem", trem.IdTrem);
                p.AddWithValue("@identificacao", trem.Identificacao);
            });
        }

        public bool Excluir(int idTrem)
        {
            const string sql = "DELETE FROM Trens WHERE id_trem = @id_trem";

            return Executar(sql, p => p.AddWithValue("@id_trem", idTrem));
        }

        public Trem? BuscarPorId(int idTrem)
        {
            const string sql = "SELECT * FROM Trens WHERE id_trem = @id_trem";

            return ConsultarUm(sql, Mapear, p => p.AddWithValue("@id_trem", idTrem));
        }

        public List<Trem> ListarTodos()
        {
            const string sql = "SELECT * FROM Trens ORDER BY identificacao";

            return Consultar(sql, Mapear);
        }

        private static Trem Mapear(SqlDataReader r)
        {
            return new Trem
            {
                IdTrem = r.GetInt32(r.GetOrdinal("id_trem")),
                Identificacao = r.GetString(r.GetOrdinal("identificacao"))
            };
        }
    }
}
