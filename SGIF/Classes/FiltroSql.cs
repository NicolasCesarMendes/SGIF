using Microsoft.Data.SqlClient;
using System.Data;
using SGIF.Models;

namespace SGIF.Classes
{
    // Pedaço de SQL e parâmetros compartilhados pelas consultas filtradas (RF13).
    // Exige que a consulta tenha a tabela Sensores com o alias "s".
    public static class FiltroSql
    {
        public const string Condicao = @"
            AND (@tipo_sensor IS NULL OR s.tipo = @tipo_sensor)
            AND (@tipo_ativo IS NULL
                 OR (@tipo_ativo = 'Locomotiva' AND s.id_locomotiva IS NOT NULL
                     AND (@id_ativo IS NULL OR s.id_locomotiva = @id_ativo))
                 OR (@tipo_ativo = 'Vagao' AND s.id_vagao IS NOT NULL
                     AND (@id_ativo IS NULL OR s.id_vagao = @id_ativo)))";

        public static void Parametros(SqlParameterCollection p, FiltroAnalise f)
        {
            p.Add("@tipo_sensor", SqlDbType.VarChar, 50).Value = (object?)f.TipoSensor ?? DBNull.Value;
            p.Add("@tipo_ativo", SqlDbType.VarChar, 20).Value = (object?)f.TipoAtivo ?? DBNull.Value;
            p.Add("@id_ativo", SqlDbType.Int).Value = (object?)f.IdAtivo ?? DBNull.Value;
        }
    }
}
