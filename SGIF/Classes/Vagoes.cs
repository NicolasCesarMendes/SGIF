using System.Data;
using Microsoft.Data.SqlClient;

namespace SGIF.Classes
{
    public class Vagoes
    {
        //-----------------------------
        // Atributos
        //-----------------------------
        private int _idVagao;
        private string _numeroIdentificacao = string.Empty;
        private string _tipo = string.Empty;
        private decimal? _capacidade;
        private string _status = "Ativo";
        private int? _idTrem;

        SqlConnection con;

        //-----------------------------
        // Getters e Setters
        //-----------------------------
        public int IdVagao { get { return _idVagao; } set { _idVagao = value; } }
        public string NumeroIdentificacao { get { return _numeroIdentificacao; } set { _numeroIdentificacao = value; } }
        public string Tipo { get { return _tipo; } set { _tipo = value; } }
        public decimal? Capacidade { get { return _capacidade; } set { _capacidade = value; } }
        public string Status { get { return _status; } set { _status = value; } }
        public int? IdTrem { get { return _idTrem; } set { _idTrem = value; } }

        //-----------------------------
        // Construtor
        //-----------------------------
        public Vagoes()
        {
            try
            {
                IConfigurationRoot o_Config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile(@".\Configuration\SGIFBD.json")
                    .Build();

                string strConexao = o_Config.GetConnectionString(@"StringConexaoSQLServer");

                con = new SqlConnection(strConexao);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        //-----------------------------
        // Inserir (preenche IdVagao com o id gerado)
        //-----------------------------
        public void Inserir()
        {
            try
            {
                string cmdSQL = @"
                    INSERT INTO Vagoes
                    (
                        numero_identificacao, tipo, capacidade, status, id_trem
                    )
                    OUTPUT INSERTED.id_vagao
                    VALUES
                    (
                        @numero_identificacao, @tipo, @capacidade, @status, @id_trem
                    )";

                SqlCommand cmd = new SqlCommand(cmdSQL, con);

                cmd.Parameters.AddWithValue("@numero_identificacao", NumeroIdentificacao);
                cmd.Parameters.AddWithValue("@tipo", Tipo);
                cmd.Parameters.AddWithValue("@capacidade", (object?)Capacidade ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@status", Status);
                cmd.Parameters.AddWithValue("@id_trem", (object?)IdTrem ?? DBNull.Value);

                con.Open();
                IdVagao = Convert.ToInt32(cmd.ExecuteScalar());
            }
            finally
            {
                con.Close();
            }
        }

        //-----------------------------
        // Alterar
        //-----------------------------
        public void Alterar()
        {
            try
            {
                string cmdSQL = @"
                    UPDATE Vagoes SET
                        numero_identificacao = @numero_identificacao,
                        tipo = @tipo,
                        capacidade = @capacidade,
                        status = @status,
                        id_trem = @id_trem
                    WHERE id_vagao = @id_vagao";

                SqlCommand cmd = new SqlCommand(cmdSQL, con);

                cmd.Parameters.AddWithValue("@id_vagao", IdVagao);
                cmd.Parameters.AddWithValue("@numero_identificacao", NumeroIdentificacao);
                cmd.Parameters.AddWithValue("@tipo", Tipo);
                cmd.Parameters.AddWithValue("@capacidade", (object?)Capacidade ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@status", Status);
                cmd.Parameters.AddWithValue("@id_trem", (object?)IdTrem ?? DBNull.Value);

                con.Open();
                cmd.ExecuteNonQuery();
            }
            finally
            {
                con.Close();
            }
        }

        //-----------------------------
        // Excluir (erro 547 se houver sensores associados)
        //-----------------------------
        public void Excluir()
        {
            try
            {
                string cmdSQL = "DELETE FROM Vagoes WHERE id_vagao = @id_vagao";

                SqlCommand cmd = new SqlCommand(cmdSQL, con);

                cmd.Parameters.AddWithValue("@id_vagao", IdVagao);

                con.Open();
                cmd.ExecuteNonQuery();
            }
            finally
            {
                con.Close();
            }
        }

        //-----------------------------
        // Selecionar
        //-----------------------------
        public DataTable? Selecionar()
        {
            try
            {
                string cmdSQL = "SELECT * FROM Vagoes ORDER BY numero_identificacao";

                SqlDataAdapter da = new SqlDataAdapter(cmdSQL, con);

                DataTable dt = new DataTable();

                da.Fill(dt);

                return dt.Rows.Count > 0 ? dt : null;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        //-----------------------------
        // Buscar por ID
        //-----------------------------
        public DataTable? BuscarPorId(int id)
        {
            try
            {
                string cmdSQL = @"
                    SELECT *
                    FROM Vagoes
                    WHERE id_vagao = @id_vagao";

                SqlCommand cmd = new SqlCommand(cmdSQL, con);

                cmd.Parameters.AddWithValue("@id_vagao", id);

                SqlDataAdapter da = new SqlDataAdapter(cmd);

                DataTable dt = new DataTable();

                da.Fill(dt);

                return dt.Rows.Count > 0 ? dt : null;
            }
            finally
            {
                con.Close();
            }
        }

        //-----------------------------
        // Buscar por texto (número ou tipo) e/ou status
        // Parâmetros nulos ou vazios são ignorados
        //-----------------------------
        public DataTable? Buscar(string? termo, string? status)
        {
            try
            {
                string cmdSQL = @"
                    SELECT *
                    FROM Vagoes
                    WHERE (@termo IS NULL OR numero_identificacao LIKE @termo OR tipo LIKE @termo)
                      AND (@status IS NULL OR status = @status)
                    ORDER BY numero_identificacao";

                SqlCommand cmd = new SqlCommand(cmdSQL, con);

                cmd.Parameters.Add("@termo", SqlDbType.VarChar, 200).Value =
                    string.IsNullOrWhiteSpace(termo) ? (object)DBNull.Value : "%" + termo.Trim() + "%";
                cmd.Parameters.Add("@status", SqlDbType.VarChar, 50).Value =
                    string.IsNullOrWhiteSpace(status) ? (object)DBNull.Value : status;

                SqlDataAdapter da = new SqlDataAdapter(cmd);

                DataTable dt = new DataTable();

                da.Fill(dt);

                return dt.Rows.Count > 0 ? dt : null;
            }
            finally
            {
                con.Close();
            }
        }
    }
}
