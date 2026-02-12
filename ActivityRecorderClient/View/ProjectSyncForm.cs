using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using log4net;
using Microsoft.SharePoint.Client;
using Tct.ActivityRecorderClient.ProjectSync;
using Tct.ActivityRecorderClient.Properties;

namespace Tct.ActivityRecorderClient.View
{
	public partial class ProjectSyncForm : FixedMetroForm
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly SynchronizationContext guiContext;
		private readonly ProjectSyncService syncService;

		public SyncContext Context { get; private set; }

		public ProjectSyncForm(ProjectSyncService service)
		{
			guiContext = AsyncOperationManager.SynchronizationContext;
			InitializeComponent();
			Icon = Tct.ActivityRecorderClient.Properties.Resources.JobCtrl;
			syncService = service;
			var credentials = syncService.LoadCredentials();
			if (credentials != null)
			{
				txtUsername.Text = credentials.Username ?? "";
				txtPassword.Text = credentials.Password ?? "";
			}

			cbRemember.Checked = !string.IsNullOrEmpty(txtPassword.Text);
			DialogResult = DialogResult.None;
		}

		private void SetBusy(bool isBusy)
		{
			txtPassword.Enabled = !isBusy;
			txtUsername.Enabled = !isBusy;
			metroButton1.Enabled = !isBusy;
			metroProgressBar1.Visible = isBusy;
		}

		private void HandleLoginClicked(object sender, EventArgs e)
		{
			SetBusy(true);
			var username = txtUsername.Text;
			var password = txtPassword.Text;
			var shouldRemember = cbRemember.Checked;
			ThreadPool.QueueUserWorkItem(_ =>
			{
				try
				{
					syncService.UpdateProgress = (act, all) => { guiContext.Post(__ =>
					{
						metroProgressBar1.Maximum = all;
						metroProgressBar1.Value = act;
					}, null); };
					Context = string.IsNullOrEmpty(username) && string.IsNullOrEmpty(password) ? syncService.CreateContext() : syncService.CreateContext(username, password);
					guiContext.Post(__ =>
					{
						syncService.SaveCredentials(new ProjectCredentials()
						{
							Username = username,
							Password = shouldRemember ? password : null,
						});
						DialogResult = DialogResult.OK;
						SetBusy(false);
						Close();
					}, null);
				}
				catch (IdcrlException ex)
				{
					log.Warn("Invalid username or password", ex);
					guiContext.Post(__ =>
					{
						SetBusy(false);
						MessageBox.Show(this, "Invalid username or password", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					}, null);
				}
				catch (ArgumentException ex)
				{
					log.Warn("Invalid username", ex);
					guiContext.Post(__ =>
					{
						SetBusy(false);
						MessageBox.Show(this, "Invalid username", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					}, null);
				}
				catch (Exception ex)
				{
					log.Warn("Unknown error", ex);
					guiContext.Post(__ =>
					{
						SetBusy(false);
						MessageBox.Show(this, "Unknown error occured:" + Environment.NewLine + ex, "Error", MessageBoxButtons.OK,
							MessageBoxIcon.Error);
					}, null);
				}
			}, null);
		}
	}
}
