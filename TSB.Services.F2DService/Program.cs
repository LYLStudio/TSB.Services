using System;
using System.ServiceProcess;

namespace TSB.Services.F2DService
{
    static class Program
    {
        /// <summary>
        /// 應用程式的主要進入點。
        /// </summary>
        static void Main()
        {
            if (Environment.UserInteractive)
            {
                DataImportService svc = new DataImportService();
                svc.TestStartupAndStop(null);

            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                new DataImportService()
                };
                ServiceBase.Run(ServicesToRun);
            }

        }
    }
}
