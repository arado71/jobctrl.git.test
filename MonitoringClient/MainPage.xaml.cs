using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.DataVisualization.Charting;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using MonitoringClient.ActivityMonitoringServiceReference;

namespace MonitoringClient
{
	public partial class MainPage : UserControl
	{
		private ActivityMonitoringClient client = new ActivityMonitoringClient();
		private readonly ObservableCollection<int> detailedUsers = new ObservableCollection<int>();
		private readonly ObservableCollection<int> briefUsers = new ObservableCollection<int>();
		private readonly Timer wcfBriefRefreshTimer; //there might be some better way to handle refreshes...
		private readonly Timer wcfDetailedRefreshTimer;
		private readonly int timerInterval = (int)TimeSpan.FromSeconds(5).TotalMilliseconds;
		private readonly Dictionary<int, DetailedStatsDetailsView> detailedStatsDetailsViewDict = new Dictionary<int, DetailedStatsDetailsView>();
		private readonly LoginWindow loginWindow = new LoginWindow();
		private double currentWidth = 320;
		private const double heightDivisor = 1.25;

		public MainPage()
		{
			InitializeComponent();
			wcfBriefRefreshTimer = new Timer(wcfBriefRefreshTimerCallback, null, Timeout.Infinite, Timeout.Infinite);
			wcfDetailedRefreshTimer = new Timer(wcfDetailedRefreshTimerCallback, null, timerInterval, Timeout.Infinite);
			client.GetBriefUserStatsCompleted += GetBriefUserStatsCompleted;
			client.GetDetailedUserStatsCompleted += GetDetailedUserStatsCompleted;
			loginWindow.Closed += new EventHandler(loginWindow_Closed);
		}

		private void wcfBriefRefreshTimerCallback(object state)
		{
			Dispatcher.BeginInvoke(RefreshBriefStats); //send to gui thread
		}

		private void wcfDetailedRefreshTimerCallback(object state)
		{
			Dispatcher.BeginInvoke(RefreshDetailedStats);
		}

		private void RefreshBriefStats() //must be called from gui thread, so the completed event will be called on the gui as well
		{
			SetBriefStatus("Refreshing...");
			client.GetBriefUserStatsAsync(briefUsers);
		}

		private void RefreshDetailedStats()
		{
			if (detailedUsers.Count == 0) //don't call the service
			{
				SetDetailedStatus("");
				wcfDetailedRefreshTimer.Change(timerInterval, Timeout.Infinite);
			}
			else
			{
				SetDetailedStatus("Refreshing...");
				client.GetDetailedUserStatsAsync(detailedUsers);
			}
		}

		private void SetBriefStatus(string status) //hax
		{
			ExpanderBrief.Tag = status;
		}

		private void SetDetailedStatus(string status)
		{
			ExpanderDetailed.Tag = status;
		}

