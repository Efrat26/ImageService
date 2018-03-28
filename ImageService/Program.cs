using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace ImageService1
{
    static class Program
    {


        static void Main(string[] args)
        {
            // Put the body of your old Main method here.  
            ServiceBase[] ServicesToRun = new ServiceBase[] { new ImageService(args) };
            ServiceBase.Run(ServicesToRun);
        }


        /*
        static void Main(string[] args)
        {
           // Put the body of your old Main method here.  
           ServiceBase[] ServicesToRun = new ServiceBase[] { new ImageService(args) };
           ServiceBase.Run(ServicesToRun);
         }
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// 
        /*
        static void Main(string[] args)
        {
            ServiceBase[] ServicesToRun = new ServiceBase[] { new ImageService(args) };
            ServiceBase.Run(ServicesToRun);
            /*
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new ImageService()
            };
            ServiceBase.Run(ServicesToRun);

        }
          if (Environment.UserInteractive)
        {
            ImageService service1 = new ImageService(args);
            service1.TestStartupAndStop(args);
        }
        else
        {
    
    }
    */
    }
}
