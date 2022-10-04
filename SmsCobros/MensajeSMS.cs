using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSCobros
{
    public class MensajeSMS
    {
        public string NOM_CLIENTE { get; set; }
        public int NUM_MENSAJE { get; set; }
        public string NUM_TELEFONO { get; set; }
        public string COUNTRYCODE { get; set; }
        public string MENSAJE { get; set; }
        public DateTime FEC_MENSAJE { get; set; }
        public string IND_ESTADO { get; set; }
        public string COD_MODULO { get; set; }

        public string toEmail { get; set; }

        public MensajeSMS()
        {
            this.toEmail = string.Empty;
        }
    }
}
