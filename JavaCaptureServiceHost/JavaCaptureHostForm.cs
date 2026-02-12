using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using log4net;
using Tct.Java.Service;
using Timer = System.Windows.Forms.Timer;

namespace JavaCaptureServiceHost
{
	public partial class JavaCaptureHostForm : Form
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private const string JavaCaptureServiceEndpointScheme = "net.pipe://localhost/JavaCaptureService_{0}";
		private readonly int parentPid;
		private ServiceHost serviceHost;
		private IJavaCaptureService javaCaptureServiceInstance;
		private Timer parentProcessCheckTimer;


		public JavaCaptureHostForm(int parentProcessId)
		{
			parentPid = parentProcessId;
			InitializeComponent();
		}

		private void JavaCaptureHostForm_Load(object sender, EventArgs e)
		{
			Visible = false;

			try
			{
				StartCheckingParentProcess();

				javaCaptureServiceInstance = new JavaCaptureService();
				javaCaptureServiceInstance.InitializePlugin(SynchronizationContext.Current);
				serviceHost = new ServiceHost(javaCaptureServiceInstance);
				serviceHost.Description.Endpoints.Clear(); //use programmatic endpoint only
				var metad = serviceHost.Description.Behaviors.Find<ServiceMetadataBehavior>();  //we don't expose mex for release builds and we might not have permissions if not elevated
				if (metad != null) serviceHost.Description.Behaviors.Remove(metad);
				string endpointAddress = string.Format(JavaCaptureServiceEndpointScheme, parentPid);
				serviceHost.AddServiceEndpoint("Tct.Java.Service.IJavaCaptureService", new NetNamedPipeBinding(), endpointAddress);
				serviceHost.Closed += serviceHost_Closed;
				serviceHost.Open();

				log.Info("JavaCaptureService started successfully.");
			}
			catch (AddressAlreadyInUseException ex)
			{
				log.Error("JavaCaptureService already listen on the given port. (Stopping JavaCapture process.)", ex);
				Environment.Exit(-1);
			}
			catch (Exception ex)
			{
				log.Error("Error occured while starting JavaCaptureService. (Stopping JavaCapture process.)", ex);
				Environment.Exit(-1);
			}
		}

		void serviceHost_Closed(object sender, EventArgs e)
		{
			log.Info("Service has been closed.");
			Close();
		}

		private void JavaCaptureHostForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			try
			{
				log.Info("Form has been closed.");
				//javaCaptureServiceInstance.Dispose(); this can be used later, when we can shut down the bridge.
				serviceHost.Closed -= serviceHost_Closed;
				serviceHost.Close();
				StopCheckingParentProcess();
			}
			catch (Exception ex)
			{
				log.Error("Error occured while closing JavaCaptureHostForm.", ex);
			}
		}

		private void StartCheckingParentProcess()
		{
			parentProcessCheckTimer = new Timer();
			parentProcessCheckTimer.Interval = 3000;
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
				using (Process.GetProcessById(parentPid)) { }
			}
			catch (Exception ex)
			{
				log.Warn("Could not find parent process (maybe it was forced to stop). Stopping JavaCapture process...", ex);
				Close();
			}
		}
	}
}
