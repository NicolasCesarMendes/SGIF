namespace SGIF.Classes
{
    public class Usuarios
    {
        
    }
}

/*

using System.Data;
using Microsoft.Data.SqlClient;

namespace ProjBancoDados.BancoDados
{
    public class Usuario
    {
        //-----------------------------
        // Atributos
        //-----------------------------
        public int idUsuario;
        public string nome;
        public string senha;
        public string email;
        public int nvAcesso;

        SqlConnection con;

        //-----------------------------
        // Construtor
        //-----------------------------
        public Usuario()
        {
            try
            {
                IConfigurationRoot o_Config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile(@".\Configuration\GRMPBD.json")
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
        // Inserir
        //-----------------------------
        public void Inserir()
        {
            try
            {
                string cmdSQL = @"INSERT INTO Usuario (Nome, Senha, Email, NvAcesso)
                                  VALUES (@Nome, @Senha, @Email, @NvAcesso)";

                SqlCommand cmd = new SqlCommand(cmdSQL, con);

                cmd.Parameters.AddWithValue("@Nome", nome);
                cmd.Parameters.AddWithValue("@Senha", senha);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@NvAcesso", nvAcesso);

                con.Open();
                cmd.ExecuteNonQuery();
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
                string cmdSQL = @"UPDATE Usuario SET
                                    Nome = @Nome,
                                    Senha = @Senha,
                                    Email = @Email,
                                    NvAcesso = @NvAcesso
                                  WHERE IdUsuario = @IdUsuario";

                SqlCommand cmd = new SqlCommand(cmdSQL, con);

                cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
                cmd.Parameters.AddWithValue("@Nome", nome);
                cmd.Parameters.AddWithValue("@Senha", senha);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@NvAcesso", nvAcesso);

                con.Open();
                cmd.ExecuteNonQuery();
            }
            finally
            {
                con.Close();
            }
        }

        //-----------------------------
        // Excluir
        //-----------------------------
        public void Excluir()
        {
            try
            {
                string cmdSQL = "DELETE FROM Usuario WHERE IdUsuario = @IdUsuario";

                SqlCommand cmd = new SqlCommand(cmdSQL, con);
                cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);

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
        public DataTable Selecionar()
        {
            try
            {
                string cmdSQL = "SELECT * FROM Usuario ORDER BY IdUsuario";

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

        public DataTable SelecionarSeguro()
        {
            try
            {
                string cmdSQL = "SELECT idUsuario ,Email, nome, nvAcesso FROM Usuario ORDER BY IdUsuario";

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

        public DataTable BuscarPorEmail(string email)
        {
            try
            {
                string cmdSQL = "SELECT * FROM Usuario WHERE Email = @Email";

                SqlCommand cmd = new SqlCommand(cmdSQL, con);
                cmd.Parameters.AddWithValue("@Email", email);

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
        public DataTable BuscarPorID(int id)
        {
            try
            {
                string cmdSQL = "SELECT NvAcesso FROM Usuario WHERE IdUsuario = @id";

                SqlCommand cmd = new SqlCommand(cmdSQL, con);
                cmd.Parameters.AddWithValue("@id", id);

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

*/
