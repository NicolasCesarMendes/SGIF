using System.Data;
using Microsoft.Data.SqlClient;

namespace SGIF.Classes
{
    public class Sensores
    {
        //-----------------------------
        // Atributos
        //-----------------------------
        private int _idSensor;
        private string _tipo = string.Empty;
        private string _modelo = string.Empty;
        private decimal? _valorMinimo;
        private decimal? _valorMaximo;
        private decimal? _limiteAtencao;
        private decimal? _limiteCritico;
        private int? _idLocomotiva;
        private int? _idVagao;

        SqlConnection con;

        //-----------------------------
        // Getters e Setters
        //-----------------------------
        public int IdSensor { get { return _idSensor; } set { _idSensor = value; } }
        public string Tipo { get { return _tipo; } set { _tipo = value; } }
        public string Modelo { get { return _modelo; } set { _modelo = value; } }
        public decimal? ValorMinimo { get { return _valorMinimo; } set { _valorMinimo = value; } }
        public decimal? ValorMaximo { get { return _valorMaximo; } set { _valorMaximo = value; } }
        public decimal? LimiteAtencao { get { return _limiteAtencao; } set { _limiteAtencao = value; } }
        public decimal? LimiteCritico { get { return _limiteCritico; } set { _limiteCritico = value; } }
        public int? IdLocomotiva { get { return _idLocomotiva; } set { _idLocomotiva = value; } }
        public int? IdVagao { get { return _idVagao; } set { _idVagao = value; } }

        //-----------------------------
        // Construtor
        //-----------------------------
        public Sensores()
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
        // Inserir (preenche IdSensor com o id gerado)
        // Informe IdLocomotiva OU IdVagao (nunca os dois)
        //-----------------------------
        public void Inserir()
        {
            try
            {
                string cmdSQL = @"
                    INSERT INTO Sensores
                    (
                        tipo, modelo, valor_minimo, valor_maximo,
                        limite_atencao, limite_critico, id_locomotiva, id_vagao
                    )
                    OUTPUT INSERTED.id_sensor
                    VALUES
                    (
                        @tipo, @modelo, @valor_minimo, @valor_maximo,
                        @limite_atencao, @limite_critico, @id_locomotiva, @id_vagao
                    )";

                SqlCommand cmd = new SqlCommand(cmdSQL, con);

                cmd.Parameters.AddWithValue("@tipo", Tipo);
                cmd.Parameters.AddWithValue("@modelo", Modelo);
                cmd.Parameters.AddWithValue("@valor_minimo", (object?)ValorMinimo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@valor_maximo", (object?)ValorMaximo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@limite_atencao", (object?)LimiteAtencao ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@limite_critico", (object?)LimiteCritico ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@id_locomotiva", (object?)IdLocomotiva ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@id_vagao", (object?)IdVagao ?? DBNull.Value);

                con.Open();
                IdSensor = Convert.ToInt32(cmd.ExecuteScalar());
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
                    UPDATE Sensores SET
                        tipo = @tipo,
                        modelo = @modelo,
                        valor_minimo = @valor_minimo,
                        valor_maximo = @valor_maximo,
                        limite_atencao = @limite_atencao,
                        limite_critico = @limite_critico,
                        id_locomotiva = @id_locomotiva,
                        id_vagao = @id_vagao
                    WHERE id_sensor = @id_sensor";

                SqlCommand cmd = new SqlCommand(cmdSQL, con);

                cmd.Parameters.AddWithValue("@id_sensor", IdSensor);
                cmd.Parameters.AddWithValue("@tipo", Tipo);
                cmd.Parameters.AddWithValue("@modelo", Modelo);
                cmd.Parameters.AddWithValue("@valor_minimo", (object?)ValorMinimo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@valor_maximo", (object?)ValorMaximo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@limite_atencao", (object?)LimiteAtencao ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@limite_critico", (object?)LimiteCritico ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@id_locomotiva", (object?)IdLocomotiva ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@id_vagao", (object?)IdVagao ?? DBNull.Value);

                con.Open();
                cmd.ExecuteNonQuery();
            }
            finally
            {
                con.Close();
            }
        }

        //-----------------------------
        // Excluir (erro 547 se o sensor já tiver leituras)
        //-----------------------------
        public void Excluir()
        {
            try
            {
                string cmdSQL = "DELETE FROM Sensores WHERE id_sensor = @id_sensor";

                SqlCommand cmd = new SqlCommand(cmdSQL, con);

                cmd.Parameters.AddWithValue("@id_sensor", IdSensor);

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
                string cmdSQL = "SELECT * FROM Sensores ORDER BY id_sensor";

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
                    FROM Sensores
                    WHERE id_sensor = @id_sensor";

                SqlCommand cmd = new SqlCommand(cmdSQL, con);

                cmd.Parameters.AddWithValue("@id_sensor", id);

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
        // Selecionar por locomotiva
        //-----------------------------
        public DataTable? SelecionarPorLocomotiva(int idLocomotiva)
        {
            try
            {
                string cmdSQL = @"
                    SELECT *
                    FROM Sensores
                    WHERE id_locomotiva = @id_locomotiva
                    ORDER BY id_sensor";

                SqlCommand cmd = new SqlCommand(cmdSQL, con);

                cmd.Parameters.AddWithValue("@id_locomotiva", idLocomotiva);

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
        // Selecionar por vagão
        //-----------------------------
        public DataTable? SelecionarPorVagao(int idVagao)
        {
            try
            {
                string cmdSQL = @"
                    SELECT *
                    FROM Sensores
                    WHERE id_vagao = @id_vagao
                    ORDER BY id_sensor";

                SqlCommand cmd = new SqlCommand(cmdSQL, con);

                cmd.Parameters.AddWithValue("@id_vagao", idVagao);

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
