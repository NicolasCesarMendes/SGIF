namespace SGIF.Models
{
    // Ficha do ativo (locomotiva ou vagão): dados do ativo e seus sensores
    public class FichaViewModel
    {
        public string Tipo { get; set; } = "Locomotiva";     // Locomotiva ou Vagao
        public int Id { get; set; }
        public string Numero { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public bool PodeEditar { get; set; }                  // Administrador e Manutenção

        public List<(string Rotulo, string Valor)> Detalhes { get; set; } = new();
        public List<SensorItemViewModel> Sensores { get; set; } = new();

        public string NomeTipo => Tipo == "Vagao" ? "Vagão" : "Locomotiva";
    }

    // Uma linha da tabela de sensores da ficha (textos já prontos para exibir)
    public class SensorItemViewModel
    {
        public int Id { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string Faixa { get; set; } = string.Empty;
        public string LimiteAtencao { get; set; } = string.Empty;
        public string LimiteCritico { get; set; } = string.Empty;
    }
}
