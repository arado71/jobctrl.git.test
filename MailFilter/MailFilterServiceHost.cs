using System.Threading;

namespace Tct.MailFilterService
{
    using System;
    using System.Linq;
    using System.ServiceModel;
    using System.ServiceProcess;
    using log4net;

    public partial class MailFilterServiceHost : ServiceBase
    {
        private const string ApplicationName = "Mail Service for Issues";
        private static readonly string Bitness = IntPtr.Size == 8 ? "64 bit" : "32 bit";
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);	
        private static readonly Version Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        private static readonly string DebugOrReleaseString = (System.Reflection.Assembly.GetExecutingAssembly()
            .GetCustomAttributes(typeof(System.Reflection.AssemblyConfigurationAttribute), false)
            .OfType<System.Reflection.AssemblyConfigurationAttribute>()
            .FirstOrDefault(n => !string.IsNullOrEmpty(n.Configuration)) ?? new System.Reflection.AssemblyConfigurationAttribute("Unknown")).Configuration + " build";
        private static PeriodicManagerBase periodicManager;

        public MailFilterServiceHost()
        {
            InitializeComponent();

            log.Info("Initializing " + ApplicationName + " (" + Bitness + ") " + DebugOrReleaseString + " " + " Ver.:" + Version);
            log.Info(string.Format("Machinename: {0}, OSVersion: {1}, Framework version: {2}", Environment.MachineName, Environment.OSVersion, Environment.Version));
        }
        protected override void OnStart(string[] args)
        {
            StartService();
        }
        protected override void OnStop()
        {
            StopService();
        }
        protected override void OnShutdown()
        {
            log.Info("mailFilter OnShutdown");
            StopService();
        }

        internal static void StartService()
        {
            StopService();
            periodicManager = new PeriodicManager();
        }
        internal static void StopService()
        {
            if (periodicManager != null)
                periodicManager.Stop();
        }
    }
}
