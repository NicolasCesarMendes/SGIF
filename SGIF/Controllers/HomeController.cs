using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGIF.Classes;
using SGIF.Models;
using System.Diagnostics;

namespace SGIF.Controllers
{
    public class HomeController : Controller
    {
        private readonly Locomotivas _locomotivas;
        private readonly Vagoes _vagoes;
        private readonly Alertas _alertas;

        public HomeController(Locomotivas locomotivas, Vagoes vagoes, Alertas alertas)
        {
            _locomotivas = locomotivas;
            _vagoes = vagoes;
            _alertas = alertas;
        }

        // Landing Page: página pública de apresentação (única aberta sem login, RNF4)
        [AllowAnonymous]
        public IActionResult LandingPage()
        {
            return View("LandingPageView");
        }

        // Tela Inicial (Painel operacional)
        public IActionResult Index()
        {
            List<Locomotiva> locomotivas = _locomotivas.Buscar(null, null);
            List<Vagao> vagoes = _vagoes.Buscar(null, null);
            Dictionary<string, int> abertos = _alertas.ContarNaoResolvidosPorNivel();

            HomeViewModel vm = new HomeViewModel
            {
                Nome = HttpContext.Session.GetString("nome") ?? "",
                TotalLocomotivas = locomotivas.Count,
                TotalVagoes = vagoes.Count,
                EmManutencao = locomotivas.Count(l => l.Status == "Em manutencao") + vagoes.Count(v => v.Status == "Em manutencao"),
                AlertasCriticos = abertos.GetValueOrDefault("Critico"),
                AlertasAtencao = abertos.GetValueOrDefault("Atencao")
            };

            foreach (AtivoAtencao a in _alertas.ListarAtivosEmAtencao(5))
            {
                bool locomotiva = a.IdLocomotiva.HasValue;

                vm.AtivosEmAtencao.Add(new AtivoAtencaoItemViewModel
                {
                    TipoAtivo = locomotiva ? "Locomotiva" : "Vagao",
                    IdAtivo = locomotiva ? a.IdLocomotiva!.Value : a.IdVagao ?? 0,
                    Ativo = locomotiva ? "Locomotiva " + a.NumeroLocomotiva : "Vagão " + a.NumeroVagao,
                    AlertasAbertos = a.AlertasAbertos,
                    Criticos = a.Criticos
                });
            }

            foreach (AlertaDetalhe d in _alertas.ListarRecentes(5))
            {
                vm.UltimosAlertas.Add(AlertaItemViewModel.De(d));
            }

            return View(vm);
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
