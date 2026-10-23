namespace SGIF.Classes
{
    public class Trens
    {
        private int _idTrem;
        private string _identificacao = string.Empty;
        private ICollection<Locomotiva> _locomotivas = new List<Locomotiva>();
        private ICollection<Vagao> _vagoes = new List<Vagao>();

        public int IdTrem
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

        public string Identificacao
        {
            get 
            { 
                return _identificacao; 
            }
            set 
            { 
                _identificacao = value; 
            }
        }

        public ICollection<Locomotiva> Locomotivas
        {
            get 
            { 
                return _locomotivas; 
            }
            set 
            { 
                _locomotivas = value; 
            }
        }

        public ICollection<Vagao> Vagoes
        {
            get 
            { 
                return _vagoes; 
            }
            set 
            { 
                _vagoes = value; 
            }
        }
    }
}
