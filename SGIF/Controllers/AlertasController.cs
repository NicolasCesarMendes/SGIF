using Microsoft.AspNetCore.Mvc;
using SGIF.Classes;
using SGIF.Models;

namespace SGIF.Controllers
{
    // Fila de alertas (RF11, HU12). Todos os perfis consultam; só Administrador e Manutenção
    // mudam a situação (Aberto / Em análise / Resolvido). A troca acontece em modal:
    // sucesso = 200; erro = mensagem em texto.
    public class AlertasController : Controller
    {
        private const int TamanhoPagina = 15;

        private readonly Alertas _alertas;

        public AlertasController(Alertas alertas)
        {
            _alertas = alertas;
        }

        public IActionResult Index(string? status, int pagina = 1)
        {
            // Só aceita as situações conhecidas; qualquer outro valor = todos
            string? filtro = status != null && AlertasViewModel.Situacoes.Contains(status) ? status : null;

            int total = _alertas.ContarFila(filtro);
            int totalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)TamanhoPagina));
            pagina = Math.Clamp(pagina, 1, totalPaginas);

            AlertasViewModel vm = new AlertasViewModel
            {
                Status = filtro,
                Pagina = pagina,
                TotalPaginas = totalPaginas,
                TotalFiltrado = total,
                PodeAtualizar = PodeAtualizar(),
                Contagens = _alertas.ContarPorStatus()
            };

            foreach (AlertaDetalhe d in _alertas.ListarFila(filtro, pagina, TamanhoPagina))
            {
                vm.Itens.Add(AlertaItemViewModel.De(d));
            }

            return View(vm);
        }

        // Conteúdo do modal: detalhes do alerta (e a troca de situação, para quem pode)
        public IActionResult FormAlerta(int id)
        {
            AlertaDetalhe? d = _alertas.BuscarDetalhe(id);

            if (d == null)
            {
                return NotFound("Alerta não encontrado. Atualize a página.");
            }

            return PartialView("_FormAlerta", new AlertaFormViewModel
            {
                Alerta = AlertaItemViewModel.De(d),
                PodeAtualizar = PodeAtualizar()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequerPerfil(Usuario.Administrador, Usuario.Manutencao)]
        public IActionResult Salvar(int id, string status)
        {
            if (!AlertasViewModel.Situacoes.Contains(status))
            {
                return BadRequest("Situação inválida.");
            }

            Alerta? alerta = _alertas.BuscarPorId(id);

            if (alerta == null)
            {
                return NotFound("Alerta não encontrado. Atualize a página.");
            }

            // Mesma situação: nada a alterar (evita trocar o responsável e a data sem necessidade)
            if (alerta.Status == status)
            {
                return Ok();
            }

            int.TryParse(HttpContext.Session.GetString("idUsuario"), out int idUsuario);

            try
            {
                if (!_alertas.AlterarStatus(id, status, idUsuario))
                {
                    return NotFound("Alerta não encontrado. Atualize a página.");
                }

                TempData["Sucesso"] = "Alerta atualizado: " + AlertasViewModel.NomeSituacao(status) + ".";
                return Ok();
            }
            catch (Exception)
            {
                return StatusCode(500, "Não foi possível atualizar o alerta. Tente novamente.");
            }
        }

        // ---------- Auxiliares ----------

        private bool PodeAtualizar()
        {
            return int.TryParse(HttpContext.Session.GetString("nivelAcesso"), out int nivel)
                && (nivel == Usuario.Administrador || nivel == Usuario.Manutencao);
        }
    }
}
