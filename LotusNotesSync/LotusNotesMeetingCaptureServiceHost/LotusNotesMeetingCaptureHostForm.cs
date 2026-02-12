using System;
using System.Windows.Forms;
using System.ServiceModel;
using System.Diagnostics;
using log4net;
using System.ServiceModel.Description;

namespace LotusNotesMeetingCaptureServiceHost
{
	public partial class LotusNotesMeetingCaptureHostForm : Form
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);	//TODO: adding log4net.dll to installer!!!
		private const string lotusNotesSyncServiceEndpointScheme = "net.pipe://localhost/LotusNotesMeetingCaptureService_{0}";
		private readonly int parentPid;
		private ServiceHost serviceHost;
		private LotusNotesMeetingCaptureServiceNamespace.LotusNotesMeetingCaptureService lotusNotesMeetingCaptureServiceInstance;

		private Timer parentProcessCheckTimer;

		public LotusNotesMeetingCaptureHostForm(int parentPid)
		{
			this.parentPid = parentPid;
			InitializeComponent();
		}

		private void HostForm_Load(object sender, EventArgs e)
		{
			Visible = false;

			try
			{
				StartCheckingParentProcess();

				lotusNotesMeetingCaptureServiceInstance = new LotusNotesMeetingCaptureServiceNamespace.LotusNotesMeetingCaptureService();
				serviceHost = new ServiceHost(lotusNotesMeetingCaptureServiceInstance);
				serviceHost.Description.Endpoints.Clear(); //use programmatic endpoint only
				var metad = serviceHost.Description.Behaviors.Find<ServiceMetadataBehavior>();	//we don't expose mex for release builds and we might not have permissions if not elevated
				if (metad != null) serviceHost.Description.Behaviors.Remove(metad);
				string endpointAddress = String.Format(lotusNotesSyncServiceEndpointScheme, parentPid);
				serviceHost.AddServiceEndpoint("LotusNotesMeetingCaptureServiceNamespace.IMeetingCaptureService", new NetNamedPipeBinding(), endpointAddress);
				serviceHost.Closed += serviceHost_Closed;
				serviceHost.Open();

				log.Info("LotusNotesMeetingCaptureService started successfully.");
			}
			catch (AddressAlreadyInUseException ex)
			{
				//MessageBox.Show("LotusNotesMeetingCaptureService already running.", "JobCTRL - LotusNotesSync initialization failure", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				log.Error("LotusNotesMeetingCaptureService already listen on the given port. (Stopping LotusNotesSync process.)", ex);
				Environment.Exit(-1);
			}
			catch (Exception ex)
			{
				//MessageBox.Show("_LotusNotesMeetingCaptureService already running.", "JobCTRL - LotusNotesSync initialization failure", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				log.Error("Error occured while starting LotusNotesMeetingCaptureService. (Stopping LotusNotesSync process.)", ex);
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
				lotusNotesMeetingCaptureServiceInstance.Dispose();
				serviceHost.Closed -= serviceHost_Closed;
				serviceHost.Close();
				StopCheckingParentProcess();
			}
			catch (Exception ex)
			{
				log.Error("Error occured while closing LotusNotesMeetingCaptureHostForm.", ex);
			}
		}

		private void StartCheckingParentProcess()
		{
			parentProcessCheckTimer = new Timer();
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
			try
			{
				using (var parentProcess = Process.GetProcessById(parentPid))
				{
					return;
				}
			}
			catch (Exception ex)
			{
				log.Warn("Could not find parent process (maybe it was forced to stop). Stopping LotusNotesSync process...", ex);
				Close();
			}
		}

	}
}
