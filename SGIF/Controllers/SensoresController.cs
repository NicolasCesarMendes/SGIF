using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SGIF.Classes;
using SGIF.Models;

namespace SGIF.Controllers
{
    // Cadastro e gestão de sensores e associação aos ativos (RF5, RF6, RF16):
    // somente Administrador e Funcionário da Manutenção. A consulta fica na Ficha do ativo.
    // Tudo acontece em modais abertos na Ficha: Sucesso = 200; erro de validação = 422 com o formulário.
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

        // Formulário do modal. Novo: vindo da Ficha, o ativo já chega selecionado. Edição: informe o id do sensor.
        public IActionResult FormSensor(string? tipoAtivo, int? idAtivo, int? id)
        {
            SensorFormViewModel vm = new SensorFormViewModel();

            if (id.HasValue && id.Value > 0)
            {
                Sensor? sensor = _sensores.BuscarPorId(id.Value);

                if (sensor == null)
                {
                    return NotFound("Sensor não encontrado. Atualize a página.");
                }

                vm.IdSensor = sensor.IdSensor;
                vm.Tipo = sensor.Tipo;
                vm.Modelo = sensor.Modelo;
                vm.ValorMinimo = EntradaNumerica.Formatar(sensor.ValorMinimo);
                vm.ValorMaximo = EntradaNumerica.Formatar(sensor.ValorMaximo);
                vm.LimiteAtencao = EntradaNumerica.Formatar(sensor.LimiteAtencao);
                vm.LimiteCritico = EntradaNumerica.Formatar(sensor.LimiteCritico);
                vm.Ativo = sensor.IdLocomotiva.HasValue
                    ? "Locomotiva:" + sensor.IdLocomotiva.Value
                    : "Vagao:" + sensor.IdVagao;
            }
            else if (idAtivo.HasValue && (tipoAtivo == "Locomotiva" || tipoAtivo == "Vagao"))
            {
                vm.Ativo = tipoAtivo + ":" + idAtivo.Value;
            }

            CarregarOpcoes(vm);
            return PartialView("_FormSensor", vm);
        }

        // Salvar (novo ou edição; a edição inclui trocar o ativo ao qual o sensor está associado)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Salvar(SensorFormViewModel vm)
        {
            Sensor? sensor = MontarSensor(vm);

            if (sensor == null)
            {
                return FormComErros(vm);
            }

            try
            {
                if (vm.IdSensor > 0)
                {
                    if (!_sensores.Atualizar(sensor))
                    {
                        return ErroGeral(vm, "Sensor não encontrado. Atualize a página.");
                    }

                    TempData["Sucesso"] = "Sensor atualizado com sucesso.";
                }
                else
                {
                    _sensores.Inserir(sensor);

                    TempData["Sucesso"] = "Sensor cadastrado com sucesso.";
                }

                return Ok();
            }
            catch (Exception)
            {
                return ErroGeral(vm, "Não foi possível salvar. Tente novamente.");
            }
        }

        // Excluir (a confirmação é feita no modal). Sucesso = 200; erro = mensagem em texto.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Excluir(int id)
        {
            try
            {
                if (!_sensores.Excluir(id))
                {
                    return NotFound("Sensor não encontrado. Atualize a página.");
                }

                TempData["Sucesso"] = "Sensor excluído com sucesso.";
                return Ok();
            }
            catch (Exception)
            {
                // (as leituras e os alertas do sensor são apagados junto, dentro de uma transação)
                return StatusCode(500, "Não foi possível excluir. Tente novamente.");
            }
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

        // Devolve o formulário com as mensagens de erro (o JavaScript o coloca de volta no modal)
        private IActionResult FormComErros(SensorFormViewModel vm)
        {
            CarregarOpcoes(vm);
            Response.StatusCode = 422;
            return PartialView("_FormSensor", vm);
        }

        private IActionResult ErroGeral(SensorFormViewModel vm, string mensagem)
        {
            ModelState.AddModelError(string.Empty, mensagem);
            return FormComErros(vm);
        }
    }
}
