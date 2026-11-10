using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using SGIF.Classes;
using SGIF.Models;

namespace SGIF.Controllers
{
    // Plano de intervenções ordenado pelo risco de falha (RF10, HU2, HU11). Todos os perfis consultam.
    // O cálculo percorre todos os sensores, então o resultado fica guardado por poucos segundos: o bastante
    // para vários usuários abrindo a tela ao mesmo tempo não repetirem a conta (RNF3), e curto para a tela
    // acompanhar o simulador (uma leitura a cada 10 s) quase em tempo real.
    public class ManutencaoController : Controller
    {
        private const string ChaveDoCache = "ranking-de-risco";
        private const int ValidadeDoCacheEmSegundos = 10;
        private static readonly TimeSpan Validade = TimeSpan.FromSeconds(ValidadeDoCacheEmSegundos);

        private readonly AnaliseSensores _analise;
        private readonly IMemoryCache _cache;

        public ManutencaoController(AnaliseSensores analise, IMemoryCache cache)
        {
            _analise = analise;
            _cache = cache;
        }

        // Página (a tela é feita pelo front-end a partir do ManutencaoViewModel)
        public IActionResult Index()
        {
            return View(MontarViewModel());
        }

        // Os mesmos dados em JSON (para testar e para a tela atualizar sem recarregar)
        public IActionResult Dados()
        {
            return Json(MontarViewModel());
        }

        private ManutencaoViewModel MontarViewModel()
        {
            return _cache.GetOrCreate(ChaveDoCache, entrada =>
            {
                entrada.AbsoluteExpirationRelativeToNow = Validade;

                return new ManutencaoViewModel
                {
                    Itens = _analise.RankingDeRisco(),
                    GeradoEm = DateTime.Now,
                    HorasDaAnalise = AnaliseSensores.HorasDaAnaliseDeRisco,
                    SegundosParaAtualizar = ValidadeDoCacheEmSegundos
                };
            })!;
        }
    }
}
