using System.Data;
using Microsoft.Data.SqlClient;

namespace SGIF.Classes
{
    public partial class Locomotivas
    {
        SqlConnection con;

        //-----------------------------
        // Construtor
        //-----------------------------
        public Locomotivas()
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
        // Inserir (preenche IdLocomotiva com o id gerado)
        //-----------------------------
        public void Inserir()
        {
            try
            {
                string cmdSQL = @"
                    INSERT INTO Locomotivas
                    (
                        numero_identificacao, modelo, fabricante, ano_fabricacao, status, id_trem
                    )
                    OUTPUT INSERTED.id_locomotiva
                    VALUES
                    (
                        @numero_identificacao, @modelo, @fabricante, @ano_fabricacao, @status, @id_trem
                    )";

                SqlCommand cmd = new SqlCommand(cmdSQL, con);

                cmd.Parameters.AddWithValue("@numero_identificacao", NumeroIdentificacao);
                cmd.Parameters.AddWithValue("@modelo", Modelo);
                cmd.Parameters.AddWithValue("@fabricante", (object?)Fabricante ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ano_fabricacao", (object?)AnoFabricacao ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@status", Status);
                cmd.Parameters.AddWithValue("@id_trem", (object?)IdTrem ?? DBNull.Value);

                con.Open();
                IdLocomotiva = Convert.ToInt32(cmd.ExecuteScalar());
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
                    UPDATE Locomotivas SET
                        numero_identificacao = @numero_identificacao,
                        modelo = @modelo,
                        fabricante = @fabricante,
                        ano_fabricacao = @ano_fabricacao,
                        status = @status,
                        id_trem = @id_trem
                    WHERE id_locomotiva = @id_locomotiva";

                SqlCommand cmd = new SqlCommand(cmdSQL, con);

                cmd.Parameters.AddWithValue("@id_locomotiva", IdLocomotiva);
                cmd.Parameters.AddWithValue("@numero_identificacao", NumeroIdentificacao);
                cmd.Parameters.AddWithValue("@modelo", Modelo);
                cmd.Parameters.AddWithValue("@fabricante", (object?)Fabricante ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ano_fabricacao", (object?)AnoFabricacao ?? DBNull.Value);
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
                string cmdSQL = "DELETE FROM Locomotivas WHERE id_locomotiva = @id_locomotiva";

                SqlCommand cmd = new SqlCommand(cmdSQL, con);

                cmd.Parameters.AddWithValue("@id_locomotiva", IdLocomotiva);

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
                string cmdSQL = "SELECT * FROM Locomotivas ORDER BY numero_identificacao";

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
                    FROM Locomotivas
                    WHERE id_locomotiva = @id_locomotiva";

                SqlCommand cmd = new SqlCommand(cmdSQL, con);

                cmd.Parameters.AddWithValue("@id_locomotiva", id);

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
        // Buscar por texto (número ou modelo) e/ou status
        // Parâmetros nulos ou vazios são ignorados
        //-----------------------------
        public DataTable? Buscar(string? termo, string? status)
        {
            try
            {
                string cmdSQL = @"
                    SELECT *
                    FROM Locomotivas
                    WHERE (@termo IS NULL OR numero_identificacao LIKE @termo OR modelo LIKE @termo)
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
