namespace SGIF.Classes
{
    public class Sensores
    {
        private int _idSensor;
        private string _tipo = string.Empty;
        private string _modelo = string.Empty;
        private decimal? _valorMinimo;
        private decimal? _valorMaximo;
        private decimal? _limiteAtencao;
        private decimal? _limiteCritico;
        private int? _idLocomotiva;
        private int? _idVagao;
        private Locomotiva? _locomotiva;
        private Vagao? _vagao;
        private ICollection<Leitura> _leituras = new List<Leitura>();

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

        public string Tipo
        {
            get 
            { 
                return _tipo; 
            }
            set 
            { 
                _tipo = value; 
            }
        }

        public string Modelo
        {
            get 
            { 
                return _modelo; 
            }
            set 
            { 
                _modelo = value; 
            }
        }

        public decimal? ValorMinimo
        {
            get 
            { 
                return _valorMinimo; 
            }
            set 
            { 
                _valorMinimo = value; 
            }
        }

        public decimal? ValorMaximo
        {
            get 
            { 
                return _valorMaximo; 
            }
            set 
            { 
                _valorMaximo = value; 
            }
        }

        public decimal? LimiteAtencao
        {
            get 
            { 
                return _limiteAtencao; 
            }
            set 
            { 
                _limiteAtencao = value; 
            }
        }

        public decimal? LimiteCritico
        {
            get 
            { 
                return _limiteCritico; 
            }
            set 
            { 
                _limiteCritico = value; 
            }
        }

        public int? IdLocomotiva
        {
            get 
            { 
                return _idLocomotiva; 
            }
            set 
            { 
                _idLocomotiva = value; 
            }
        }

        public int? IdVagao
        {
            get 
            { 
                return _idVagao; 
            }
            set 
            { 
                _idVagao = value; 
            }
        }

        public Locomotiva? Locomotiva
        {
            get 
            { 
                return _locomotiva; 
            }
            set 
            { 
                _locomotiva = value; 
            }
        }

        public Vagao? Vagao
        {
            get 
            { 
                return _vagao; 
            }
            set 
            { 
                _vagao = value; 
            }
        }

        public ICollection<Leitura> Leituras
        {
            get 
            { 
                return _leituras; 
            }
            set 
            { 
                _leituras = value; 
            }
        }
    }
}
