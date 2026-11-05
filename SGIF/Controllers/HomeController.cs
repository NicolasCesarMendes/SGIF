using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGIF.Models;
using System.Diagnostics;

namespace SGIF.Controllers
{
    public class HomeController : Controller
    {
        // Landing Page: página pública de apresentação (única aberta sem login, RNF4)
        [AllowAnonymous]
        public IActionResult LandingPage()
        {
            return View("LandingPageView");
        }

        // Tela Inicial (Painel operacional)
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
