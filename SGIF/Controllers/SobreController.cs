using Microsoft.AspNetCore.Mvc;

namespace SGIF.Controllers
{
    // Página "Sobre": informações do projeto (conteúdo a ser preenchido).
    // Todos os perfis acessam; o filtro de autenticação já exige login.
    public class SobreController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
