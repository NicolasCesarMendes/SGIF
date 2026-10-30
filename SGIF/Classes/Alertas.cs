using System.Data;
using Microsoft.Data.SqlClient;

namespace SGIF.Classes
{
    public class Alertas
    {
        //-----------------------------
        // Atributos
        //-----------------------------
        private int _idAlerta;
        private int _idLeitura;
        private string _nivel = string.Empty;
        private string? _descricao;
        private DateTime _dataHora = DateTime.Now;
        private string _status = "Aberto";

        SqlConnection con;

        //-----------------------------
        // Getters e Setters
        //-----------------------------
        public int IdAlerta { get { return _idAlerta; } set { _idAlerta = value; } }
        public int IdLeitura { get { return _idLeitura; } set { _idLeitura = value; } }
        public string Nivel { get { return _nivel; } set { _nivel = value; } }
        public string? Descricao { get { return _descricao; } set { _descricao = value; } }
        public DateTime DataHora { get { return _dataHora; } set { _dataHora = value; } }
        public string Status { get { return _status; } set { _status = value; } }

        //-----------------------------
        // Construtor
        //-----------------------------
        public Alertas()
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
        // Inserir (preenche IdAlerta com o id gerado)
        // Nivel: 'Atencao' ou 'Critico'
        //-----------------------------
        public void Inserir()
        {
            try
            {
                string cmdSQL = @"
                    INSERT INTO Alertas
                    (
                        id_leitura, nivel, descricao, data_hora, status
                    )
                    OUTPUT INSERTED.id_alerta
                    VALUES
                    (
                        @id_leitura, @nivel, @descricao, @data_hora, @status
                    )";

                SqlCommand cmd = new SqlCommand(cmdSQL, con);

                cmd.Parameters.AddWithValue("@id_leitura", IdLeitura);
                cmd.Parameters.AddWithValue("@nivel", Nivel);
                cmd.Parameters.AddWithValue("@descricao", (object?)Descricao ?? DBNull.Value);
                cmd.Parameters.Add("@data_hora", SqlDbType.DateTime2).Value = DataHora;
                cmd.Parameters.AddWithValue("@status", Status);

                con.Open();
                IdAlerta = Convert.ToInt32(cmd.ExecuteScalar());
            }
            finally
            {
                con.Close();
            }
        }

        //-----------------------------
        // Alterar status (use IdAlerta e Status)
        // Status: 'Aberto', 'Em analise' ou 'Resolvido'
        //-----------------------------
        public void AlterarStatus()
        {
            try
            {
                string cmdSQL = @"
                    UPDATE Alertas SET
                        status = @status
                    WHERE id_alerta = @id_alerta";

                SqlCommand cmd = new SqlCommand(cmdSQL, con);

                cmd.Parameters.AddWithValue("@id_alerta", IdAlerta);
                cmd.Parameters.AddWithValue("@status", Status);

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
                string cmdSQL = "DELETE FROM Alertas WHERE id_alerta = @id_alerta";

                SqlCommand cmd = new SqlCommand(cmdSQL, con);

                cmd.Parameters.AddWithValue("@id_alerta", IdAlerta);

                con.Open();
                cmd.ExecuteNonQuery();
            }
            finally
            {
                con.Close();
            }
        }

        //-----------------------------
        // Selecionar (críticos primeiro, depois os mais recentes)
        //-----------------------------
        public DataTable? Selecionar()
        {
            try
            {
                string cmdSQL = @"
                    SELECT *
                    FROM Alertas
                    ORDER BY CASE nivel WHEN 'Critico' THEN 0 ELSE 1 END, data_hora DESC";

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
                    FROM Alertas
                    WHERE id_alerta = @id_alerta";

                SqlCommand cmd = new SqlCommand(cmdSQL, con);

                cmd.Parameters.AddWithValue("@id_alerta", id);

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
        // Selecionar por status (críticos primeiro)
        //-----------------------------
        public DataTable? SelecionarPorStatus(string status)
        {
            try
            {
                string cmdSQL = @"
                    SELECT *
                    FROM Alertas
                    WHERE status = @status
                    ORDER BY CASE nivel WHEN 'Critico' THEN 0 ELSE 1 END, data_hora DESC";

                SqlCommand cmd = new SqlCommand(cmdSQL, con);

                cmd.Parameters.AddWithValue("@status", status);

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
