using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SGIF.Classes;
using SGIF.Models;

namespace SGIF.Controllers
{
    public class FrotaController : Controller
    {
        private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

        private readonly Locomotivas _locomotivas;
        private readonly Vagoes _vagoes;

        public FrotaController(Locomotivas locomotivas, Vagoes vagoes)
        {
            _locomotivas = locomotivas;
            _vagoes = vagoes;
        }

        // Lista da Frota (todos os perfis podem consultar)
        public IActionResult Index(string? tipo, string? busca, string? estado)
        {
            tipo = (tipo == "Locomotiva" || tipo == "Vagao") ? tipo : "Todos";
            estado = (estado != null && FrotaViewModel.Estados.Contains(estado)) ? estado : null;

            FrotaViewModel vm = new FrotaViewModel
            {
                Tipo = tipo,
                Busca = busca,
                Estado = estado,
                PodeEditar = PodeEditar()
            };

            if (tipo != "Vagao")
            {
                foreach (Locomotiva l in _locomotivas.Buscar(busca, estado))
                {
                    string[] partes =
                    {
                        l.Fabricante ?? "",
                        l.AnoFabricacao?.ToString() ?? ""
                    };

                    vm.Itens.Add(new AtivoItemViewModel
                    {
                        Tipo = "Locomotiva",
                        Id = l.IdLocomotiva,
                        Numero = l.NumeroIdentificacao,
                        Descricao = l.Modelo,
                        Detalhes = string.Join(" · ", partes.Where(p => p != "")),
                        Estado = l.Status
                    });
                }
            }

            if (tipo != "Locomotiva")
            {
                foreach (Vagao v in _vagoes.Buscar(busca, estado))
                {
                    vm.Itens.Add(new AtivoItemViewModel
                    {
                        Tipo = "Vagao",
                        Id = v.IdVagao,
                        Numero = v.NumeroIdentificacao,
                        Descricao = v.Tipo,
                        Detalhes = v.Capacidade.HasValue
                            ? "Capacidade: " + v.Capacidade.Value.ToString("0.##", PtBr)
                            : "",
                        Estado = v.Status
                    });
                }
            }

            vm.Itens = vm.Itens.OrderBy(i => i.Tipo).ThenBy(i => i.Numero).ToList();

            return View(vm);
        }

        // Formulário do modal (novo ou edição). Devolve só o HTML do formulário.
        [RequerPerfil(Usuario.Administrador, Usuario.Manutencao)]
        public IActionResult FormAtivo(string? tipo, int? id)
        {
            AtivoFormViewModel vm = new AtivoFormViewModel
            {
                Tipo = tipo == "Vagao" ? "Vagao" : "Locomotiva"
            };

            if (id.HasValue && id.Value > 0)
            {
                if (vm.Tipo == "Locomotiva")
                {
                    Locomotiva? l = _locomotivas.BuscarPorId(id.Value);
                    if (l == null) return NotFound();

                    vm.Id = l.IdLocomotiva;
                    vm.NumeroIdentificacao = l.NumeroIdentificacao;
                    vm.Modelo = l.Modelo;
                    vm.Fabricante = l.Fabricante;
                    vm.AnoFabricacao = l.AnoFabricacao;
                    vm.Estado = l.Status;
                }
                else
                {
                    Vagao? v = _vagoes.BuscarPorId(id.Value);
                    if (v == null) return NotFound();

                    vm.Id = v.IdVagao;
                    vm.NumeroIdentificacao = v.NumeroIdentificacao;
                    vm.TipoVagao = v.Tipo;
                    vm.Capacidade = v.Capacidade?.ToString("0.##", PtBr);
                    vm.Estado = v.Status;
                }
            }

            return PartialView("_FormAtivo", vm);
        }

