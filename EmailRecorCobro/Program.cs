using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace EmailRecorCobro
{
    static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        static void Main()
        {
            //#if DEBUG
            //            EmailCobro servicio = new EmailCobro();
            //            servicio.onDebug();
            //            Console.ReadLine();
            //#else
            //                ServiceBase[] ServicesToRun;
            //                        ServicesToRun = new ServiceBase[]
            //                        {
            //                            new EmailCobro()
            //                        };
            //                        ServiceBase.Run(ServicesToRun);
            //#endif
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                            new EmailCobro()
            };
            ServiceBase.Run(ServicesToRun);

        }
    }
}
