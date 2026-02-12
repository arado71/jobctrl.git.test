using System.ServiceProcess;
using System.Threading;
using log4net;

namespace Tct.MailFilterService
{
    static class Program
    {

        public static ServiceBase Service { get; set; }
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static void Main()
        {
#if DEBUG
            log.Debug("Debug build does not work as windows service!");
            MailFilterServiceHost.StartService();
            Thread.Sleep(Timeout.Infinite);
#else
            Service = new MailFilterServiceHost();
            ServiceBase[] servicesToRun = new[] 
            { 
                Service  
            };
            ServiceBase.Run(servicesToRun);
#endif
        }
    }
}
