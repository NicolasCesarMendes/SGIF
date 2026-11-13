namespace SGIF.Models
{
    public class Usuario
    {
        // Níveis de acesso (coluna nivel_acesso)
        public const int Administrador = 1;
        public const int Manutencao = 2;
        public const int Funcionario = 3;

        // Usuários de teste padrão (ids 1, 2 e 3): não podem ser excluídos
        public static readonly int[] IdsPadrao = { 1, 2, 3 };

        public static bool EhPadrao(int idUsuario)
        {
            return IdsPadrao.Contains(idUsuario);
        }

        public int IdUsuario { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
        public int NivelAcesso { get; set; } = Funcionario;
        public bool Ativo { get; set; } = true;

        // Nome do perfil para exibir na tela
        public static string NomePerfil(int nivel)
        {
            return nivel switch
            {
                Administrador => "Administrador",
                Manutencao => "Funcionário da Manutenção",
                Funcionario => "Funcionário",
                _ => "Desconhecido"
            };
        }
    }
}
