using Microsoft.AspNetCore.Mvc;
using SGIF.Classes;
using SGIF.Models;

namespace SGIF.Controllers
{
    // Ficha do ativo: dados da locomotiva/vagão e seus sensores. Todos os perfis podem consultar.
    // Mostra também as últimas leituras de cada sensor e os alertas mais recentes do ativo.
    public class FichaController : Controller
    {
        private const int QuantidadeLeituras = 10;
        private const int QuantidadeAlertas = 10;

        private readonly Locomotivas _locomotivas;
        private readonly Vagoes _vagoes;
        private readonly Sensores _sensores;
        private readonly Leituras _leituras;
        private readonly Alertas _alertas;
        private readonly AnaliseSensores _analise;

        public FichaController(Locomotivas locomotivas, Vagoes vagoes, Sensores sensores, Leituras leituras, Alertas alertas,
                               AnaliseSensores analise)
        {
            _locomotivas = locomotivas;
            _vagoes = vagoes;
            _sensores = sensores;
            _leituras = leituras;
            _alertas = alertas;
            _analise = analise;
        }

        public IActionResult Index(string tipo, int id, string? periodo)
        {
            FichaViewModel vm = new FichaViewModel
            {
                Tipo = tipo == "Vagao" ? "Vagao" : "Locomotiva",
                Id = id,
                PodeEditar = PodeEditar()
            };

            // Período do histórico: só aceita os códigos conhecidos
            var escolhido = FichaViewModel.Periodos.FirstOrDefault(x => x.Codigo == periodo);
            if (escolhido.Codigo == null)
            {
                escolhido = FichaViewModel.Periodos.First(x => x.Codigo == FichaViewModel.PeriodoPadrao);
            }

            vm.Periodo = escolhido.Codigo;
            DateTime inicioPeriodo = DateTime.Now.AddHours(-escolhido.Horas);

            List<Sensor> sensores;

            if (vm.Tipo == "Locomotiva")
            {
                Locomotiva? l = _locomotivas.BuscarPorId(id);

                if (l == null)
                {
                    return AtivoNaoEncontrado();
                }

                vm.Numero = l.NumeroIdentificacao;
                vm.Estado = l.Status;
                vm.Detalhes.Add(("Modelo", l.Modelo));
                vm.Detalhes.Add(("Fabricante", l.Fabricante ?? "-"));
                vm.Detalhes.Add(("Ano de fabricação", l.AnoFabricacao?.ToString() ?? "-"));

                sensores = _sensores.ListarPorLocomotiva(id);
            }
            else
            {
                Vagao? v = _vagoes.BuscarPorId(id);

                if (v == null)
                {
                    return AtivoNaoEncontrado();
                }

                vm.Numero = v.NumeroIdentificacao;
                vm.Estado = v.Status;
                vm.Detalhes.Add(("Tipo", v.Tipo));
                vm.Detalhes.Add(("Capacidade", v.Capacidade.HasValue ? EntradaNumerica.Formatar(v.Capacidade) : "-"));

                sensores = _sensores.ListarPorVagao(id);
            }

            foreach (Sensor s in sensores)
            {
                string unidade = TiposSensor.Unidade(s.Tipo);
                Leitura? ultima = _leituras.BuscarUltimaPorSensor(s.IdSensor);

                SensorItemViewModel item = new SensorItemViewModel
                {
                    Id = s.IdSensor,
                    Ativo = s.Ativo,
                    Tipo = TiposSensor.Nome(s.Tipo),
                    Modelo = s.Modelo,
                    Faixa = FormatarFaixa(s.ValorMinimo, s.ValorMaximo, unidade),
                    LimiteAtencao = FormatarValor(s.LimiteAtencao, unidade),
                    LimiteCritico = FormatarValor(s.LimiteCritico, unidade),
                    UltimaLeitura = ultima != null ? FormatarValor(ultima.Valor, unidade) : "-",
                    UltimaLeituraEm = ultima?.DataHora.ToString("dd/MM/yyyy HH:mm:ss"),
                    NivelLeitura = ultima != null ? ProcessadorLeituras.NivelDoAlerta(s, ultima.Valor) ?? "" : ""
                };

                // Histórico recente: cada valor ganha a cor do nível que ele atingiria
                foreach (Leitura leitura in _leituras.ListarRecentesPorSensor(s.IdSensor, QuantidadeLeituras, inicioPeriodo))
                {
                    item.Leituras.Add(new LeituraItemViewModel
                    {
                        Valor = FormatarValor(leitura.Valor, unidade),
                        DataHora = leitura.DataHora.ToString("dd/MM/yyyy HH:mm:ss"),
                        Nivel = ProcessadorLeituras.NivelDoAlerta(s, leitura.Valor) ?? ""
                    });
                }

                // Estatísticas do período (calculadas no banco)
                LeituraResumo resumo = _leituras.ResumirPorSensor(s.IdSensor, inicioPeriodo);
                item.QuantidadeNoPeriodo = resumo.Quantidade;
                item.Minimo = FormatarValor(resumo.Minimo, unidade);
                item.Maximo = FormatarValor(resumo.Maximo, unidade);
                item.Media = FormatarValor(resumo.Media.HasValue ? Math.Round(resumo.Media.Value, 2) : null, unidade);

                vm.Sensores.Add(item);
            }

            foreach (AlertaDetalhe d in _alertas.ListarPorAtivo(vm.Tipo == "Locomotiva", id, QuantidadeAlertas))
            {
                vm.Alertas.Add(AlertaItemViewModel.De(d));
            }

            return View(vm);
        }

