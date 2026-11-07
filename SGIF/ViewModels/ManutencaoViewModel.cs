namespace SGIF.Models
{
    // Página Manutenção: plano de intervenções ordenado pelo risco de falha (RF10)
    public class ManutencaoViewModel
    {
        public List<RiscoAtivo> Itens { get; set; } = new();
        public DateTime GeradoEm { get; set; }
        public int HorasDaAnalise { get; set; }       // janela dos dados usados no cálculo
    }
}
