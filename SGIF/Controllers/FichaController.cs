using Microsoft.AspNetCore.Mvc;
using SGIF.Classes;
using SGIF.Models;

namespace SGIF.Controllers
{
    // Ficha do ativo: dados da locomotiva/vagão e seus sensores. Todos os perfis podem consultar.
    // (Gráficos e alertas do ativo entram nos próximos passos.)
    public class FichaController : Controller
    {
        private readonly Locomotivas _locomotivas;
        private readonly Vagoes _vagoes;
        private readonly Sensores _sensores;
        private readonly Leituras _leituras;

        public FichaController(Locomotivas locomotivas, Vagoes vagoes, Sensores sensores, Leituras leituras)
        {
            _locomotivas = locomotivas;
            _vagoes = vagoes;
            _sensores = sensores;
            _leituras = leituras;
        }

        public IActionResult Index(string tipo, int id)
        {
            FichaViewModel vm = new FichaViewModel
            {
                Tipo = tipo == "Vagao" ? "Vagao" : "Locomotiva",
                Id = id,
                PodeEditar = PodeEditar()
            };

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

                vm.Sensores.Add(new SensorItemViewModel
                {
                    Id = s.IdSensor,
                    Tipo = TiposSensor.Nome(s.Tipo),
                    Modelo = s.Modelo,
                    Faixa = FormatarFaixa(s.ValorMinimo, s.ValorMaximo, unidade),
                    LimiteAtencao = FormatarValor(s.LimiteAtencao, unidade),
                    LimiteCritico = FormatarValor(s.LimiteCritico, unidade),
                    UltimaLeitura = ultima != null ? FormatarValor(ultima.Valor, unidade) : "-",
                    UltimaLeituraEm = ultima?.DataHora.ToString("dd/MM/yyyy HH:mm:ss"),
                    NivelLeitura = ultima != null ? ProcessadorLeituras.NivelDoAlerta(s, ultima.Valor) ?? "" : ""
                });
            }

            return View(vm);
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
