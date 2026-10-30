namespace SGIF.Classes
{
    public class Locomotivas
    {
        private int _idLocomotiva;
        private string _numeroIdentificacao = string.Empty;
        private string _modelo = string.Empty;
        private string? _fabricante;
        private int? _anoFabricacao;
        private string _status = "Ativa";
        private int? _idTrem;
        private Trem? _trem;
        private ICollection<Sensor> _sensores = new List<Sensor>();

        public int IdLocomotiva
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

        public string NumeroIdentificacao
        {
            get 
            { 
                return _numeroIdentificacao; 
            }
            set 
            { 
                _numeroIdentificacao = value; 
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

        public string? Fabricante
        {
            get 
            { 
                return _fabricante; 
            }
            set 
            { 
                _fabricante = value; 
            }
        }

        public int? AnoFabricacao
        {
            get 
            { 
                return _anoFabricacao; 
            }
            set 
            { 
                _anoFabricacao = value; 
            }
        }

        public string Status
        {
            get 
            { 
                return _status; 
            }
            set 
            { 
                _status = value; 
            }
        }

        public int? IdTrem
        {
            get 
            { 
                return _idTrem; 
            }
            set 
            { 
                _idTrem = value; 
            }
        }

        public Trem? Trem
        {
            get 
            { 
                return _trem; 
            }
            set 
            { 
                _trem = value; 
            }
        }

        public ICollection<Sensor> Sensores
        {
            get 
            { 
                return _sensores; 
            }
            set 
            { 
                _sensores = value; 
            }
        }
    }
}
