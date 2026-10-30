namespace SGIF.Classes
{
    public class Leituras
    {
        private int _idLeitura;
        private int _idSensor;
        private decimal _valor;
        private DateTime _dataHora = DateTime.Now;
        private Sensor? _sensor;
        private ICollection<Alerta> _alertas = new List<Alerta>();

        public int IdLeitura
        {
            get 
            { 
                return _idLeitura; 
            }
            set 
            { 
                _idLeitura = value; 
            }
        }

        public int IdSensor
        {
            get 
            { 
                return _idSensor; 
            }
            set 
            { 
                _idSensor = value; 
            }
        }

        public decimal Valor
        {
            get 
            { 
                return _valor; 
            }
            set 
            { 
                _valor = value; 
            }
        }

        public DateTime DataHora
        {
            get 
            { 
                return _dataHora; 
            }
            set 
            { 
                _dataHora = value; 
            }
        }

        public Sensor? Sensor
        {
            get 
            { 
                return _sensor; 
            }
            set 
            { 
                _sensor = value; 
            }
        }

        public ICollection<Alerta> Alertas
        {
            get 
            { 
                return _alertas; 
            }
            set 
            { 
                _alertas = value; 
            }
        }
    }
}
