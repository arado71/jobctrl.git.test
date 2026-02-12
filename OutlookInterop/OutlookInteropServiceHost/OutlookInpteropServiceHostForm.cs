using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Windows.Forms;
using log4net;
using OutlookInteropService;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows.ProcessInfo;
using Tct.ActivityRecorderClient.Capturing.Mail;

namespace OutlookInteropServiceHost
{
	public partial class OutlookInpteropServiceHostForm : Form
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private ServiceHost serviceHost;
		private readonly string suffix;
		private readonly int parentPid;
		private MailTrackingType trackingType;
		private MailTrackingSettings trackingSettings;

		public OutlookInpteropServiceHostForm(int parentPid, MailTrackingType trackingType, MailTrackingSettings trackingSettings)
			: this()
		{
			this.parentPid = parentPid;
			this.trackingType = trackingType;
			this.trackingSettings = trackingSettings;
			if (!IsDebug && parentPid == -1) //-1 is only for debug builds
			{
				throw new ArgumentOutOfRangeException("parentPid");
			}

			suffix = "_" + parentPid;

			if (parentPid == -1)
			{
				this.StartPosition = FormStartPosition.Manual;
				this.ShowInTaskbar = true;
				this.Location = new Point(100, 100);
				this.WindowState = FormWindowState.Normal;
			}
		}

		public OutlookInpteropServiceHostForm()
		{
			InitializeComponent();
		}

		private void OutlookInpteropServiceHostForm_Load(object sender, EventArgs e)
		{
			this.Visible = parentPid == -1;

			try
			{
				var serviceInstance = new OutlookMailCaptureService(trackingType, trackingSettings);
				serviceHost = new ServiceHost(serviceInstance);
				serviceHost.Description.Endpoints.Clear(); //use programmatic endpoint only

				if (!IsDebug || !ProcessElevationHelper.IsElevated()) //we don't expose mex for release builds and we might not have permissions if not elevated
				{
					var metad = serviceHost.Description.Behaviors.Find<ServiceMetadataBehavior>();
					if (metad != null) serviceHost.Description.Behaviors.Remove(metad);
				}

				serviceHost.AddServiceEndpoint("OutlookInteropService.IOutlookMailCaptureService", new NetNamedPipeBinding(), "net.pipe://localhost/OutlookMailCaptureService" + suffix); //todo use settings from config
				serviceHost.Closed += new EventHandler(ServiceHost_Closed);
				serviceHost.Open();

				//test
				//var eee = serviceInstance.GetMailCaptures();
				//var start = Environment.TickCount;
				//for (int i = 0; i < 100; i++)
				//{
				//    eee = serviceInstance.GetMailCaptures();
				//}
				//var time = Environment.TickCount - start;

				log.Info("OutlookMailCaptureService started successfully.");
			}
			catch (AddressAlreadyInUseException ex)
			{
				log.Error("OutlookMailCaptureService already listening on the given address.", ex);
				Environment.Exit(-1);
			}
			catch (Exception ex)
			{
				log.Error("Error occured while starting OutlookMailCaptureService.", ex);
				Environment.Exit(-1);
			}
		}

		private void ServiceHost_Closed(object sender, EventArgs e)
		{
			log.Info("Service has been closed.");
			Close();
		}

		private void OutlookInpteropServiceHostForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			try
			{
				log.Info("Form has been closed.");
				if (serviceHost != null)
				{
					serviceHost.Closed -= ServiceHost_Closed;
					serviceHost.Close();
					serviceHost = null;
				}
			}
			catch (Exception ex)
			{
				log.Error("Error occured while closing OutlookMailCaptureHostForm.", ex);
			}
		}

		private void timer_Tick(object sender, EventArgs e)
		{
			if (parentPid == -1) //test capturing
			{
				if (serviceHost == null) return;
				try
				{
					var captures = ((OutlookMailCaptureService)serviceHost.SingletonInstance).GetMailCaptures();
					log.Info("Captures " + captures);
				}
				catch (Exception ex)
				{
					log.Error("GetMailCaptures failed", ex);
				}
				return;
			}
			if (!ProcessNameHelper.IsProcessRunning(parentPid))
			{
				log.Error("Could not find parent process (maybe it was forced to stop). Stopping outlook interop process...");
				Close();
			}
		}

		private static bool IsDebug
		{
			get
			{
#if DEBUG
				return true;
#endif
				return false;
			}
		}
	}
}
