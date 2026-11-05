using System.Data;
using Microsoft.Data.SqlClient;
using SGIF.Models;

namespace SGIF.Classes
{
    public class Vagoes : BaseDados
    {
        public Vagoes(IConfiguration configuration) : base(configuration)
        {
        }

        public int Inserir(Vagao vagao)
        {
            const string sql = @"
                INSERT INTO Vagoes
                    (numero_identificacao, tipo, capacidade, status, id_trem)
                OUTPUT INSERTED.id_vagao
                VALUES
                    (@numero_identificacao, @tipo, @capacidade, @status, @id_trem)";

            vagao.IdVagao = ExecutarEscalar(sql, p => AdicionarParametros(p, vagao));
            return vagao.IdVagao;
        }

        public bool Atualizar(Vagao vagao)
        {
            const string sql = @"
                UPDATE Vagoes SET
                    numero_identificacao = @numero_identificacao,
                    tipo = @tipo,
                    capacidade = @capacidade,
                    status = @status,
                    id_trem = @id_trem
                WHERE id_vagao = @id_vagao";

            return Executar(sql, p =>
            {
                p.AddWithValue("@id_vagao", vagao.IdVagao);
                AdicionarParametros(p, vagao);
            });
        }

        public bool Excluir(int idVagao)
        {
            const string sql = "DELETE FROM Vagoes WHERE id_vagao = @id_vagao";

            return Executar(sql, p => p.AddWithValue("@id_vagao", idVagao));
        }

        public Vagao? BuscarPorId(int idVagao)
        {
            const string sql = "SELECT * FROM Vagoes WHERE id_vagao = @id_vagao";

            return ConsultarUm(sql, Mapear, p => p.AddWithValue("@id_vagao", idVagao));
        }

        public List<Vagao> ListarTodos()
        {
            return Buscar(null, null);
        }

        // Busca por texto (número ou tipo) e/ou status. Parâmetros nulos ou vazios são ignorados.
        public List<Vagao> Buscar(string? termo, string? status)
        {
            const string sql = @"
                SELECT * FROM Vagoes
                WHERE (@termo IS NULL OR numero_identificacao LIKE @termo OR tipo LIKE @termo)
                  AND (@status IS NULL OR status = @status)
                ORDER BY numero_identificacao";

            return Consultar(sql, Mapear, p =>
            {
                p.Add("@termo", SqlDbType.VarChar, 200).Value =
                    string.IsNullOrWhiteSpace(termo) ? (object)DBNull.Value : "%" + termo.Trim() + "%";
                p.Add("@status", SqlDbType.VarChar, 50).Value =
                    string.IsNullOrWhiteSpace(status) ? (object)DBNull.Value : status;
            });
        }

        private void AdicionarParametros(SqlParameterCollection p, Vagao v)
        {
            p.AddWithValue("@numero_identificacao", v.NumeroIdentificacao);
            p.AddWithValue("@tipo", v.Tipo);
            p.AddWithValue("@capacidade", ValorOuNulo(v.Capacidade));
            p.AddWithValue("@status", v.Status);
            p.AddWithValue("@id_trem", ValorOuNulo(v.IdTrem));
        }

        private static Vagao Mapear(SqlDataReader r)
        {
            return new Vagao
            {
                IdVagao = r.GetInt32(r.GetOrdinal("id_vagao")),
                NumeroIdentificacao = r.GetString(r.GetOrdinal("numero_identificacao")),
                Tipo = r.GetString(r.GetOrdinal("tipo")),
                Capacidade = r.GetDecimalNulo("capacidade"),
                Status = r.GetString(r.GetOrdinal("status")),
                IdTrem = r.GetIntNulo("id_trem")
            };
        }
    }
}
