namespace SGIF.Classes
{
    public partial class Vagoes
    {
        //-----------------------------
        // Atributos
        //-----------------------------
        private int _idVagao;
        private string _numeroIdentificacao = string.Empty;
        private string _tipo = string.Empty;
        private decimal? _capacidade;
        private string _status = "Ativo";
        private int? _idTrem;

        //-----------------------------
        // Getters e Setters
        //-----------------------------
        public int IdVagao
        {
            get { return _idVagao; }
            set { _idVagao = value; }
        }

        public string NumeroIdentificacao
        {
            get { return _numeroIdentificacao; }
            set { _numeroIdentificacao = value; }
        }

        public string Tipo
        {
            get { return _tipo; }
            set { _tipo = value; }
        }

        public decimal? Capacidade
        {
            get { return _capacidade; }
            set { _capacidade = value; }
        }

        public string Status
        {
            get { return _status; }
            set { _status = value; }
        }

        public int? IdTrem
        {
            get { return _idTrem; }
            set { _idTrem = value; }
        }
    }
}
