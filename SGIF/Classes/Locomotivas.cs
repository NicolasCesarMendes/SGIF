using System.Data;
using Microsoft.Data.SqlClient;
using SGIF.Models;

namespace SGIF.Classes
{
    public class Locomotivas : BaseDados
    {
        public Locomotivas(IConfiguration configuration) : base(configuration)
        {
        }

        public int Inserir(Locomotiva locomotiva)
        {
            const string sql = @"
                INSERT INTO Locomotivas
                    (numero_identificacao, modelo, fabricante, ano_fabricacao, status, id_trem)
                OUTPUT INSERTED.id_locomotiva
                VALUES
                    (@numero_identificacao, @modelo, @fabricante, @ano_fabricacao, @status, @id_trem)";

            locomotiva.IdLocomotiva = ExecutarEscalar(sql, p => AdicionarParametros(p, locomotiva));
            return locomotiva.IdLocomotiva;
        }

        public bool Atualizar(Locomotiva locomotiva)
        {
            const string sql = @"
                UPDATE Locomotivas SET
                    numero_identificacao = @numero_identificacao,
                    modelo = @modelo,
                    fabricante = @fabricante,
                    ano_fabricacao = @ano_fabricacao,
                    status = @status,
                    id_trem = @id_trem
                WHERE id_locomotiva = @id_locomotiva";

            return Executar(sql, p =>
            {
                p.AddWithValue("@id_locomotiva", locomotiva.IdLocomotiva);
                AdicionarParametros(p, locomotiva);
            });
        }

        public bool Excluir(int idLocomotiva)
        {
            const string sql = "DELETE FROM Locomotivas WHERE id_locomotiva = @id_locomotiva";

            return Executar(sql, p => p.AddWithValue("@id_locomotiva", idLocomotiva));
        }

        public Locomotiva? BuscarPorId(int idLocomotiva)
        {
            const string sql = "SELECT * FROM Locomotivas WHERE id_locomotiva = @id_locomotiva";

            return ConsultarUm(sql, Mapear, p => p.AddWithValue("@id_locomotiva", idLocomotiva));
        }

        public List<Locomotiva> ListarTodos()
        {
            return Buscar(null, null);
        }

        // Busca por texto (número ou modelo) e/ou status. Parâmetros nulos ou vazios são ignorados.
        public List<Locomotiva> Buscar(string? termo, string? status)
        {
            const string sql = @"
                SELECT * FROM Locomotivas
                WHERE (@termo IS NULL OR numero_identificacao LIKE @termo OR modelo LIKE @termo)
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

        private void AdicionarParametros(SqlParameterCollection p, Locomotiva l)
        {
            p.AddWithValue("@numero_identificacao", l.NumeroIdentificacao);
            p.AddWithValue("@modelo", l.Modelo);
            p.AddWithValue("@fabricante", ValorOuNulo(l.Fabricante));
            p.AddWithValue("@ano_fabricacao", ValorOuNulo(l.AnoFabricacao));
            p.AddWithValue("@status", l.Status);
            p.AddWithValue("@id_trem", ValorOuNulo(l.IdTrem));
        }

        private static Locomotiva Mapear(SqlDataReader r)
        {
            return new Locomotiva
            {
                IdLocomotiva = r.GetInt32(r.GetOrdinal("id_locomotiva")),
                NumeroIdentificacao = r.GetString(r.GetOrdinal("numero_identificacao")),
                Modelo = r.GetString(r.GetOrdinal("modelo")),
                Fabricante = r.GetStringNulo("fabricante"),
                AnoFabricacao = r.GetIntNulo("ano_fabricacao"),
                Status = r.GetString(r.GetOrdinal("status")),
                IdTrem = r.GetIntNulo("id_trem")
            };
        }
    }
}
