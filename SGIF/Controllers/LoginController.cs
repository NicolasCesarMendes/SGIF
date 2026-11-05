using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
                return RedirectToAction("Index", "Home");
            }

            return View("LoginExibirView");
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

                if (usuario != null && usuario.Ativo)
                {
                    var passwordHasher = new PasswordHasher<Usuario>();

                    var resultado = passwordHasher.VerifyHashedPassword(usuario, usuario.Senha, vm.Senha);

                    if (resultado == PasswordVerificationResult.Success ||
                        resultado == PasswordVerificationResult.SuccessRehashNeeded)
                    {
                        HttpContext.Session.SetString("idUsuario", usuario.IdUsuario.ToString());
                        HttpContext.Session.SetString("nome", usuario.Nome);
                        HttpContext.Session.SetString("nivelAcesso", usuario.NivelAcesso.ToString());

                        // Depois de entrar, vai sempre à Tela Inicial
                        return RedirectToAction("Index", "Home");
                    }
                }

                // Mesma mensagem para e-mail inexistente, senha errada ou conta desativada
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