        // Salvar (novo ou edição). Sucesso = 200; erro de validação = 422 com o formulário e as mensagens.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequerPerfil(Usuario.Administrador, Usuario.Manutencao)]
        public IActionResult Salvar(AtivoFormViewModel vm)
        {
            ValidarPorTipo(vm);

            if (!ModelState.IsValid)
            {
                return FormComErros(vm);
            }

            try
            {
                bool salvou;

                if (vm.Tipo == "Locomotiva")
                {
                    Locomotiva? l = vm.Id > 0 ? _locomotivas.BuscarPorId(vm.Id) : new Locomotiva();
                    if (l == null) return ErroGeral(vm, "Locomotiva não encontrada.");

                    l.NumeroIdentificacao = vm.NumeroIdentificacao.Trim();
                    l.Modelo = vm.Modelo!.Trim();
                    l.Fabricante = string.IsNullOrWhiteSpace(vm.Fabricante) ? null : vm.Fabricante.Trim();
                    l.AnoFabricacao = vm.AnoFabricacao;
                    l.Status = vm.Estado;

                    if (vm.Id > 0)
                    {
                        salvou = _locomotivas.Atualizar(l);
                    }
                    else
                    {
                        _locomotivas.Inserir(l);
                        salvou = true;
                    }
                }
                else
                {
                    Vagao? v = vm.Id > 0 ? _vagoes.BuscarPorId(vm.Id) : new Vagao();
                    if (v == null) return ErroGeral(vm, "Vagão não encontrado.");

                    TentarLerCapacidade(vm.Capacidade, out decimal? capacidade);

                    v.NumeroIdentificacao = vm.NumeroIdentificacao.Trim();
                    v.Tipo = vm.TipoVagao!.Trim();
                    v.Capacidade = capacidade;
                    v.Status = vm.Estado;

                    if (vm.Id > 0)
                    {
                        salvou = _vagoes.Atualizar(v);
                    }
                    else
                    {
                        _vagoes.Inserir(v);
                        salvou = true;
                    }
                }

                return salvou ? Ok() : ErroGeral(vm, "O ativo não foi encontrado. Atualize a página.");
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                // UNIQUE do banco: número de identificação repetido
                ModelState.AddModelError(nameof(vm.NumeroIdentificacao), "Já existe um ativo com este número.");
                return FormComErros(vm);
            }
            catch (Exception)
            {
                return ErroGeral(vm, "Não foi possível salvar. Tente novamente.");
            }
        }

        // Excluir (a confirmação é feita no modal). Sucesso = 200; erro = mensagem em texto.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequerPerfil(Usuario.Administrador, Usuario.Manutencao)]
        public IActionResult Excluir(string tipo, int id)
        {
            try
            {
                bool excluiu = tipo == "Vagao" ? _vagoes.Excluir(id) : _locomotivas.Excluir(id);

                return excluiu ? Ok() : NotFound("O ativo não foi encontrado. Atualize a página.");
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                // Chave estrangeira: ainda existem sensores ligados a este ativo
                return Conflict("Não é possível excluir: este ativo possui sensores associados. Remova os sensores antes.");
            }
            catch (Exception)
            {
                return StatusCode(500, "Não foi possível excluir. Tente novamente.");
            }
        }

        // ---------- Auxiliares ----------

        private bool PodeEditar()
        {
            return int.TryParse(HttpContext.Session.GetString("nivelAcesso"), out int nivel)
                && (nivel == Usuario.Administrador || nivel == Usuario.Manutencao);
        }

        // Validações que dependem do tipo do ativo
        private void ValidarPorTipo(AtivoFormViewModel vm)
        {
            if (!FrotaViewModel.Estados.Contains(vm.Estado))
            {
                ModelState.AddModelError(nameof(vm.Estado), "Estado inválido");
            }

            if (vm.Tipo == "Locomotiva")
            {
                if (string.IsNullOrWhiteSpace(vm.Modelo))
                {
                    ModelState.AddModelError(nameof(vm.Modelo), "Informe o modelo");
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(vm.TipoVagao))
                {
                    ModelState.AddModelError(nameof(vm.TipoVagao), "Informe o tipo do vagão");
                }

                if (!TentarLerCapacidade(vm.Capacidade, out _))
                {
                    ModelState.AddModelError(nameof(vm.Capacidade), "Capacidade inválida (exemplo: 50,5)");
                }
            }
        }

        // Aceita vírgula ou ponto. Vazio = sem capacidade (válido).
        private static bool TentarLerCapacidade(string? texto, out decimal? valor)
        {
            valor = null;

            if (string.IsNullOrWhiteSpace(texto))
            {
                return true;
            }

            string normalizado = texto.Trim().Replace(',', '.');

            if (decimal.TryParse(normalizado, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal d)
                && d >= 0 && d <= 99999999.99m)
            {
                valor = Math.Round(d, 2);
                return true;
            }

            return false;
        }

        private IActionResult FormComErros(AtivoFormViewModel vm)
        {
            Response.StatusCode = 422;
            return PartialView("_FormAtivo", vm);
        }

        private IActionResult ErroGeral(AtivoFormViewModel vm, string mensagem)
        {
            ModelState.AddModelError("", mensagem);
            return FormComErros(vm);
        }
    }
}
