using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Tct.DeployService.Page
{
	public partial class Updating : WizardPage
	{
		public Updating(IWizard wizard) :
			base(wizard)
		{
			InitializeComponent();
		}

		public override void Process()
		{
			// Parse configuration
			UpdateStatus(0, Resource.ProgressParseConfig);
			var config = ServiceConfiguration.Load(Path.Combine(Wizard.Session.InstallPath, "ActivityRecorderService.dll.config"));

			// Stop service
			UpdateStatus(5, Resource.ProgressStopService);
			var serviceStarted = InstallHelper.StopService("JobCTRL Service");

			// Extract files
			UpdateStatus(10, Resource.ProgressExtract);
			InstallHelper.ExtractPayload(Wizard.Session.InstallPath);

			var envVars = new Dictionary<string, string>
			{
				{"TARGETSERVER", config.RecorderDatabase.Address},
				{"TARGETDBNAME", config.RecorderDatabase.Database},
				{"USER", config.RecorderDatabase.User},
				{"PASSWORD", config.RecorderDatabase.Password}
			};

			// Update database
			UpdateStatus(60, Resource.ProgressDbUpdate);
			InstallHelper.ExecuteCommand(Path.Combine(Wizard.Session.InstallPath, "Upgrade-auto.bat"), envVars);
			
			// Start service
			if (serviceStarted)
			{
				UpdateStatus(90, Resource.ProgressStartService);
				InstallHelper.StartService("JobCTRL Service");
			}

			// Cleanup
			UpdateStatus(95, Resource.ProgressCleanup);
			if (File.Exists(Path.Combine(Wizard.Session.InstallPath, "Upgrade-auto.bat"))) File.Delete(Path.Combine(Wizard.Session.InstallPath, "Upgrade-auto.bat"));
			if (Directory.Exists(Path.Combine(Wizard.Session.InstallPath, "Change Scripts"))) Directory.Delete(Path.Combine(Wizard.Session.InstallPath, "Change Scripts"), true);

			UpdateStatus(100, Resource.ProgressDone);
			Wizard.Finish();
		}

		private void UpdateStatus(int progress, string action)
		{
			if (InvokeRequired)
			{
				Invoke(new Action(() => UpdateStatus(progress, action)));
				return;
			}

			progressBar.Value = progress;
			lblActivity.Text = action;
		}
	}
}
