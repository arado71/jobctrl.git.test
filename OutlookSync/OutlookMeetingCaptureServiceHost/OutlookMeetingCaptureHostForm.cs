using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ServiceModel;
using System.Diagnostics;
using log4net;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows.ProcessInfo;

namespace OutlookMeetingCaptureServiceHost
{
	public partial class OutlookMeetingCaptureHostForm : Form
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);	//TODO: adding log4net.dll to installer!!!
		private static readonly string outlookSyncServiceEndpointScheme = "net.pipe://localhost/OutlookMeetingCaptureService_{0}"; 
		private readonly int parentPID;
		private readonly string storePattern;
		private readonly TimeSpan? serviceTimeout;
		private ServiceHost serviceHost;
		private OutlookMeetingCaptureService.OutlookMeetingCaptureService outlookMeetingCaptureServiceInstance;
		private Uri[] baseAddresses;

		private Timer parentProcessCheckTimer;

		public OutlookMeetingCaptureHostForm(int hostPID, string storePathPattern, TimeSpan? serviceTimeout)
		{
			this.parentPID = hostPID;
			this.storePattern = storePathPattern;
			this.serviceTimeout = serviceTimeout;
			baseAddresses = new Uri[] { new Uri(String.Format(outlookSyncServiceEndpointScheme, hostPID)) };
			InitializeComponent();
		}

		private void HostForm_Load(object sender, EventArgs e)
		{
			this.Visible = false;
			
			try
			{
				StartCheckingParentProcess();

				outlookMeetingCaptureServiceInstance = new OutlookMeetingCaptureService.OutlookMeetingCaptureService
				{
					LocalStorePattern = storePattern
				};
				outlookMeetingCaptureServiceInstance.Initialize();
				serviceHost = new ServiceHost(outlookMeetingCaptureServiceInstance);
				string endpointAddress = String.Format(outlookSyncServiceEndpointScheme, parentPID.ToString());
				var binding = new NetNamedPipeBinding();
				if (serviceTimeout.HasValue)
				{
					binding.ReceiveTimeout = serviceTimeout.Value;
					binding.SendTimeout = serviceTimeout.Value;
					binding.OpenTimeout = serviceTimeout.Value;
					binding.CloseTimeout = serviceTimeout.Value;
					log.Debug($"Service timeout: {serviceTimeout:g}");
				}
				serviceHost.AddServiceEndpoint("OutlookMeetingCaptureService.IMeetingCaptureService", binding, endpointAddress);
				serviceHost.Closed += new EventHandler(serviceHost_Closed);
				serviceHost.Open();

				log.Info("OutlookMeetingCaptureService started successfully.");
			}
			catch (AddressAlreadyInUseException ex)
			{
				//MessageBox.Show("OutlookMeetingCaptureService already running.", "JobCTRL - OutlookSync initialization failure", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				log.Error("OutlookMeetingCaptureService already listen on the given port. (Stopping OutlookSync process.)", ex);
				Environment.Exit(-1);
			}
			catch (Exception ex)
			{
				//MessageBox.Show("_OutlookMeetingCaptureService already running.", "JobCTRL - OutlookSync initialization failure", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				log.Error("Error occured while starting OutlookMeetingCaptureService. (Stopping OutlookSync process.)", ex);
				Environment.Exit(-1);
			}
		}

		void serviceHost_Closed(object sender, EventArgs e)
		{
			log.Info("Service has been closed.");
			Close();
		}

		private void HostForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			try
			{
				log.Info("Form has been closed.");
				outlookMeetingCaptureServiceInstance.Dispose();
				serviceHost.Closed -= serviceHost_Closed;
				serviceHost.Close();
				StopCheckingParentProcess();
			}
			catch (Exception ex)
			{
				log.Error("Error occured while closing OutlookMeetingCaptureHostForm.", ex);
			}
		}

		private void StartCheckingParentProcess()
		{
			parentProcessCheckTimer = new System.Windows.Forms.Timer();
			parentProcessCheckTimer.Interval = 30000;
			parentProcessCheckTimer.Tick += parentProcessCheckTimer_Tick;
			parentProcessCheckTimer.Enabled = true;

			log.Info("Parent process checker started successfully.");
		}

		private void StopCheckingParentProcess()
		{
			parentProcessCheckTimer.Enabled = false;
			parentProcessCheckTimer.Tick -= parentProcessCheckTimer_Tick;
			parentProcessCheckTimer.Dispose();
			parentProcessCheckTimer = null;

			log.Info("Parent process checker stopped successfully.");
		}

		void parentProcessCheckTimer_Tick(object sender, EventArgs e)
		{
			if (!ProcessNameHelper.IsProcessRunning(parentPID))
			{
				log.Error("Could not find parent process (maybe it was forced to stop). Stopping OutlookSync process...");
				Close();
			}
		}

	}
}
