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
