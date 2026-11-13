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
        private readonly AnaliseSensores _analise;

        public HomeController(Locomotivas locomotivas, Vagoes vagoes, Alertas alertas, AnaliseSensores analise)
        {
            _locomotivas = locomotivas;
            _vagoes = vagoes;
            _alertas = alertas;
            _analise = analise;
        }

        // Landing Page: página pública de apresentação (única aberta sem login, RNF4)
        [AllowAnonymous]
        public IActionResult LandingPageExibir()
        {
            return View("LandingPageExibirView");
        }

        // Tela Inicial (Painel operacional)
        public IActionResult PainelOperacionalExibir()
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

            return View("PainelOperacionalExibirView", vm);
        }

        // Dados dos gráficos do painel (JSON): média e máximo da frota por tipo de sensor (RF9).
        // Exemplo: /Home/Serie?periodo=24h
        public IActionResult Serie(string? periodo)
        {
            var escolhido = AnaliseSensores.EscolherPeriodo(periodo);

            var series = TiposSensor.Todos.Select(tipo => new
            {
                tipo,
                nome = TiposSensor.Nome(tipo),
                unidade = TiposSensor.Unidade(tipo),
                pontos = _analise.SerieDaFrota(tipo, escolhido.Horas).Select(p => new
                {
                    inicio = p.Inicio,
                    media = p.Media,
                    minimo = p.Minimo,
                    maximo = p.Maximo,
                    quantidade = p.Quantidade
                }).ToList()
            }).ToList();

            return Json(new
            {
                periodo = escolhido.Codigo,
                minutosPorPonto = AnaliseSensores.MinutosPorPonto(escolhido.Horas),
                series
            });
        }

        // Página "Sobre": informações do projeto (conteúdo a ser preenchido).
        // Todos os perfis acessam; o filtro de autenticação já exige login.
        public IActionResult SobreExibir()
        {
            return View("SobreExibirView");
        }

        // Página "Equipe": integrantes do projeto e contato (conteúdo a ser preenchido).
        // Todos os perfis acessam; o filtro de autenticação já exige login.
        public IActionResult EquipeExibir()
        {
            return View("EquipeExibirView");
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
