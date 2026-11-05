using Microsoft.Data.SqlClient;
using SGIF.Models;

namespace SGIF.Classes
{
    public class Usuarios : BaseDados
    {
        public Usuarios(IConfiguration configuration) : base(configuration)
        {
        }

        public int Inserir(Usuario usuario)
        {
            const string sql = @"
                INSERT INTO Usuarios (nome, email, senha, nivel_acesso, ativo)
                OUTPUT INSERTED.id_usuario
                VALUES (@nome, @email, @senha, @nivel_acesso, @ativo)";

            usuario.IdUsuario = ExecutarEscalar(sql, p =>
            {
                p.AddWithValue("@nome", usuario.Nome);
                p.AddWithValue("@email", usuario.Email);
                p.AddWithValue("@senha", usuario.Senha);
                p.AddWithValue("@nivel_acesso", usuario.NivelAcesso);
                p.AddWithValue("@ativo", usuario.Ativo);
            });
            return usuario.IdUsuario;
        }

        public bool Atualizar(Usuario usuario)
        {
            const string sql = @"
                UPDATE Usuarios SET
                    nome = @nome,
                    email = @email,
                    nivel_acesso = @nivel_acesso,
                    ativo = @ativo
                WHERE id_usuario = @id_usuario";

            return Executar(sql, p =>
            {
                p.AddWithValue("@id_usuario", usuario.IdUsuario);
                p.AddWithValue("@nome", usuario.Nome);
                p.AddWithValue("@email", usuario.Email);
                p.AddWithValue("@nivel_acesso", usuario.NivelAcesso);
                p.AddWithValue("@ativo", usuario.Ativo);
            });
        }

        // "Minha conta": o próprio usuário altera só nome e e-mail (nunca o nível nem o ativo)
        public bool AtualizarPerfil(int idUsuario, string nome, string email)
        {
            const string sql = @"
                UPDATE Usuarios SET
                    nome = @nome,
                    email = @email
                WHERE id_usuario = @id_usuario";

            return Executar(sql, p =>
            {
                p.AddWithValue("@id_usuario", idUsuario);
                p.AddWithValue("@nome", nome);
                p.AddWithValue("@email", email);
            });
        }

        public bool AtualizarSenha(int idUsuario, string senhaHash)
        {
            const string sql = "UPDATE Usuarios SET senha = @senha WHERE id_usuario = @id_usuario";

            return Executar(sql, p =>
            {
                p.AddWithValue("@id_usuario", idUsuario);
                p.AddWithValue("@senha", senhaHash);
            });
        }

        public bool AlterarAtivo(int idUsuario, bool ativo)
        {
            const string sql = "UPDATE Usuarios SET ativo = @ativo WHERE id_usuario = @id_usuario";

            return Executar(sql, p =>
            {
                p.AddWithValue("@id_usuario", idUsuario);
                p.AddWithValue("@ativo", ativo);
            });
        }

        public bool Excluir(int idUsuario)
        {
            const string sql = "DELETE FROM Usuarios WHERE id_usuario = @id_usuario";

            return Executar(sql, p => p.AddWithValue("@id_usuario", idUsuario));
        }

        public Usuario? BuscarPorId(int idUsuario)
        {
            const string sql = "SELECT * FROM Usuarios WHERE id_usuario = @id_usuario";

            return ConsultarUm(sql, Mapear, p => p.AddWithValue("@id_usuario", idUsuario));
        }

        public Usuario? BuscarPorEmail(string email)
        {
            const string sql = "SELECT * FROM Usuarios WHERE email = @email";

            return ConsultarUm(sql, Mapear, p => p.AddWithValue("@email", email));
        }

        public List<Usuario> ListarTodos()
        {
            const string sql = "SELECT * FROM Usuarios ORDER BY nome";

            return Consultar(sql, Mapear);
        }

        private static Usuario Mapear(SqlDataReader r)
        {
            return new Usuario
            {
                IdUsuario = r.GetInt32(r.GetOrdinal("id_usuario")),
                Nome = r.GetString(r.GetOrdinal("nome")),
                Email = r.GetString(r.GetOrdinal("email")),
                Senha = r.GetString(r.GetOrdinal("senha")),
                NivelAcesso = r.GetInt32(r.GetOrdinal("nivel_acesso")),
                Ativo = r.GetBoolean(r.GetOrdinal("ativo"))
            };
        }
    }
}
