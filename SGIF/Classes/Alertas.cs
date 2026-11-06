using System.Data;
using Microsoft.Data.SqlClient;
using SGIF.Models;

namespace SGIF.Classes
{
    public class Alertas : BaseDados
    {
        public Alertas(IConfiguration configuration) : base(configuration)
        {
        }

        public int Inserir(Alerta alerta)
        {
            const string sql = @"
                INSERT INTO Alertas (id_leitura, nivel, descricao, data_hora, status)
                OUTPUT INSERTED.id_alerta
                VALUES (@id_leitura, @nivel, @descricao, @data_hora, @status)";

            alerta.IdAlerta = ExecutarEscalar(sql, p =>
            {
                p.AddWithValue("@id_leitura", alerta.IdLeitura);
                p.AddWithValue("@nivel", alerta.Nivel);
                p.AddWithValue("@descricao", ValorOuNulo(alerta.Descricao));
                p.Add("@data_hora", SqlDbType.DateTime2).Value = alerta.DataHora;
                p.AddWithValue("@status", alerta.Status);
            });
            return alerta.IdAlerta;
        }

        public bool AlterarStatus(int idAlerta, string status, int idUsuario)
        {
            // Aberto: sem responsável. Em analise/Resolvido: quem mexeu. Só Resolvido tem data.
            const string sql = @"
            UPDATE Alertas SET
                status = @status,
                id_usuario_responsavel = CASE WHEN @status = 'Aberto' THEN NULL ELSE @id_usuario END,
                data_resolucao = CASE WHEN @status = 'Resolvido' THEN @agora ELSE NULL END
            WHERE id_alerta = @id_alerta";

            return Executar(sql, p =>
            {
                p.AddWithValue("@id_alerta", idAlerta);
                p.AddWithValue("@status", status);
                p.AddWithValue("@id_usuario", idUsuario);
                p.Add("@agora", SqlDbType.DateTime2).Value = DateTime.Now;
            });
        }

        public bool Excluir(int idAlerta)
        {
            const string sql = "DELETE FROM Alertas WHERE id_alerta = @id_alerta";

            return Executar(sql, p => p.AddWithValue("@id_alerta", idAlerta));
        }

        public Alerta? BuscarPorId(int idAlerta)
        {
            const string sql = "SELECT * FROM Alertas WHERE id_alerta = @id_alerta";

            return ConsultarUm(sql, Mapear, p => p.AddWithValue("@id_alerta", idAlerta));
        }

        // Fila de alertas: críticos primeiro, depois os mais recentes. status nulo = todos.
        public List<Alerta> ListarPorStatus(string? status)
        {
            const string sql = @"
                SELECT * FROM Alertas
                WHERE (@status IS NULL OR status = @status)
                ORDER BY CASE nivel WHEN 'Critico' THEN 0 ELSE 1 END, data_hora DESC";

            return Consultar(sql, Mapear, p =>
            {
                p.Add("@status", SqlDbType.VarChar, 50).Value =
                    string.IsNullOrWhiteSpace(status) ? (object)DBNull.Value : status;
            });
        }

        // O sensor já tem um alerta deste nível que ainda não foi resolvido?
        public bool ExisteNaoResolvidoParaSensor(int idSensor, string nivel)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Alertas a
                JOIN Leituras l ON l.id_leitura = a.id_leitura
                WHERE l.id_sensor = @id_sensor
                  AND a.nivel = @nivel
                  AND a.status <> 'Resolvido'";

            return ExecutarEscalar(sql, p =>
            {
                p.AddWithValue("@id_sensor", idSensor);
                p.AddWithValue("@nivel", nivel);
            }) > 0;
        }

        // ---------- Fila de alertas (tela Alertas) ----------

        private const string ColunasDetalhe = @"
            a.id_alerta, a.nivel, a.descricao, a.data_hora, a.status, a.data_resolucao,
            a.id_usuario_responsavel, u.nome AS nome_responsavel,
            l.valor, s.id_sensor, s.tipo AS tipo_sensor, s.modelo AS modelo_sensor,
            s.id_locomotiva, s.id_vagao,
            lo.numero_identificacao AS numero_locomotiva, va.numero_identificacao AS numero_vagao";

