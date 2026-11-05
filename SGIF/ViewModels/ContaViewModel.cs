using System.ComponentModel.DataAnnotations;

namespace SGIF.Models
{
    // Página "Minha conta": dois formulários independentes (dados e senha)
    public class ContaViewModel
    {
        public PerfilViewModel Perfil { get; set; } = new();
        public SenhaViewModel Senha { get; set; } = new();
        public string NomePerfil { get; set; } = string.Empty;   // só para exibir
    }

    // Formulário "Meus dados"
    public class PerfilViewModel
    {
        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(200, ErrorMessage = "Máximo de 200 caracteres")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O e-mail é obrigatório")]
        [EmailAddress(ErrorMessage = "Digite um e-mail válido")]
        [StringLength(150, ErrorMessage = "Máximo de 150 caracteres")]
        public string Email { get; set; } = string.Empty;
    }

    // Formulário "Alterar senha"
    public class SenhaViewModel
    {
        [Required(ErrorMessage = "Informe a senha atual")]
        public string SenhaAtual { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe a nova senha")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "A senha deve ter de 6 a 100 caracteres")]
        public string NovaSenha { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirme a nova senha")]
        [Compare(nameof(NovaSenha), ErrorMessage = "As senhas não conferem")]
        public string ConfirmarNovaSenha { get; set; } = string.Empty;
    }
}
