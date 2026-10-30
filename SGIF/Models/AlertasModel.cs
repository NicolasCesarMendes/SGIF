namespace SGIF.Classes
{
    public class Alertas
    {
        private int _idAlerta;
        private int _idLeitura;
        private string _nivel = string.Empty;
        private string? _descricao;
        private DateTime _dataHora = DateTime.Now;
        private string _status = "Aberto";
        private Leitura? _leitura;

        public int IdAlerta
        {
            get 
            { 
                return _idAlerta; 
            }
            set 
            { 
                _idAlerta = value; 
            }
        }

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

        public string Nivel
        {
            get 
            { 
                return _nivel; 
            }
            set 
            { 
                _nivel = value; 
            }
        }

        public string? Descricao
        {
            get 
            { 
                return _descricao; 
            }
            set 
            { 
                _descricao = value; 
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

        public Leitura? Leitura
        {
            get 
            { 
                return _leitura; 
            }
            set 
            { 
                _leitura = value; 
            }
        }
    }
}
