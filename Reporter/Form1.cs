using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Reporter.Communication;
using Reporter.Email;
using Reporter.Excel;
using Reporter.Model;
using Reporter.Model.ProcessedItems;
using Reporter.Processing;
using Reporter.Reports;

namespace Reporter
{
	public partial class Form1 : Form
	{
		private NetQueryResult netQueryResult = null;
		private readonly List<DataTable> reports = new List<DataTable>();
		private readonly SynchronizationContext guiContext;

		public Form1()
		{
			InitializeComponent();
			guiContext = SynchronizationContext.Current;
			SetLoading(false);
		}

		private void HandleUserAddClicked(object sender, EventArgs e)
		{
			lbUserId.Items.Add(Convert.ToInt32(numUserId.Value));
		}

		private void HandleCompanyAddClicked(object sender, EventArgs e)
		{
			foreach (var userId in CommunicationHelper.GetUserIdForCompany(Convert.ToInt32(numUserId.Value)))
			{
				lbUserId.Items.Add(userId);
			}
		}

		private void HandleRemoveUserClicked(object sender, EventArgs e)
		{
			if (lbUserId.SelectedIndex < 0) return;
			lbUserId.Items.Remove(lbUserId.SelectedItem);
		}

		private void HandleFetchClicked(object sender, EventArgs e)
		{
			if (ModifierKeys == Keys.Shift)
			{
				// Do super secret reporting instead ;)
				var userIds = lbUserId.Items.Cast<int>().ToArray();
				var fromDate = dtFrom.Value.Date.AddHours(3).ToUniversalTime();
				var toDate = dtTo.Value.Date.AddHours(3).AddDays(1).ToUniversalTime();
				ReportHelper.GeneratePluginReports(userIds, fromDate, toDate);
				return;
			}

			gbTransform.Enabled = false;
			btnExport.Enabled = false;
			SetLoading(true);
			ThreadPool.QueueUserWorkItem(_ =>
			{
				var stopwatch = Stopwatch.StartNew();
				var userIds = lbUserId.Items.Cast<int>().ToArray();
				var query = CommunicationHelper.Query(userIds, dtFrom.Value.Date.AddHours(3).ToUniversalTime(),
					dtTo.Value.Date.AddHours(3).AddDays(1).ToUniversalTime());
				/*HtmlReportHelper.GenerateEmailReport(@"C:\tmp3", userIds, dtFrom.Value.Date.AddHours(3).ToUniversalTime(),
					dtTo.Value.Date.AddHours(3).AddDays(1).ToUniversalTime());*/
				netQueryResult = query.CalculateNet();
				Debug.WriteLine("Fetch done in {0}ms", stopwatch.Elapsed.TotalMilliseconds);
				guiContext.Post(__ =>
				{
					SetLoading(false);
					if (netQueryResult == null) return;
					gbTransform.Enabled = true;
					btnExport.Enabled = true;
				}, null);
			}, null);
		}

		private void SetLoading(bool isLoading)
		{
			pbLoading.Visible = isLoading;
		}

		private void HandleExportClicked(object sender, EventArgs e)
		{
			if (saveFileDialog1.ShowDialog() != DialogResult.OK) return;
			SetLoading(true);
			ThreadPool.QueueUserWorkItem(_ =>
			{
				var transformResult = Processing.ReportHelper.Transform(netQueryResult);
				ExcelHelper.Export(saveFileDialog1.FileName, transformResult, true, cbReports.Checked ? reports.ToArray() : null);
				guiContext.Post(__ =>
				{
					SetLoading(false);
				}, null);
			});
			
		}

		private void HandleRemoveAllClicked(object sender, EventArgs e)
		{
			lbUserId.Items.Clear();
		}
	}
}