		private const int pageSize = 20;
		private Dictionary<int, BriefUserStats> lastBriefStatsDict = new Dictionary<int, BriefUserStats>();
		private void GetBriefUserStatsCompleted(object sender, GetBriefUserStatsCompletedEventArgs e)
		{
			dataGrid1Busy.IsBusy = false;
			//schedule next refresh
			wcfBriefRefreshTimer.Change(timerInterval, Timeout.Infinite);
			if (e.Error != null)
			{
				SetBriefStatus(e.Error.Message.Contains("CrossDomainError") || e.Error.Message.Contains("cross-domain") ? "CrossDomainError!" : "Error!");
				//MessageBox.Show(e.Error.ToString());
			}
			else
			{
				SetBriefStatus("Updating...");
				//BriefStatsGrid.SelectedIndex = -1;
				//System.Windows.Data.CollectionViewSource myCollectionViewSource = (System.Windows.Data.CollectionViewSource)this.Resources["briefUserStatsViewSource"];
				//myCollectionViewSource.Source = e.Result;
				if (BriefStatsGrid.ItemsSource == null)
				{
					var view = new PagedCollectionView(e.Result);
					lastBriefStatsDict = e.Result.ToDictionary(n => n.UserId);
					view.PageSize = pageSize;
					using (var refresh = view.DeferRefresh())
					{
						view.SortDescriptions.Add(new System.ComponentModel.SortDescription("UserName", System.ComponentModel.ListSortDirection.Ascending));
					}
					BriefStatsGrid.ItemsSource = view;
					BriefStatsPager.Source = view;
				}
				else
				{
					if (e.Result.Count == lastBriefStatsDict.Count
						&& e.Result.All(n => lastBriefStatsDict.ContainsKey(n.UserId)))
					{
						foreach (var newStats in e.Result)
						{
							lastBriefStatsDict[newStats.UserId].Update(newStats);
						}
					}
					else //reload data from scratch (this should be very rare)
					{
						var selected = BriefStatsGrid.SelectedItem as BriefUserStats;

						var source = (PagedCollectionView)BriefStatsGrid.ItemsSource;
						//var dict = source.SourceCollection.Cast<BriefUserStats>().Select((n, idx) => new { Idx = idx, Data = n }).ToDictionary(n => n.Data.UserId, n => n.Idx);
						var sortDesc = source.SortDescriptions.ToList();
						var pageIndex = source.PageIndex;
						foreach (var briefUserStats in e.Result)
						{
							briefUserStats.IsSelectedForDetails = detailedUsers.Contains(briefUserStats.UserId); //ugly hax
						}
						var view = new PagedCollectionView(e.Result);
						lastBriefStatsDict = e.Result.ToDictionary(n => n.UserId);
						view.PageSize = pageSize;
						foreach (var description in sortDesc)
						{
							view.SortDescriptions.Add(description);
						}
						view.MoveToPage(pageIndex);
						BriefStatsGrid.ItemsSource = view;
						BriefStatsPager.Source = view;

						if (selected != null)
						{
							BriefStatsGrid.SelectedItem = e.Result.Where(n => n.UserId == selected.UserId).FirstOrDefault();
						}
					}

				}

				SetBriefStatus("Done.");
			}
		}

		private void GetDetailedUserStatsCompleted(object sender, GetDetailedUserStatsCompletedEventArgs e)
		{
			//schedule next refresh
			wcfDetailedRefreshTimer.Change(timerInterval, Timeout.Infinite);
			if (e.Error != null)
			{
				SetDetailedStatus("Error!");
			}
			else
			{
				SetDetailedStatus("Updating...");
				foreach (var detailedUserStats in e.Result)
				{
					DetailedStatsDetailsView ctrl;
					if (!detailedStatsDetailsViewDict.TryGetValue(detailedUserStats.UserId, out ctrl))
					{
						continue; //we don't care anymore
					}
					detailedUserStats.ShowActivity = cbActivities.IsChecked.GetValueOrDefault(false); //local setting hax
					ctrl.DataContext = detailedUserStats;
				}
				SetDetailedStatus("Done.");
			}
		}

		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			loginWindow.Show();
			loginWindow.Focus();

			// Do not load your data at design time.
			// if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
			// {
			// 	//Load your data here and assign the result to the CollectionViewSource.
			// 	System.Windows.Data.CollectionViewSource myCollectionViewSource = (System.Windows.Data.CollectionViewSource)this.Resources["Resource Key for CollectionViewSource"];
			// 	myCollectionViewSource.Source = your data
			// }
		}

		private void loginWindow_Closed(object sender, EventArgs e)
		{
			if (!loginWindow.UserNamePassword.IsValid) return;
			client.ClientCredentials.UserName.UserName = loginWindow.UserNamePassword.UserName;
			client.ClientCredentials.UserName.Password = AuthenticationHelper.GetHashedHexString(loginWindow.UserNamePassword.Password);
			client.GetBriefUserStatsAsync(briefUsers);
			dataGrid1Busy.IsBusy = true;
		}



