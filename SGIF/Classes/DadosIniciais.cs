using Microsoft.AspNetCore.Identity;
using SGIF.Models;

namespace SGIF.Classes
{
    // Cria o primeiro usuário (administrador) quando a tabela Usuarios está vazia.
    // O e-mail e a senha NÃO ficam no código: vêm da configuração (User Secrets).
    public static class DadosIniciais
    {
        // Nível de acesso do administrador (1 = Administrador, 2 = Manutenção, 3 = Funcionário)
        private const int NivelAdministrador = 1;

        public static void CriarAdministrador(IServiceProvider servicos)
        {
            using var escopo = servicos.CreateScope();

            IConfiguration config = escopo.ServiceProvider.GetRequiredService<IConfiguration>();
            ILogger logger = escopo.ServiceProvider
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger("DadosIniciais");

            string? email = config["AdminInicial:Email"];
            string? senha = config["AdminInicial:Senha"];

            // Sem configuração, não faz nada
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(senha))
            {
                return;
            }

            try
            {
                Usuarios usuarios = escopo.ServiceProvider.GetRequiredService<Usuarios>();

                // Só cria se ainda não existir nenhum usuário
                if (usuarios.ListarTodos().Count > 0)
                {
                    return;
                }

                Usuario admin = new Usuario
                {
                    Nome = "Administrador",
                    Email = email.Trim(),
                    NivelAcesso = NivelAdministrador,
                    Ativo = true
                };

                admin.Senha = new PasswordHasher<Usuario>().HashPassword(admin, senha);

                usuarios.Inserir(admin);

                logger.LogInformation("Administrador inicial criado: {Email}", admin.Email);
            }
            catch (Exception ex)
            {
                // Não derruba a aplicação se o banco estiver fora do ar
                logger.LogWarning("Não foi possível criar o administrador inicial: {Mensagem}", ex.Message);
            }
        }
    }
}