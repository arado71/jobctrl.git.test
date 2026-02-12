using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using log4net;
using Tct.Java.Plugin;

namespace Tct.Java.Service
{
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single)]
	public class JavaCaptureService: IJavaCaptureService
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private JavaAccessibilityPlugin plugin;

		public void InitializePlugin(SynchronizationContext syncContext)
		{
			plugin = new JavaAccessibilityPlugin(syncContext);
		}

		public KeyValuePair<string, string> Capture(JavaCaptureSettings captureSettings)
		{
			Debug.Assert(plugin != null, "Call plugin initializer first!");
			try
			{
				return plugin.Capture(captureSettings) ??
				       throw new FaultException<FailReason>(
					       new FailReason
					       {
						       Message = "Capturing is not possible, because the Java Access Bridge can't be enabled.",
						       Type = FailReasonType.CaptureImpossible
					       });
			}
			catch (Exception ex)
			{
				Debug.Assert(false, ex.ToString());
				throw new FaultException<FailReason>(
					new FailReason
					{
						Message = ex.ToString(),
						Type = FailReasonType.CaptureImpossible
					});
			}
		}

		public void StopService()
		{
			log.Info("StopService called");
			var sw = Stopwatch.StartNew();
			try
			{
				ServiceHostBase host = OperationContext.Current.Host;
				var sc = SynchronizationContext.Current;
				ThreadPool.QueueUserWorkItem(_ =>
				{
					Thread.Sleep(50);
					sc.Post(__ => host.Close(), null);
				}); //With .NET 4.0 simple post doesn't worked.
			}
			catch (Exception ex)
			{
				log.Error("StopService failed", ex);
				throw;
			}
			finally
			{
				log.InfoFormat("StopService finished in {0:0.000}ms.", sw.Elapsed.TotalMilliseconds);
			}
		}
	}
}