        private const string JuncoesDetalhe = @"
            FROM Alertas a
            INNER JOIN Leituras l ON l.id_leitura = a.id_leitura
            INNER JOIN Sensores s ON s.id_sensor = l.id_sensor
            LEFT JOIN Locomotivas lo ON lo.id_locomotiva = s.id_locomotiva
            LEFT JOIN Vagoes va ON va.id_vagao = s.id_vagao
            LEFT JOIN Usuarios u ON u.id_usuario = a.id_usuario_responsavel";

        // Uma página da fila. Não resolvidos primeiro; dentro deles, críticos primeiro e mais recentes antes.
        // status nulo = todos.
        public List<AlertaDetalhe> ListarFila(string? status, int pagina, int tamanho)
        {
            const string sql = "SELECT " + ColunasDetalhe + JuncoesDetalhe + @"
                WHERE (@status IS NULL OR a.status = @status)
                ORDER BY CASE WHEN a.status = 'Resolvido' THEN 1 ELSE 0 END,
                         CASE WHEN a.nivel = 'Critico' THEN 0 ELSE 1 END,
                         a.data_hora DESC, a.id_alerta DESC
                OFFSET @pular ROWS FETCH NEXT @tamanho ROWS ONLY";

            return Consultar(sql, MapearDetalhe, p =>
            {
                p.Add("@status", SqlDbType.VarChar, 50).Value = (object?)status ?? DBNull.Value;
                p.AddWithValue("@pular", (pagina - 1) * tamanho);
                p.AddWithValue("@tamanho", tamanho);
            });
        }

        // Quantos alertas existem no filtro (para a paginação). status nulo = todos.
        public int ContarFila(string? status)
        {
            const string sql = "SELECT COUNT(*) FROM Alertas WHERE (@status IS NULL OR status = @status)";

            return ExecutarEscalar(sql, p =>
            {
                p.Add("@status", SqlDbType.VarChar, 50).Value = (object?)status ?? DBNull.Value;
            });
        }

        // Quantidade de alertas por situação (números dos botões de filtro)
        public Dictionary<string, int> ContarPorStatus()
        {
            const string sql = "SELECT status, COUNT(*) FROM Alertas GROUP BY status";

            return Consultar(sql, r => (Status: r.GetString(0), Total: r.GetInt32(1)), _ => { })
                .ToDictionary(x => x.Status, x => x.Total);
        }

        // Alertas mais recentes de um ativo (locomotiva ou vagão), para a Ficha
        public List<AlertaDetalhe> ListarPorAtivo(bool locomotiva, int idAtivo, int quantidade)
        {
            string filtro = locomotiva ? "s.id_locomotiva = @id_ativo" : "s.id_vagao = @id_ativo";

            string sql = "SELECT TOP (@quantidade) " + ColunasDetalhe + JuncoesDetalhe + @"
                WHERE " + filtro + @"
                ORDER BY a.data_hora DESC, a.id_alerta DESC";

            return Consultar(sql, MapearDetalhe, p =>
            {
                p.AddWithValue("@id_ativo", idAtivo);
                p.AddWithValue("@quantidade", quantidade);
            });
        }

        // ---------- Painel (Tela Inicial) ----------

        // Alertas ainda não resolvidos, por nível (Atencao / Critico)
        public Dictionary<string, int> ContarNaoResolvidosPorNivel()
        {
            const string sql = "SELECT nivel, COUNT(*) FROM Alertas WHERE status <> 'Resolvido' GROUP BY nivel";

            return Consultar(sql, r => (Nivel: r.GetString(0), Total: r.GetInt32(1)), _ => { })
                .ToDictionary(x => x.Nivel, x => x.Total);
        }

        // Os alertas mais recentes do sistema (qualquer situação)
        public List<AlertaDetalhe> ListarRecentes(int quantidade)
        {
            const string sql = "SELECT TOP (@quantidade) " + ColunasDetalhe + JuncoesDetalhe + @"
                ORDER BY a.data_hora DESC, a.id_alerta DESC";

            return Consultar(sql, MapearDetalhe, p => p.AddWithValue("@quantidade", quantidade));
        }

