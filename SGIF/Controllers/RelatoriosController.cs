using Microsoft.AspNetCore.Mvc;
using SGIF.Classes;
using SGIF.Models;

namespace SGIF.Controllers
{
    // Estatísticas e comparativos mensais (RF12), com filtros por ativo, tipo e período (RF13).
    // Todos os perfis consultam.
    // Exemplos: /Relatorios/Dados?periodo=30d
    //           /Relatorios/Dados?tipoAtivo=Locomotiva&idAtivo=1&tipoSensor=Temperatura&periodo=90d&meses=6
    public class RelatoriosController : Controller
    {
        private readonly Leituras _leituras;
        private readonly Alertas _alertas;
        private readonly Locomotivas _locomotivas;
        private readonly Vagoes _vagoes;

        public RelatoriosController(Leituras leituras, Alertas alertas, Locomotivas locomotivas, Vagoes vagoes)
        {
            _leituras = leituras;
            _alertas = alertas;
            _locomotivas = locomotivas;
            _vagoes = vagoes;
        }

        // Página (a tela é feita pelo front-end a partir do RelatorioViewModel)
        public IActionResult Index(string? tipoAtivo, int? idAtivo, string? tipoSensor, string? periodo, int meses = 6)
        {
            return View(Montar(tipoAtivo, idAtivo, tipoSensor, periodo, meses));
        }

        // Os mesmos dados em JSON
        public IActionResult Dados(string? tipoAtivo, int? idAtivo, string? tipoSensor, string? periodo, int meses = 6)
        {
            return Json(Montar(tipoAtivo, idAtivo, tipoSensor, periodo, meses));
        }

        private RelatorioViewModel Montar(string? tipoAtivo, int? idAtivo, string? tipoSensor, string? periodo, int meses)
        {
            // Só aceita valores conhecidos; qualquer outro vira "sem filtro"
            FiltroAnalise filtro = new FiltroAnalise
            {
                TipoAtivo = (tipoAtivo == "Locomotiva" || tipoAtivo == "Vagao") ? tipoAtivo : null,
                TipoSensor = (tipoSensor != null && TiposSensor.Todos.Contains(tipoSensor)) ? tipoSensor : null
            };

            // O id do ativo só vale junto com o tipo (os ids de locomotivas e vagões se repetem)
            filtro.IdAtivo = (filtro.TipoAtivo != null && idAtivo > 0) ? idAtivo : null;

            var escolhido = RelatorioViewModel.Periodos.FirstOrDefault(p => p.Codigo == periodo);

            if (escolhido.Codigo == null)
            {
                escolhido = RelatorioViewModel.Periodos.First(p => p.Codigo == RelatorioViewModel.PeriodoPadrao);
            }

            meses = Math.Clamp(meses, 2, 12);

            DateTime agora = DateTime.Now;
            DateTime inicio = agora.AddDays(-escolhido.Dias);

            return new RelatorioViewModel
            {
                TipoAtivo = filtro.TipoAtivo,
                IdAtivo = filtro.IdAtivo,
                TipoSensor = filtro.TipoSensor,
                Periodo = escolhido.Codigo,
                Meses = meses,
                GeradoEm = agora,
                Ativos = ListarOpcoesDeAtivos(),
                Estatisticas = _leituras.EstatisticasPorTipo(filtro, inicio),
                Alertas = _alertas.ResumirPeriodo(filtro, inicio),
                ComparativoMensal = MontarComparativo(filtro, meses, agora)
            };
        }

        // Locomotivas e vagões para o filtro "ativo específico"
        private List<OpcaoDeAtivo> ListarOpcoesDeAtivos()
        {
            List<OpcaoDeAtivo> opcoes = new List<OpcaoDeAtivo>();

            foreach (Locomotiva l in _locomotivas.ListarTodos())
            {
                opcoes.Add(new OpcaoDeAtivo
                {
                    TipoAtivo = "Locomotiva",
                    IdAtivo = l.IdLocomotiva,
                    Texto = "Locomotiva " + l.NumeroIdentificacao + " - " + l.Modelo
                });
            }

            foreach (Vagao v in _vagoes.ListarTodos())
            {
                opcoes.Add(new OpcaoDeAtivo
                {
                    TipoAtivo = "Vagao",
                    IdAtivo = v.IdVagao,
                    Texto = "Vagão " + v.NumeroIdentificacao + " - " + v.Tipo
                });
            }

            return opcoes;
        }

        // Um item por mês (os últimos N meses, incluindo o atual), com a variação da média em relação
        // ao último mês que teve dados do mesmo tipo de sensor.
        private List<ComparativoMes> MontarComparativo(FiltroAnalise filtro, int meses, DateTime agora)
        {
            DateTime primeiroMes = new DateTime(agora.Year, agora.Month, 1).AddMonths(-(meses - 1));

            List<ResumoMensal> leituras = _leituras.ResumirPorMes(filtro, primeiroMes);
            var alertas = _alertas.ContarPorMes(filtro, primeiroMes);

            List<ComparativoMes> resultado = new List<ComparativoMes>();
            Dictionary<string, decimal> mediaAnterior = new Dictionary<string, decimal>();

            for (int i = 0; i < meses; i++)
            {
                DateTime mes = primeiroMes.AddMonths(i);

                ComparativoMes item = new ComparativoMes
                {
                    Ano = mes.Year,
                    Mes = mes.Month,
                    Rotulo = mes.Month.ToString("00") + "/" + mes.Year
                };

                foreach (ResumoMensal r in leituras.Where(x => x.Ano == mes.Year && x.Mes == mes.Month))
                {
                    decimal? variacao = null;

                    if (mediaAnterior.TryGetValue(r.TipoSensor, out decimal anterior) && anterior != 0)
                    {
                        variacao = Math.Round((r.Media - anterior) / anterior * 100, 1);
                    }

                    item.Tipos.Add(new ComparativoTipo
                    {
                        TipoSensor = r.TipoSensor,
                        Quantidade = r.Quantidade,
                        Media = Math.Round(r.Media, 2),
                        Maximo = r.Maximo,
                        VariacaoMediaPercentual = variacao
                    });

                    mediaAnterior[r.TipoSensor] = r.Media;
                }

                item.AlertasCriticos = alertas
                    .Where(a => a.Ano == mes.Year && a.Mes == mes.Month && a.Nivel == "Critico")
                    .Sum(a => a.Total);

                item.AlertasAtencao = alertas
                    .Where(a => a.Ano == mes.Year && a.Mes == mes.Month && a.Nivel == "Atencao")
                    .Sum(a => a.Total);

                resultado.Add(item);
            }

            return resultado;
        }
    }
}
