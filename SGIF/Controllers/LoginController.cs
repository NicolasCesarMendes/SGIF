using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SGIF.Classes;
using SGIF.Models;

namespace SGIF.Controllers
{
    [AllowAnonymous]
    public class LoginController : Controller
    {
        private readonly Usuarios _usuarios;

        public LoginController(Usuarios usuarios)
        {
            _usuarios = usuarios;
        }

        public IActionResult LoginExibir()
        {
            // Quem já está logado vai direto para a Tela Inicial
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("idUsuario")))
            {
                return RedirectToAction("PainelOperacionalExibir", "Home");
            }

            return View("LoginExibirView");
        }

        public IActionResult CadastroExibir()
        {
            // Quem já está logado não precisa de cadastro
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("idUsuario")))
            {
                return RedirectToAction("PainelOperacionalExibir", "Home");
            }

            return View("CadastroExibirView");
        }

        // Autocadastro (RF14): a conta é sempre criada como Funcionário.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CadastrarProcessar(CadastroViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View("CadastroExibirView", vm);
            }

            try
            {
                Usuario usuario = new Usuario
                {
                    Nome = vm.Nome.Trim(),
                    Email = vm.Email.Trim(),
                    NivelAcesso = Usuario.Funcionario,   // definido aqui, nunca vem do formulário
                    Ativo = true
                };

                usuario.Senha = new PasswordHasher<Usuario>().HashPassword(usuario, vm.Senha);

                _usuarios.Inserir(usuario);

                TempData["Sucesso"] = "Conta criada com sucesso. Entre com o seu e-mail e a sua senha.";
                return RedirectToAction("LoginExibir");
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                // UNIQUE do banco: e-mail já cadastrado
                ModelState.AddModelError(nameof(vm.Email), "Já existe uma conta com este e-mail.");
                return View("CadastroExibirView", vm);
            }
            catch (Exception)
            {
                ViewBag.Erro = "Não foi possível criar a conta agora. Tente novamente.";
                return View("CadastroExibirView", vm);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult LoginProcessar(LoginViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View("LoginExibirView", vm);
            }

            try
            {
                Usuario? usuario = _usuarios.BuscarPorEmail(vm.Email);

                if (usuario != null)
                {
                    var passwordHasher = new PasswordHasher<Usuario>();

                    var resultado = passwordHasher.VerifyHashedPassword(usuario, usuario.Senha, vm.Senha);

                    if (resultado == PasswordVerificationResult.Success ||
                        resultado == PasswordVerificationResult.SuccessRehashNeeded)
                    {
                        // Senha certa, mas a conta foi desativada: mensagem própria.
                        // (Só aparece com a senha correta, para não revelar quais e-mails existem.)
                        if (!usuario.Ativo)
                        {
                            ViewBag.Erro = "Esta conta está desativada. Procure um administrador do sistema.";
                            return View("LoginExibirView", vm);
                        }

                        HttpContext.Session.SetString("idUsuario", usuario.IdUsuario.ToString());
                        HttpContext.Session.SetString("nome", usuario.Nome);
                        HttpContext.Session.SetString("nivelAcesso", usuario.NivelAcesso.ToString());

                        // Depois de entrar, vai sempre à Tela Inicial
                        return RedirectToAction("PainelOperacionalExibir", "Home");
                    }
                }

                // Mesma mensagem para e-mail inexistente ou senha errada
                ViewBag.Erro = "E-mail ou senha inválidos.";
                return View("LoginExibirView", vm);
            }
            catch (Exception)
            {
                ViewBag.Erro = "Não foi possível entrar agora. Tente novamente.";
                return View("LoginExibirView", vm);
            }
        }

        // Sair por POST (botão do menu), com token anti-forgery
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SairProcessar()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("LoginExibir");
        }
    }
}
