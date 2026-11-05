using System.ComponentModel.DataAnnotations;

namespace SGIF.Models
{
    // Dados da página Frota (lista, filtros e permissões)
    public class FrotaViewModel
    {
        public string Tipo { get; set; } = "Todos";        // Todos, Locomotiva ou Vagao
        public string? Busca { get; set; }
        public string? Estado { get; set; }
        public bool PodeEditar { get; set; }                // Administrador e Manutenção
        public List<AtivoItemViewModel> Itens { get; set; } = new();

        // Estados possíveis de um ativo (valores gravados no banco, sem acento)
        public static readonly string[] Estados = { "Ativo", "Em manutencao", "Inativo" };

        public static string NomeEstado(string estado)
        {
            return estado == "Em manutencao" ? "Em manutenção" : estado;
        }
    }

    // Uma linha da lista (locomotiva ou vagão)
    public class AtivoItemViewModel
    {
        public string Tipo { get; set; } = string.Empty;        // Locomotiva ou Vagao
        public int Id { get; set; }
        public string Numero { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;   // modelo (locomotiva) ou tipo (vagão)
        public string Detalhes { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
    }

    // Formulário de cadastro/edição (aberto em modal)
    public class AtivoFormViewModel
    {
        public string Tipo { get; set; } = "Locomotiva";        // Locomotiva ou Vagao
        public int Id { get; set; }                              // 0 = novo

        [Required(ErrorMessage = "Informe o número de identificação")]
        [StringLength(50, ErrorMessage = "Máximo de 50 caracteres")]
        public string NumeroIdentificacao { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe o estado")]
        public string Estado { get; set; } = "Ativo";

        // Campos da locomotiva
        [StringLength(100, ErrorMessage = "Máximo de 100 caracteres")]
        public string? Modelo { get; set; }

        [StringLength(100, ErrorMessage = "Máximo de 100 caracteres")]
        public string? Fabricante { get; set; }

        [Range(1800, 2100, ErrorMessage = "Informe um ano válido")]
        public int? AnoFabricacao { get; set; }

        // Campos do vagão
        [StringLength(100, ErrorMessage = "Máximo de 100 caracteres")]
        public string? TipoVagao { get; set; }

        // Texto (aceita vírgula ou ponto): convertido para decimal no controller
        public string? Capacidade { get; set; }
    }
}
