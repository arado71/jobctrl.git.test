using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using MonitoringClient.ActivityMonitoringServiceReference;

namespace MonitoringClient
{
	public partial class KickWindow : ChildWindow
	{
		public int UserId { get; set; }
		public int ComputerId { get; set; }
		public ActivityMonitoringClient Client { get; set; }

		public KickWindow()
		{
			InitializeComponent();
		}

		private void OKButton_Click(object sender, RoutedEventArgs e)
		{
			busyKick.IsBusy = true;
			Client.KickUserComputerCompleted += KickUserComputerCompleted;
			var saved = Client.InnerChannel.OperationTimeout;
			Client.InnerChannel.OperationTimeout = TimeSpan.FromMinutes(2.3);
			Client.KickUserComputerAsync(UserId, ComputerId, TxtReason.Text, TimeSpan.FromMinutes(2));
			Client.InnerChannel.OperationTimeout = saved;
		}

		private void KickUserComputerCompleted(object sender, KickUserComputerCompletedEventArgs e)
		{
			busyKick.IsBusy = false;
			((ActivityMonitoringClient)sender).KickUserComputerCompleted -= KickUserComputerCompleted;
			MessageBoxWindow msg;
			if (e.Error != null)
			{
				if (e.Error is FaultException<KickTimeoutException>)
				{
					msg = new MessageBoxWindow() { Title = "Failed to kick user in the allotted time", Text = "Failed to kick user in the allotted time." };
				}
				else
				{
					msg = new MessageBoxWindow() { Title = "Failed to kick user", Text = "Failed to kick user. Error was: " + e.Error.Message };
				}
			}
			else
			{
				if (e.Result == KickResult.UnknownError)
				{
					msg = new MessageBoxWindow() { Title = "Failed to kick user", Text = "Failed to kick user. Result was: " + e.Result };
				}
				else
				{
					msg = new MessageBoxWindow() { Title = "Successfully kicked user", Text = "Successfully kicked user. Result was: " + e.Result };
				}
			}
			msg.Show();
			this.DialogResult = true;
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = false;
		}
	}
}

