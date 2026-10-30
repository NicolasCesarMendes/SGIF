using System.Data;
using Microsoft.Data.SqlClient;

namespace SGIF.Classes
{
    public class Leituras
    {
        //-----------------------------
        // Atributos
        //-----------------------------
        private int _idLeitura;
        private int _idSensor;
        private decimal _valor;
        private DateTime _dataHora = DateTime.Now;

        SqlConnection con;

        //-----------------------------
        // Getters e Setters
        //-----------------------------
        public int IdLeitura { get { return _idLeitura; } set { _idLeitura = value; } }
        public int IdSensor { get { return _idSensor; } set { _idSensor = value; } }
        public decimal Valor { get { return _valor; } set { _valor = value; } }
        public DateTime DataHora { get { return _dataHora; } set { _dataHora = value; } }

        //-----------------------------
        // Construtor
        //-----------------------------
        public Leituras()
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
        // Inserir (preenche IdLeitura com o id gerado)
        //-----------------------------
        public void Inserir()
        {
            try
            {
                string cmdSQL = @"
                    INSERT INTO Leituras
                    (
                        id_sensor, valor, data_hora
                    )
                    OUTPUT INSERTED.id_leitura
                    VALUES
                    (
                        @id_sensor, @valor, @data_hora
                    )";

                SqlCommand cmd = new SqlCommand(cmdSQL, con);

                cmd.Parameters.AddWithValue("@id_sensor", IdSensor);
                cmd.Parameters.AddWithValue("@valor", Valor);
                cmd.Parameters.Add("@data_hora", SqlDbType.DateTime2).Value = DataHora;

                con.Open();
                IdLeitura = Convert.ToInt32(cmd.ExecuteScalar());
            }
            finally
            {
                con.Close();
            }
        }

        //-----------------------------
        // Buscar por ID
        // (Leituras é histórico: não há Alterar, Excluir nem Selecionar geral)
        //-----------------------------
        public DataTable? BuscarPorId(int id)
        {
            try
            {
                string cmdSQL = @"
                    SELECT *
                    FROM Leituras
                    WHERE id_leitura = @id_leitura";

                SqlCommand cmd = new SqlCommand(cmdSQL, con);

                cmd.Parameters.AddWithValue("@id_leitura", id);

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
        // Selecionar por sensor (período opcional)
        //-----------------------------
        public DataTable? SelecionarPorSensor(int idSensor, DateTime? inicio = null, DateTime? fim = null)
        {
            try
            {
                string cmdSQL = @"
                    SELECT *
                    FROM Leituras
                    WHERE id_sensor = @id_sensor
                      AND (@inicio IS NULL OR data_hora >= @inicio)
                      AND (@fim IS NULL OR data_hora <= @fim)
                    ORDER BY data_hora";

                SqlCommand cmd = new SqlCommand(cmdSQL, con);

                cmd.Parameters.AddWithValue("@id_sensor", idSensor);
                cmd.Parameters.Add("@inicio", SqlDbType.DateTime2).Value = (object?)inicio ?? DBNull.Value;
                cmd.Parameters.Add("@fim", SqlDbType.DateTime2).Value = (object?)fim ?? DBNull.Value;

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
        // Última leitura de um sensor
        //-----------------------------
        public DataTable? BuscarUltimaPorSensor(int idSensor)
        {
            try
            {
                string cmdSQL = @"
                    SELECT TOP 1 *
                    FROM Leituras
                    WHERE id_sensor = @id_sensor
                    ORDER BY data_hora DESC";

                SqlCommand cmd = new SqlCommand(cmdSQL, con);

                cmd.Parameters.AddWithValue("@id_sensor", idSensor);

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
