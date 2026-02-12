using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace NativeMessagingHost
{
	class Logger
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public static void Setup(string image)
		{
			log4net.Config.XmlConfigurator.Configure();
			if (image != "msedge") return;

			var hierarchy = (Hierarchy) LogManager.GetRepository();
			var appenders = hierarchy.Root.Appenders.OfType<RollingFileAppender>().ToList();
			foreach (var appender in appenders)
			{
				appender.File = appender.File.Replace(".Chrome.", $".MsEdge.");
				appender.ActivateOptions();
			}
		}
	}
}
