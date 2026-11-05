using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SGIF.Classes;
using SGIF.Models;

namespace SGIF.Controllers
{
    // Cadastro e gestão de sensores e associação aos ativos (RF5, RF6, RF16):
    // somente Administrador e Funcionário da Manutenção. A consulta fica na Ficha do ativo.
    [RequerPerfil(Usuario.Administrador, Usuario.Manutencao)]
    public class SensoresController : Controller
    {
        private readonly Sensores _sensores;
        private readonly Locomotivas _locomotivas;
        private readonly Vagoes _vagoes;

        public SensoresController(Sensores sensores, Locomotivas locomotivas, Vagoes vagoes)
        {
            _sensores = sensores;
            _locomotivas = locomotivas;
            _vagoes = vagoes;
        }

        // Criar. Vindo da Ficha, o ativo já chega selecionado.
        public IActionResult CriarExibir(string? tipoAtivo, int? idAtivo)
        {
            SensorFormViewModel vm = new SensorFormViewModel();

            if (idAtivo.HasValue && (tipoAtivo == "Locomotiva" || tipoAtivo == "Vagao"))
            {
                vm.Ativo = tipoAtivo + ":" + idAtivo.Value;
            }

            CarregarOpcoes(vm);
            return View("Form", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CriarProcessar(SensorFormViewModel vm)
        {
            Sensor? sensor = MontarSensor(vm);

            if (sensor == null)
            {
                CarregarOpcoes(vm);
                return View("Form", vm);
            }

            try
            {
                _sensores.Inserir(sensor);

                TempData["Sucesso"] = "Sensor cadastrado com sucesso.";
                return IrParaFicha(sensor);
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Não foi possível salvar. Tente novamente.");
                CarregarOpcoes(vm);
                return View("Form", vm);
            }
        }

        // Editar (inclui trocar o ativo ao qual o sensor está associado)
        public IActionResult EditarExibir(int id)
        {
            Sensor? sensor = _sensores.BuscarPorId(id);

            if (sensor == null)
            {
                TempData["Erro"] = "Sensor não encontrado.";
                return RedirectToAction("Index", "Frota");
            }

            SensorFormViewModel vm = new SensorFormViewModel
            {
                IdSensor = sensor.IdSensor,
                Tipo = sensor.Tipo,
                Modelo = sensor.Modelo,
                ValorMinimo = EntradaNumerica.Formatar(sensor.ValorMinimo),
                ValorMaximo = EntradaNumerica.Formatar(sensor.ValorMaximo),
                LimiteAtencao = EntradaNumerica.Formatar(sensor.LimiteAtencao),
                LimiteCritico = EntradaNumerica.Formatar(sensor.LimiteCritico),
                Ativo = sensor.IdLocomotiva.HasValue
                    ? "Locomotiva:" + sensor.IdLocomotiva.Value
                    : "Vagao:" + sensor.IdVagao
            };

            CarregarOpcoes(vm);
            return View("Form", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditarProcessar(SensorFormViewModel vm)
        {
            Sensor? sensor = MontarSensor(vm);

            if (sensor == null)
            {
                CarregarOpcoes(vm);
                return View("Form", vm);
            }

            try
            {
                if (!_sensores.Atualizar(sensor))
                {
                    TempData["Erro"] = "Sensor não encontrado.";
                    return RedirectToAction("Index", "Frota");
                }

                TempData["Sucesso"] = "Sensor atualizado com sucesso.";
                return IrParaFicha(sensor);
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Não foi possível salvar. Tente novamente.");
                CarregarOpcoes(vm);
                return View("Form", vm);
            }
        }

        // Excluir (a confirmação é feita na tela, antes de enviar)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ExcluirProcessar(int id)
        {
            Sensor? sensor = _sensores.BuscarPorId(id);

            if (sensor == null)
            {
                TempData["Erro"] = "Sensor não encontrado.";
                return RedirectToAction("Index", "Frota");
            }

            try
            {
                _sensores.Excluir(id);
                TempData["Sucesso"] = "Sensor excluído com sucesso.";
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                // Chave estrangeira: o sensor já tem leituras registradas
                TempData["Erro"] = "Este sensor possui leituras registradas e não pode ser excluído.";
            }
            catch (Exception)
            {
                TempData["Erro"] = "Não foi possível excluir. Tente novamente.";
            }

            return IrParaFicha(sensor);
        }

        // ---------- Auxiliares ----------

        // Valida o formulário e devolve o Sensor pronto para gravar (ou null, com os erros no ModelState)
        private Sensor? MontarSensor(SensorFormViewModel vm)
        {
            if (!string.IsNullOrEmpty(vm.Tipo) && !TiposSensor.Todos.Contains(vm.Tipo))
            {
                ModelState.AddModelError(nameof(vm.Tipo), "Tipo inválido");
            }

            if (!EntradaNumerica.TentarLerDecimal(vm.ValorMinimo, out decimal? minimo))
            {
                ModelState.AddModelError(nameof(vm.ValorMinimo), "Valor inválido (exemplo: 12,5)");
            }

            if (!EntradaNumerica.TentarLerDecimal(vm.ValorMaximo, out decimal? maximo))
            {
                ModelState.AddModelError(nameof(vm.ValorMaximo), "Valor inválido (exemplo: 12,5)");
            }

            if (!EntradaNumerica.TentarLerDecimal(vm.LimiteAtencao, out decimal? atencao))
            {
                ModelState.AddModelError(nameof(vm.LimiteAtencao), "Valor inválido (exemplo: 12,5)");
            }

            if (!EntradaNumerica.TentarLerDecimal(vm.LimiteCritico, out decimal? critico))
            {
                ModelState.AddModelError(nameof(vm.LimiteCritico), "Valor inválido (exemplo: 12,5)");
            }

            if (minimo.HasValue && maximo.HasValue && minimo >= maximo)
            {
                ModelState.AddModelError(nameof(vm.ValorMaximo), "O valor máximo deve ser maior que o mínimo");
            }

            // O banco exige limite de atenção menor que o crítico (ck_sensores_limites)
            if (atencao.HasValue && critico.HasValue && atencao >= critico)
            {
                ModelState.AddModelError(nameof(vm.LimiteCritico), "O limite crítico deve ser maior que o de atenção");
            }

            // O sensor pertence a UMA locomotiva OU a UM vagão (ck_sensores_um_ativo)
            int? idLocomotiva = null;
            int? idVagao = null;
            string[] partes = (vm.Ativo ?? "").Split(':');

            if (partes.Length == 2 && int.TryParse(partes[1], out int idAtivo))
            {
                if (partes[0] == "Locomotiva" && _locomotivas.BuscarPorId(idAtivo) != null)
                {
                    idLocomotiva = idAtivo;
                }
                else if (partes[0] == "Vagao" && _vagoes.BuscarPorId(idAtivo) != null)
                {
                    idVagao = idAtivo;
                }
            }

            // Só acrescenta a mensagem se o campo ainda não tiver erro (ex.: "Selecione o ativo")
            bool ativoJaTemErro = ModelState.TryGetValue(nameof(vm.Ativo), out var entradaAtivo)
                && entradaAtivo.Errors.Count > 0;

            if (idLocomotiva == null && idVagao == null && !ativoJaTemErro)
            {
                ModelState.AddModelError(nameof(vm.Ativo), "Selecione um ativo válido");
            }

            if (!ModelState.IsValid)
            {
                return null;
            }

            return new Sensor
            {
                IdSensor = vm.IdSensor,
                Tipo = vm.Tipo!,
                Modelo = vm.Modelo.Trim(),
                ValorMinimo = minimo,
                ValorMaximo = maximo,
                LimiteAtencao = atencao,
                LimiteCritico = critico,
                IdLocomotiva = idLocomotiva,
                IdVagao = idVagao
            };
        }

        // Preenche as opções do select de ativos
        private void CarregarOpcoes(SensorFormViewModel vm)
        {
            vm.Locomotivas = _locomotivas.ListarTodos()
                .Select(l => new OpcaoAtivo
                {
                    Valor = "Locomotiva:" + l.IdLocomotiva,
                    Texto = l.NumeroIdentificacao + " - " + l.Modelo
                })
                .ToList();

            vm.Vagoes = _vagoes.ListarTodos()
                .Select(v => new OpcaoAtivo
                {
                    Valor = "Vagao:" + v.IdVagao,
                    Texto = v.NumeroIdentificacao + " - " + v.Tipo
                })
                .ToList();
        }

        private IActionResult IrParaFicha(Sensor sensor)
        {
            if (sensor.IdLocomotiva.HasValue)
            {
                return RedirectToAction("Index", "Ficha", new { tipo = "Locomotiva", id = sensor.IdLocomotiva.Value });
            }

            if (sensor.IdVagao.HasValue)
            {
                return RedirectToAction("Index", "Ficha", new { tipo = "Vagao", id = sensor.IdVagao.Value });
            }

            return RedirectToAction("Index", "Frota");
        }
    }
}
