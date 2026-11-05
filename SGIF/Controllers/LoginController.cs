using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SGIF.Classes;
using SGIF.Models;

namespace SGIF.Controllers
{
    public class LoginController : Controller
    {
        private readonly Usuarios _usuarios;

        public LoginController(Usuarios usuarios)
        {
            _usuarios = usuarios;
        }

        public IActionResult LoginExibir()
        {
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

                        // Provisório: trocar pelo Dashboard quando ele existir
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

        public IActionResult SairProcessar()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("LoginExibir");
        }
    }
}