        // Dados dos gráficos da Ficha (JSON), um bloco por sensor: pontos do período, limites, tendência e
        // anomalias (RF8, RF9, RF10, RF13). A tela só desenha o que vem daqui.
        // Exemplo: /Ficha/Serie?tipo=Locomotiva&id=1&periodo=24h
        public IActionResult Serie(string tipo, int id, string? periodo)
        {
            bool locomotiva = tipo != "Vagao";

            bool existe = locomotiva ? _locomotivas.BuscarPorId(id) != null : _vagoes.BuscarPorId(id) != null;

            if (!existe)
            {
                return NotFound("Ativo não encontrado.");
            }

            var escolhido = AnaliseSensores.EscolherPeriodo(periodo);
            List<Sensor> sensores = locomotiva ? _sensores.ListarPorLocomotiva(id) : _sensores.ListarPorVagao(id);

            var dadosDosSensores = sensores.Select(s =>
            {
                List<PontoSerie> pontos = _analise.Serie(s.IdSensor, escolhido.Horas);
                TendenciaSensor tendencia = AnaliseSensores.CalcularTendencia(s, pontos);
                AnomaliasSensor anomalias = _analise.Anomalias(s.IdSensor, escolhido.Horas);

                return new
                {
                    id = s.IdSensor,
                    ativo = s.Ativo,
                    tipo = s.Tipo,
                    nome = TiposSensor.Nome(s.Tipo),
                    unidade = TiposSensor.Unidade(s.Tipo),
                    modelo = s.Modelo,
                    valorMinimo = s.ValorMinimo,
                    valorMaximo = s.ValorMaximo,
                    limiteAtencao = s.LimiteAtencao,
                    limiteCritico = s.LimiteCritico,
                    tendencia = new
                    {
                        direcao = tendencia.Direcao,
                        inclinacaoPorHora = tendencia.InclinacaoPorHora,
                        r2 = tendencia.R2,
                        horasAteAtencao = tendencia.HorasAteAtencao,
                        horasAteCritico = tendencia.HorasAteCritico
                    },
                    anomalias = new
                    {
                        quantidade = anomalias.Quantidade,
                        media = anomalias.Media,
                        desvio = anomalias.Desvio,
                        limiteInferior = anomalias.LimiteInferior,
                        limiteSuperior = anomalias.LimiteSuperior,
                        ultimas = anomalias.Ultimas.Select(l => new { dataHora = l.DataHora, valor = l.Valor }).ToList()
                    },
                    pontos = pontos.Select(p => new
                    {
                        inicio = p.Inicio,
                        media = p.Media,
                        minimo = p.Minimo,
                        maximo = p.Maximo,
                        quantidade = p.Quantidade
                    }).ToList()
                };
            }).ToList();

            return Json(new
            {
                periodo = escolhido.Codigo,
                minutosPorPonto = AnaliseSensores.MinutosPorPonto(escolhido.Horas),
                sensores = dadosDosSensores
            });
        }

        // ---------- Auxiliares ----------

        private IActionResult AtivoNaoEncontrado()
        {
            TempData["Erro"] = "Ativo não encontrado.";
            return RedirectToAction("Index", "Frota");
        }

        private bool PodeEditar()
        {
            return int.TryParse(HttpContext.Session.GetString("nivelAcesso"), out int nivel)
                && (nivel == Usuario.Administrador || nivel == Usuario.Manutencao);
        }

        private static string FormatarValor(decimal? valor, string unidade)
        {
            return valor.HasValue ? (EntradaNumerica.Formatar(valor) + " " + unidade).Trim() : "-";
        }

        private static string FormatarFaixa(decimal? minimo, decimal? maximo, string unidade)
        {
            if (!minimo.HasValue && !maximo.HasValue)
            {
                return "-";
            }

            string min = minimo.HasValue ? EntradaNumerica.Formatar(minimo) : "?";
            string max = maximo.HasValue ? EntradaNumerica.Formatar(maximo) : "?";

            return (min + " a " + max + " " + unidade).Trim();
        }
    }
}