		private void BriefStatsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var grid = (DataGrid)sender;
			var stats = grid.SelectedItem as BriefUserStats;
			if (stats != null)
			{
				BriefStatsDetailsView.Visibility = Visibility.Visible;
				BriefStatsDetailsView.DataContext = stats;
			}
			else
			{
				BriefStatsDetailsView.Visibility = Visibility.Collapsed;
			}
		}

		private void DetailsCheckBox_Checked(object sender, RoutedEventArgs e)
		{
			var cb = (CheckBox)sender;
			var briefStats = cb.Tag as BriefUserStats;
			if (briefStats == null) return;
			if (detailedStatsDetailsViewDict.ContainsKey(briefStats.UserId)) return;
			detailedUsers.Add(briefStats.UserId);
			//todo briefUsers.Remove(briefStats.UserId);
			var ctx = briefStats.ToDetailedUserStats();
			ctx.ShowActivity = cbActivities.IsChecked.GetValueOrDefault(false);
			var ctrl = new DetailedStatsDetailsView() { DataContext = ctx, };
			SetSizeForDetailedView(ctrl);
			detailedStatsDetailsViewDict.Add(briefStats.UserId, ctrl);
			DetailsPanel.Children.Add(ctrl);
		}

		private void DetailsCheckBox_Unchecked(object sender, RoutedEventArgs e)
		{
			var cb = (CheckBox)sender;
			var briefStats = cb.Tag as BriefUserStats;
			if (briefStats == null) return;
			if (!detailedStatsDetailsViewDict.ContainsKey(briefStats.UserId)) return;
			detailedUsers.Remove(briefStats.UserId);
			//todo briefUsers.Add(briefStats.UserId);
			DetailsPanel.Children.Remove(detailedStatsDetailsViewDict[briefStats.UserId]);
			detailedStatsDetailsViewDict.Remove(briefStats.UserId);
		}

		private void FullScreen_Click(object sender, RoutedEventArgs e)
		{
			Application.Current.Host.Content.IsFullScreen = !Application.Current.Host.Content.IsFullScreen;
		}

		private void FiveScreens_Click(object sender, RoutedEventArgs e)
		{
			SetNumberOfDetailedViewInARow(5);
		}

		private void FourScreens_Click(object sender, RoutedEventArgs e)
		{
			SetNumberOfDetailedViewInARow(4);
		}

		private void ThreeScreens_Click(object sender, RoutedEventArgs e)
		{
			SetNumberOfDetailedViewInARow(3);
		}

		private void TwoScreens_Click(object sender, RoutedEventArgs e)
		{
			SetNumberOfDetailedViewInARow(2);
		}

		private void SetNumberOfDetailedViewInARow(int num)
		{
			var width = DetailsPanel.RenderSize.Width; //Application.Current.RootVisual.RenderSize.Width;
			currentWidth = width / num - 3;
			foreach (DetailedStatsDetailsView detailsView in DetailsPanel.Children)
			{
				SetSizeForDetailedView(detailsView);
			}
		}

		private void SetSizeForDetailedView(DetailedStatsDetailsView detailsView)
		{
			var viewBox = detailsView.GetLogicalChildrenBreadthFirst().OfType<Viewbox>().FirstOrDefault();
			viewBox.Width = currentWidth;
			viewBox.Height = currentWidth / heightDivisor;
		}

		private void SetActivityVisibilityForAllDetailedViews()
		{
			foreach (var ctx in detailedStatsDetailsViewDict.Values.Select(n => n.DataContext).OfType<DetailedUserStats>())
			{
				ctx.ShowActivity = cbActivities.IsChecked.GetValueOrDefault(false);
			}
		}

		private void cbActivities_Checked(object sender, RoutedEventArgs e)
		{
			SetActivityVisibilityForAllDetailedViews();
		}

		private void cbActivities_Unchecked(object sender, RoutedEventArgs e)
		{
			SetActivityVisibilityForAllDetailedViews();
		}

		private void KickButton_Click(object sender, RoutedEventArgs e)
		{
			var btn = (Button)sender;
			var kickWnd = new KickWindow();
			kickWnd.UserId = (int)VisualTreeHelperEx.GetParentOfType<ItemsControl>(btn).Tag; //quick and dirty hax
			kickWnd.ComputerId = (int)btn.Tag; //like wise
			kickWnd.Client = client;
			kickWnd.Show();
			kickWnd.Focus();
		}
	}
}
