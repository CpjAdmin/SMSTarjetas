using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace SMSCobros
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new SmsCobros()
            };
            ServiceBase.Run(ServicesToRun);
            //#if DEBUG  //esto se llaman directivas C# preprocessor directives
            //            SmsCobros servicio = new SmsCobros();
            //            servicio.onDebug();
            //            Console.ReadLine();
            //#else
            //                                        ServiceBase[] ServicesToRun;
            //                                                ServicesToRun = new ServiceBase[]
            //                                                {
            //                                                    new SmsCobros()
            //                                                };
            //                                                ServiceBase.Run(ServicesToRun);
            //#endif
        }
    }
}
