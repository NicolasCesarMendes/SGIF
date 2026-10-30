namespace SGIF.Classes
{
    public partial class Usuarios
    {
        //-----------------------------
        // Atributos
        //-----------------------------
        private int _idUsuario;
        private string _nome = string.Empty;
        private string _email = string.Empty;
        private string _senha = string.Empty;
        private int _nivelAcesso = 3;
        private bool _ativo = true;

        //-----------------------------
        // Getters e Setters
        //-----------------------------
        public int IdUsuario
        {
            get { return _idUsuario; }
            set { _idUsuario = value; }
        }

        public string Nome
        {
            get { return _nome; }
            set { _nome = value; }
        }

        public string Email
        {
            get { return _email; }
            set { _email = value; }
        }

        public string Senha
        {
            get { return _senha; }
            set { _senha = value; }
        }

        public int NivelAcesso
        {
            get { return _nivelAcesso; }
            set { _nivelAcesso = value; }
        }

        public bool Ativo
        {
            get { return _ativo; }
            set { _ativo = value; }
        }
    }
}
