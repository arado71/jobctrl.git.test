using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace Tct.DeployService
{
	static class Program
	{
		static Program()
		{
			// Load dependencies
			AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
			{
				string resourceName = "Tct.DeployService." + new AssemblyName(args.Name).Name + ".dll";
				using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
				{
					if (stream == null) return null;
					var assemblyData = new byte[stream.Length];
					stream.Read(assemblyData, 0, assemblyData.Length);
					return Assembly.Load(assemblyData);
				}
			};
		}

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] arguments)
		{
			if (!arguments.Contains("/s"))
			{
				var hierarchy = (Hierarchy)LogManager.GetRepository();
				var patternLayout = new PatternLayout
				{
					ConversionPattern = "%d [%t] %-5p %m%n"
				};

				patternLayout.ActivateOptions();

				var tracer = new TraceAppender
				{
					Layout = patternLayout
				};
				tracer.ActivateOptions();
				hierarchy.Root.AddAppender(tracer);

				var roller = new RollingFileAppender
				{
					Layout = patternLayout,
					AppendToFile = true,
					RollingStyle = RollingFileAppender.RollingMode.Size,
					MaxSizeRollBackups = 4,
					MaximumFileSize = "1MB",
					StaticLogFileName = true,
					File = "log.txt"
				};
				roller.ActivateOptions();
				hierarchy.Root.AddAppender(roller);

				hierarchy.Root.Level = Level.All;
				hierarchy.Configured = true;	
			}

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new WizardForm());
		}
	}
}
