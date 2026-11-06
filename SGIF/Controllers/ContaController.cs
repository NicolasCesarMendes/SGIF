using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SGIF.Classes;
using SGIF.Models;

namespace SGIF.Controllers
{
    // Minha conta (RF14): qualquer usuário logado altera os próprios dados e a própria senha.
    // O nível de acesso e o "ativo" NÃO podem ser alterados aqui.
    // Abre em modal (pelo nome do usuário no menu): sucesso = 200; erro de validação = 422 com o modal.
    public class ContaController : Controller
    {
        private readonly Usuarios _usuarios;

        public ContaController(Usuarios usuarios)
        {
            _usuarios = usuarios;
        }

        // Conteúdo do modal (dois formulários: dados e senha)
        public IActionResult Modal()
        {
            Usuario? usuario = BuscarLogado();

            if (usuario == null)
            {
                return RedirectToAction("LoginExibir", "Login");
            }

            return PartialView("_ModalConta", MontarViewModel(usuario));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SalvarPerfilProcessar(PerfilViewModel vm)
        {
            Usuario? usuario = BuscarLogado();

            if (usuario == null)
            {
                return RedirectToAction("LoginExibir", "Login");
            }

            if (!ModelState.IsValid)
            {
                return ModalComErros(usuario, vm);
            }

            try
            {
                string nome = vm.Nome.Trim();

                _usuarios.AtualizarPerfil(usuario.IdUsuario, nome, vm.Email.Trim());
                HttpContext.Session.SetString("nome", nome);

                TempData["Sucesso"] = "Dados atualizados com sucesso.";
                return Ok();
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                // UNIQUE do banco: e-mail já usado por outra conta
                ModelState.AddModelError(nameof(vm.Email), "Já existe uma conta com este e-mail.");
                return ModalComErros(usuario, vm);
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Não foi possível salvar. Tente novamente.");
                return ModalComErros(usuario, vm);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AlterarSenhaProcessar(SenhaViewModel vm)
        {
            Usuario? usuario = BuscarLogado();

            if (usuario == null)
            {
                return RedirectToAction("LoginExibir", "Login");
            }

            var hasher = new PasswordHasher<Usuario>();

            if (ModelState.IsValid)
            {
                PasswordVerificationResult resultado = hasher.VerifyHashedPassword(usuario, usuario.Senha, vm.SenhaAtual);

                if (resultado == PasswordVerificationResult.Failed)
                {
                    ModelState.AddModelError(nameof(vm.SenhaAtual), "A senha atual está incorreta.");
                }
            }

            if (!ModelState.IsValid)
            {
                // Os campos de senha nunca voltam preenchidos
                return ModalComErros(usuario, null);
            }

            try
            {
                _usuarios.AtualizarSenha(usuario.IdUsuario, hasher.HashPassword(usuario, vm.NovaSenha));

                TempData["Sucesso"] = "Senha alterada com sucesso.";
                return Ok();
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Não foi possível alterar a senha. Tente novamente.");
                return ModalComErros(usuario, null);
            }
        }

        // ---------- Auxiliares ----------

        private Usuario? BuscarLogado()
        {
            if (!int.TryParse(HttpContext.Session.GetString("idUsuario"), out int idUsuario))
            {
                return null;
            }

            return _usuarios.BuscarPorId(idUsuario);
        }

        private static ContaViewModel MontarViewModel(Usuario usuario)
        {
            return new ContaViewModel
            {
                Perfil = new PerfilViewModel { Nome = usuario.Nome, Email = usuario.Email },
                NomePerfil = Usuario.NomePerfil(usuario.NivelAcesso)
            };
        }

        // Devolve o modal com as mensagens de erro, mantendo o que o usuário digitou nos dados
        private IActionResult ModalComErros(Usuario usuario, PerfilViewModel? digitado)
        {
            ContaViewModel vm = MontarViewModel(usuario);

            if (digitado != null)
            {
                vm.Perfil = digitado;
            }

            Response.StatusCode = 422;
            return PartialView("_ModalConta", vm);
        }
    }
}
