using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace MyWindowsHeartbeat
{
    static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        static void Main()
        {
           
            //#if (!DEBUG)
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
			{ 
				new Service1() 
			};
            ServiceBase.Run(ServicesToRun);

            /*#else
            
            Service1 s = new Service1();
            s.InitializeComponent();

            s.Start_();
            Console.WriteLine("Press any key to stop program");

            while (Console.ReadLine() == null)
            {
                System.Threading.Thread.Sleep(100);
            }
            s.Stop_();

            /*#endif*/
        }
    }
}
