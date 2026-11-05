using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SGIF.Models;

namespace SGIF.Classes
{
    // Roda antes de TODA action (registrado em Program.cs).
    // 1) Sem login -> vai para o Login (RNF4). Exceção: actions com [AllowAnonymous]
    // 2) Conta desativada ou removida -> encerra a sessão e vai para o Login
    // 3) Perfil sem permissão ([RequerPerfil]) -> vai para a Tela Inicial
    public class AutenticacaoFiltro : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            IList<object> metadados = context.ActionDescriptor.EndpointMetadata;

            // Páginas públicas (Landing, Login...)
            if (metadados.OfType<IAllowAnonymous>().Any())
            {
                return;
            }

            HttpContext http = context.HttpContext;

            if (!int.TryParse(http.Session.GetString("idUsuario"), out int idUsuario))
            {
                context.Result = new RedirectToActionResult("LoginExibir", "Login", null);
                return;
            }

            // Confere no banco: a conta pode ter sido desativada ou ter mudado de perfil
            Usuarios usuarios = http.RequestServices.GetRequiredService<Usuarios>();
            Usuario? usuario = usuarios.BuscarPorId(idUsuario);

            if (usuario == null || !usuario.Ativo)
            {
                http.Session.Clear();
                context.Result = new RedirectToActionResult("LoginExibir", "Login", null);
                return;
            }

            // Mantém a sessão atualizada (o menu usa esses valores)
            http.Session.SetString("nome", usuario.Nome);
            http.Session.SetString("nivelAcesso", usuario.NivelAcesso.ToString());

            // Restrição por perfil, se a action/controller tiver [RequerPerfil]
            List<RequerPerfilAttribute> exigidos = metadados.OfType<RequerPerfilAttribute>().ToList();

            if (exigidos.Count > 0 && !exigidos.All(e => e.Perfis.Contains(usuario.NivelAcesso)))
            {
                context.Result = new RedirectToActionResult("Index", "Home", null);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }

    // Restringe um controller/action a determinados perfis.
    // Exemplo: [RequerPerfil(Usuario.Administrador, Usuario.Manutencao)]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequerPerfilAttribute : Attribute
    {
        public int[] Perfis { get; }

        public RequerPerfilAttribute(params int[] perfis)
        {
            Perfis = perfis;
        }
    }
}
