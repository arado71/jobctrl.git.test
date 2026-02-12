using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Text;
using log4net;
using Tct.ActivityRecorderClient.Communication;

namespace Tct.ActivityRecorderClient.Capturing.Core
{
	public static class AppControlServiceHelper
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public static bool IsRegistered { get; private set; }
		public static void RegisterProcess()
		{
			try
			{
				using (var wrapper = new AppControlServiceClientWrapper())
				{
					wrapper.Client.RegisterProcess(Process.GetCurrentProcess().Id);
					log.Info("Process registered in AppControlService");
					IsRegistered = true;
				}
			}
			catch (EndpointNotFoundException)
			{
				log.Debug("AppControlService is not available, registering process skipped");
				IsRegistered = false;
			}
			catch (Exception e)
			{
				log.Error("RegisterProcess failed", e);
				IsRegistered = false;
			}
		}

		public static void UnregisterProcess()
		{
			try
			{
				using (var wrapper = new AppControlServiceClientWrapper())
				{
					wrapper.Client.UnregisterProcess(Process.GetCurrentProcess().Id);
					log.Info("Process unregistered from AppControlService");
					IsRegistered = false;
				}
			}
			catch (EndpointNotFoundException)
			{
				log.Debug("AppControlService is not available, unregistering process skipped");
			}
			catch (Exception e)
			{
				log.Error("UnregisterProcess failed", e);
			}
		}

		public static bool Ping()
		{
			try
			{
				using (var wrapper = new AppControlServiceClientWrapper())
				{
					wrapper.Client.Ping();
				}
			}
			catch (EndpointNotFoundException)
			{
				if (IsRegistered)
				{
					log.Debug("AppControlService became not available");
					IsRegistered = false;
				}

				return false;
			}
			catch (ActionNotSupportedException e)
			{
				// do nothing, ping maybe not implemented in service. It doesn't matter.
			}
			catch (Exception e)
			{
				log.Debug("Ping error",e);
				return false;
			}
			if (!IsRegistered) 
				RegisterProcess();
			return true;
		}
	}
}