        // Ativos com alertas não resolvidos: os com mais alertas críticos primeiro
        public List<AtivoAtencao> ListarAtivosEmAtencao(int quantidade)
        {
            const string sql = @"
                SELECT TOP (@quantidade)
                    s.id_locomotiva, s.id_vagao,
                    lo.numero_identificacao AS numero_locomotiva, va.numero_identificacao AS numero_vagao,
                    COUNT(*) AS abertos,
                    SUM(CASE WHEN a.nivel = 'Critico' THEN 1 ELSE 0 END) AS criticos
                FROM Alertas a
                INNER JOIN Leituras l ON l.id_leitura = a.id_leitura
                INNER JOIN Sensores s ON s.id_sensor = l.id_sensor
                LEFT JOIN Locomotivas lo ON lo.id_locomotiva = s.id_locomotiva
                LEFT JOIN Vagoes va ON va.id_vagao = s.id_vagao
                WHERE a.status <> 'Resolvido'
                GROUP BY s.id_locomotiva, s.id_vagao, lo.numero_identificacao, va.numero_identificacao
                ORDER BY criticos DESC, abertos DESC";

            return Consultar(sql, r => new AtivoAtencao
            {
                IdLocomotiva = r.GetIntNulo("id_locomotiva"),
                IdVagao = r.GetIntNulo("id_vagao"),
                NumeroLocomotiva = r.GetStringNulo("numero_locomotiva"),
                NumeroVagao = r.GetStringNulo("numero_vagao"),
                AlertasAbertos = r.GetInt32(r.GetOrdinal("abertos")),
                Criticos = r.GetInt32(r.GetOrdinal("criticos"))
            }, p => p.AddWithValue("@quantidade", quantidade));
        }

        public AlertaDetalhe? BuscarDetalhe(int idAlerta)
        {
            const string sql = "SELECT " + ColunasDetalhe + JuncoesDetalhe + " WHERE a.id_alerta = @id_alerta";

            return ConsultarUm(sql, MapearDetalhe, p => p.AddWithValue("@id_alerta", idAlerta));
        }

        private static AlertaDetalhe MapearDetalhe(SqlDataReader r)
        {
            return new AlertaDetalhe
            {
                IdAlerta = r.GetInt32(r.GetOrdinal("id_alerta")),
                Nivel = r.GetString(r.GetOrdinal("nivel")),
                Descricao = r.GetStringNulo("descricao"),
                DataHora = r.GetDateTime(r.GetOrdinal("data_hora")),
                Status = r.GetString(r.GetOrdinal("status")),
                DataResolucao = r.GetDateTimeNulo("data_resolucao"),
                IdUsuarioResponsavel = r.GetIntNulo("id_usuario_responsavel"),
                NomeResponsavel = r.GetStringNulo("nome_responsavel"),
                Valor = r.GetDecimal(r.GetOrdinal("valor")),
                IdSensor = r.GetInt32(r.GetOrdinal("id_sensor")),
                TipoSensor = r.GetString(r.GetOrdinal("tipo_sensor")),
                ModeloSensor = r.GetString(r.GetOrdinal("modelo_sensor")),
                IdLocomotiva = r.GetIntNulo("id_locomotiva"),
                IdVagao = r.GetIntNulo("id_vagao"),
                NumeroLocomotiva = r.GetStringNulo("numero_locomotiva"),
                NumeroVagao = r.GetStringNulo("numero_vagao")
            };
        }

        private static Alerta Mapear(SqlDataReader r)
        {
            return new Alerta
            {
                IdAlerta = r.GetInt32(r.GetOrdinal("id_alerta")),
                IdLeitura = r.GetInt32(r.GetOrdinal("id_leitura")),
                Nivel = r.GetString(r.GetOrdinal("nivel")),
                Descricao = r.GetStringNulo("descricao"),
                DataHora = r.GetDateTime(r.GetOrdinal("data_hora")),
                Status = r.GetString(r.GetOrdinal("status")),
                IdUsuarioResponsavel = r.GetIntNulo("id_usuario_responsavel"),
                DataResolucao = r.GetDateTimeNulo("data_resolucao")
            };
        }
    }
}
