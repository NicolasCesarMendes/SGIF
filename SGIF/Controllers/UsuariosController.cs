using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SGIF.Classes;
using SGIF.Models;

namespace SGIF.Controllers
{
    // Gestão de usuários (RF15): somente o Administrador.
    // O administrador não altera a própria conta aqui (usa "Minha conta").
    // Criar, editar e excluir acontecem em modais: sucesso = 200; erro de validação = 422 com o formulário.
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
        public IActionResult UsuariosExibir()
        {
            UsuariosViewModel vm = new UsuariosViewModel
            {
                Itens = _usuarios.ListarTodos(),
                IdLogado = IdLogado()
            };

            return View("UsuariosExibirView", vm);
        }

        // Formulário do modal (novo, ou edição se vier o id)
        public IActionResult FormUsuario(int? id)
        {
            UsuarioFormViewModel vm = new UsuarioFormViewModel();

            if (id.HasValue && id.Value > 0)
            {
                if (id.Value == IdLogado())
                {
                    return Conflict(MensagemPropriaConta);
                }

                Usuario? usuario = _usuarios.BuscarPorId(id.Value);

                if (usuario == null)
                {
                    return NotFound("Usuário não encontrado. Atualize a página.");
                }

                vm.IdUsuario = usuario.IdUsuario;
                vm.Nome = usuario.Nome;
                vm.Email = usuario.Email;
                vm.NivelAcesso = usuario.NivelAcesso;
                vm.Ativo = usuario.Ativo;
            }

            return PartialView("_FormUsuario", vm);
        }

        // Salvar (novo ou edição)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Salvar(UsuarioFormViewModel vm)
        {
            bool novo = vm.IdUsuario == 0;

            if (!novo && vm.IdUsuario == IdLogado())
            {
                return ErroGeral(vm, MensagemPropriaConta);
            }

            if (novo && string.IsNullOrWhiteSpace(vm.Senha))
            {
                ModelState.AddModelError(nameof(vm.Senha), "A senha é obrigatória");
            }

            ValidarNivel(vm);

            if (!ModelState.IsValid)
            {
                return FormComErros(vm);
            }

            try
            {
                PasswordHasher<Usuario> hasher = new PasswordHasher<Usuario>();

                if (novo)
                {
                    Usuario usuario = new Usuario
                    {
                        Nome = vm.Nome.Trim(),
                        Email = vm.Email.Trim(),
                        NivelAcesso = vm.NivelAcesso,
                        Ativo = true
                    };

                    usuario.Senha = hasher.HashPassword(usuario, vm.Senha!);

                    _usuarios.Inserir(usuario);

                    TempData["Sucesso"] = "Usuário criado com sucesso.";
                }
                else
                {
                    Usuario? usuario = _usuarios.BuscarPorId(vm.IdUsuario);

                    if (usuario == null)
                    {
                        return ErroGeral(vm, "Usuário não encontrado. Atualize a página.");
                    }

                    usuario.Nome = vm.Nome.Trim();
                    usuario.Email = vm.Email.Trim();
                    usuario.NivelAcesso = vm.NivelAcesso;
                    usuario.Ativo = vm.Ativo;

                    _usuarios.Atualizar(usuario);

                    // Senha em branco = mantém a senha atual
                    if (!string.IsNullOrWhiteSpace(vm.Senha))
                    {
                        _usuarios.AtualizarSenha(usuario.IdUsuario, hasher.HashPassword(usuario, vm.Senha));
                    }

                    TempData["Sucesso"] = "Usuário atualizado com sucesso.";
                }

                return Ok();
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                ModelState.AddModelError(nameof(vm.Email), "Já existe uma conta com este e-mail.");
                return FormComErros(vm);
            }
            catch (Exception)
            {
                return ErroGeral(vm, "Não foi possível salvar. Tente novamente.");
            }
        }

        // Excluir (a confirmação é feita no modal). Sucesso = 200; erro = mensagem em texto.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Excluir(int id)
        {
            if (id == IdLogado())
            {
                return Conflict(MensagemPropriaConta);
            }

            // Usuários de teste padrão (ids 1, 2 e 3) nunca são excluídos
            if (Usuario.EhPadrao(id))
            {
                return Conflict("Este é um usuário de teste padrão e não pode ser excluído.");
            }

            try
            {
                if (!_usuarios.Excluir(id))
                {
                    return NotFound("Usuário não encontrado. Atualize a página.");
                }

                TempData["Sucesso"] = "Usuário excluído com sucesso.";
                return Ok();
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                // Chave estrangeira: o usuário é responsável por algum alerta
                return Conflict("Este usuário possui alertas associados. Desative-o em vez de excluir.");
            }
            catch (Exception)
            {
                return StatusCode(500, "Não foi possível excluir. Tente novamente.");
            }
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

        // Devolve o formulário com as mensagens de erro (o JavaScript o coloca de volta no modal)
        private IActionResult FormComErros(UsuarioFormViewModel vm)
        {
            Response.StatusCode = 422;
            return PartialView("_FormUsuario", vm);
        }

        private IActionResult ErroGeral(UsuarioFormViewModel vm, string mensagem)
        {
            ModelState.AddModelError(string.Empty, mensagem);
            return FormComErros(vm);
        }
    }
}
