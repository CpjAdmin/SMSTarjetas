using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailRecorCobro
{
    public class MensajeEmail
    {
        public int NUM_MENSAJE { get; set; }

        public string ORIGEN { get; set; }

        public string DESTINO { get; set; }

        public string ASUNTO { get; set; }

        public string TEXTO { get; set; }

        public DateTime FEC_MENSAJE { get; set; }
        public string ESTADO { get; set; }
        public string COD_MODULO { get; set; }

        public int COD_CLIENTE { get; set; }

        public string NOM_CLIENTE { get; set; }
    }
}
