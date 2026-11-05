using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SGIF.Classes;
using SGIF.Models;

namespace SGIF.Controllers
{
    // Gestão de usuários (RF15): somente o Administrador.
    // O administrador não altera a própria conta aqui (usa "Minha conta").
    [RequerPerfil(Usuario.Administrador)]
    public class UsuariosController : Controller
    {
        private const string MensagemPropriaConta = "Para alterar os seus próprios dados, use Minha conta.";

        private readonly Usuarios _usuarios;

        public UsuariosController(Usuarios usuarios)
        {
            _usuarios = usuarios;
        }

        // Lista
        public IActionResult Index()
        {
            UsuariosViewModel vm = new UsuariosViewModel
            {
                Itens = _usuarios.ListarTodos(),
                IdLogado = IdLogado()
            };

            return View(vm);
        }

        // Criar
        public IActionResult CriarExibir()
        {
            return View("Form", new UsuarioFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CriarProcessar(UsuarioFormViewModel vm)
        {
            if (string.IsNullOrWhiteSpace(vm.Senha))
            {
                ModelState.AddModelError(nameof(vm.Senha), "A senha é obrigatória");
            }

            ValidarNivel(vm);

            if (!ModelState.IsValid)
            {
                return View("Form", vm);
            }

            try
            {
                Usuario usuario = new Usuario
                {
                    Nome = vm.Nome.Trim(),
                    Email = vm.Email.Trim(),
                    NivelAcesso = vm.NivelAcesso,
                    Ativo = true
                };

                usuario.Senha = new PasswordHasher<Usuario>().HashPassword(usuario, vm.Senha!);

                _usuarios.Inserir(usuario);

                TempData["Sucesso"] = "Usuário criado com sucesso.";
                return RedirectToAction("Index");
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                ModelState.AddModelError(nameof(vm.Email), "Já existe uma conta com este e-mail.");
                return View("Form", vm);
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Não foi possível salvar. Tente novamente.");
                return View("Form", vm);
            }
        }

        // Editar
        public IActionResult EditarExibir(int id)
        {
            if (id == IdLogado())
            {
                TempData["Erro"] = MensagemPropriaConta;
                return RedirectToAction("Index");
            }

            Usuario? usuario = _usuarios.BuscarPorId(id);

            if (usuario == null)
            {
                TempData["Erro"] = "Usuário não encontrado.";
                return RedirectToAction("Index");
            }

            UsuarioFormViewModel vm = new UsuarioFormViewModel
            {
                IdUsuario = usuario.IdUsuario,
                Nome = usuario.Nome,
                Email = usuario.Email,
                NivelAcesso = usuario.NivelAcesso
            };

            return View("Form", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditarProcessar(UsuarioFormViewModel vm)
        {
            if (vm.IdUsuario == IdLogado())
            {
                TempData["Erro"] = MensagemPropriaConta;
                return RedirectToAction("Index");
            }

            ValidarNivel(vm);

            if (!ModelState.IsValid)
            {
                return View("Form", vm);
            }

            try
            {
                Usuario? usuario = _usuarios.BuscarPorId(vm.IdUsuario);

                if (usuario == null)
                {
                    TempData["Erro"] = "Usuário não encontrado.";
                    return RedirectToAction("Index");
                }

                usuario.Nome = vm.Nome.Trim();
                usuario.Email = vm.Email.Trim();
                usuario.NivelAcesso = vm.NivelAcesso;

                _usuarios.Atualizar(usuario);

                // Senha em branco = mantém a senha atual
                if (!string.IsNullOrWhiteSpace(vm.Senha))
                {
                    string hash = new PasswordHasher<Usuario>().HashPassword(usuario, vm.Senha);
                    _usuarios.AtualizarSenha(usuario.IdUsuario, hash);
                }

                TempData["Sucesso"] = "Usuário atualizado com sucesso.";
                return RedirectToAction("Index");
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                ModelState.AddModelError(nameof(vm.Email), "Já existe uma conta com este e-mail.");
                return View("Form", vm);
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Não foi possível salvar. Tente novamente.");
                return View("Form", vm);
            }
        }

        // Ativar / Desativar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AlterarAtivoProcessar(int id, bool ativo)
        {
            if (id == IdLogado())
            {
                TempData["Erro"] = MensagemPropriaConta;
                return RedirectToAction("Index");
            }

            try
            {
                if (_usuarios.AlterarAtivo(id, ativo))
                {
                    TempData["Sucesso"] = ativo ? "Usuário ativado." : "Usuário desativado.";
                }
                else
                {
                    TempData["Erro"] = "Usuário não encontrado.";
                }
            }
            catch (Exception)
            {
                TempData["Erro"] = "Não foi possível alterar a situação. Tente novamente.";
            }

            return RedirectToAction("Index");
        }

        // Excluir (a confirmação é feita na tela, antes de enviar)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ExcluirProcessar(int id)
        {
            if (id == IdLogado())
            {
                TempData["Erro"] = MensagemPropriaConta;
                return RedirectToAction("Index");
            }

            try
            {
                if (_usuarios.Excluir(id))
                {
                    TempData["Sucesso"] = "Usuário excluído com sucesso.";
                }
                else
                {
                    TempData["Erro"] = "Usuário não encontrado.";
                }
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                // Chave estrangeira: o usuário é responsável por algum alerta
                TempData["Erro"] = "Este usuário possui alertas associados. Desative-o em vez de excluir.";
            }
            catch (Exception)
            {
                TempData["Erro"] = "Não foi possível excluir. Tente novamente.";
            }

            return RedirectToAction("Index");
        }

        // ---------- Auxiliares ----------

        private int IdLogado()
        {
            int.TryParse(HttpContext.Session.GetString("idUsuario"), out int id);
            return id;
        }

        private void ValidarNivel(UsuarioFormViewModel vm)
        {
            int[] niveisValidos = { Usuario.Administrador, Usuario.Manutencao, Usuario.Funcionario };

            if (!niveisValidos.Contains(vm.NivelAcesso))
            {
                ModelState.AddModelError(nameof(vm.NivelAcesso), "Selecione um perfil válido");
            }
        }
    }
}
