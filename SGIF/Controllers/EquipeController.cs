using Microsoft.AspNetCore.Mvc;

namespace SGIF.Controllers
{
    // Página "Equipe": integrantes do projeto e contato (conteúdo a ser preenchido).
    // Todos os perfis acessam; o filtro de autenticação já exige login.
    public class EquipeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
