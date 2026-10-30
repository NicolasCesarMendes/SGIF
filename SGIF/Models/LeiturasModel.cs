namespace SGIF.Classes
{
    public partial class Leituras
    {
        //-----------------------------
        // Atributos
        //-----------------------------
        private int _idLeitura;
        private int _idSensor;
        private decimal _valor;
        private DateTime _dataHora = DateTime.Now;

        //-----------------------------
        // Getters e Setters
        //-----------------------------
        public int IdLeitura
        {
            get { return _idLeitura; }
            set { _idLeitura = value; }
        }

        public int IdSensor
        {
            get { return _idSensor; }
            set { _idSensor = value; }
        }

        public decimal Valor
        {
            get { return _valor; }
            set { _valor = value; }
        }

        public DateTime DataHora
        {
            get { return _dataHora; }
            set { _dataHora = value; }
        }
    }
}
