namespace SGIF.Classes
{
    public partial class Trens
    {
        //-----------------------------
        // Atributos
        //-----------------------------
        private int _idTrem;
        private string _identificacao = string.Empty;

        //-----------------------------
        // Getters e Setters
        //-----------------------------
        public int IdTrem
        {
            get { return _idTrem; }
            set { _idTrem = value; }
        }

        public string Identificacao
        {
            get { return _identificacao; }
            set { _identificacao = value; }
        }
    }
}
