using System.ComponentModel.DataAnnotations;

namespace SGIF.Models
{
    // Autocadastro (RF14): a conta sempre nasce como Funcionário.
    // O nível de acesso NÃO faz parte deste formulário: quem define é o servidor.
    public class CadastroViewModel
    {
        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(200, ErrorMessage = "Máximo de 200 caracteres")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O e-mail é obrigatório")]
        [EmailAddress(ErrorMessage = "Digite um e-mail válido")]
        [StringLength(150, ErrorMessage = "Máximo de 150 caracteres")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A senha é obrigatória")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "A senha deve ter de 6 a 100 caracteres")]
        public string Senha { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirme a senha")]
        [Compare(nameof(Senha), ErrorMessage = "As senhas não conferem")]
        public string ConfirmarSenha { get; set; } = string.Empty;
    }
}
