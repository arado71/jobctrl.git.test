using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Menu;
using Tct.ActivityRecorderClient.Properties;
using Tct.ActivityRecorderClient.Search;
using Tct.ActivityRecorderClient.Telemetry;
using Tct.ActivityRecorderClient.View.Navigation;
using Screen = System.Windows.Forms.Screen;

namespace Tct.ActivityRecorderClient.View.Controls
{
	public partial class SearchBox : UserControl, ISelectionProvider<WorkDataWithParentNames>
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private const int MaxItems = 15;

		private static readonly SolidBrush foreBrush = new SolidBrush(StyleUtils.ForegroundLight);
		private static readonly SolidBrush shadowBrush = new SolidBrush(StyleUtils.Shadow);
		private readonly Bitmap leftEdge;
		private readonly StringMatcher matcher = new WorkNameMatcher();
		private readonly SearchResult resultWindow = new SearchResult();
		private readonly Bitmap rightEdge;
		private ClientMenuLookup clientMenuLookup = null;
		private ClientMenuLookup clientMenuLookupOwn = null;
		private bool forceDropdownFocus = false;

		public event EventHandler EmptyEscapePressed;
		public event EventHandler OnDropdownHidden;

		public INavigator Navigator
		{
			get { return resultWindow.Navigator; }

			set { resultWindow.Navigator = value; }
		}

		public bool DropdownShown
		{
			get { return resultWindow.Visible || forceDropdownFocus; }
		}

		public bool DropdownFocus
		{
			get { return forceDropdownFocus || resultWindow.HasFocus || resultWindow.IsForeground; }
		}

		public override string Text
		{
			get { return txtInput.Text; }

			set { txtInput.Text = value; }
		}

		public SearchBox()
		{
			InitializeComponent();
			txtInput.SetCueBanner(Labels.SearchForWork);
			resultWindow.GotFocus += (_, __) => txtInput.Focus();
			MenuQuery.Instance.ClientMenuLookup.Changed += (_, __) => UpdateClientMenu(MenuQuery.Instance.ClientMenuLookup.Value);
			txtInput.BackColor = StyleUtils.ForegroundLight;
			txtInput.ForeColor = StyleUtils.Foreground;
			txtInput.Font = StyleUtils.GetFont(FontStyle.Light, 10f);
			leftEdge = Resources.searchFrame.Clone() as Bitmap;
			if (leftEdge == null) return;
			rightEdge = leftEdge.Clone() as Bitmap;
			if (rightEdge == null) return;
			rightEdge.RotateFlip(RotateFlipType.RotateNoneFlipX);
		}

		public WorkDataWithParentNames Selection
		{
			get { return resultWindow.Selection; }
		}

		public void ClearSelection()
		{
			resultWindow.ClearSelection();
		}

		public void HideDropdown()
		{
			HideResults();
		}

		public void UpdateClientMenu(ClientMenuLookup clientMenuLookup)
		{
			if (ConfigManager.LocalSettingsForUser.SearchOwnTasks && !ConfigManager.LocalSettingsForUser.SearchInClosed)
			{
				this.clientMenuLookup = clientMenuLookup;
				UpdateMatcher(clientMenuLookup.WorkDataById.Where(kv => kv.Value.WorkData.IsVisibleInMenu).Select(kv => kv.Value));
			}

			clientMenuLookupOwn = clientMenuLookup;
			RefreshResults();
		}

		private void HandleFocused(object sender, EventArgs e)
		{
			if (!forceDropdownFocus)
			{
				RefreshResults();
			}
		}

		private void HandleInputChanged(object sender, EventArgs e)
		{
			delayTimer.Start();
		}

		private void HandleInputKeyPressing(object sender, KeyEventArgs e)
		{
			switch (e.KeyCode)
			{
				case Keys.Up:
					log.Debug("UI - Search up pressed");
					e.SuppressKeyPress = true;
					if (string.IsNullOrEmpty(txtInput.Text) && !DropdownShown)
					{
						CreateDefaultList();
					}

					if (!DropdownShown)
					{
						RefreshResults();
					}

					resultWindow.SelectPrevious();
					break;
				case Keys.Down:
					log.Debug("UI - Search down pressed");
					e.SuppressKeyPress = true;
					if (string.IsNullOrEmpty(txtInput.Text) && !DropdownShown)
					{
						CreateDefaultList();
					}

					if (!DropdownShown)
					{
						RefreshResults();
					}

					resultWindow.SelectNext();
					break;
				case Keys.Escape:
					log.Debug("UI - Search esc pressed");
					e.SuppressKeyPress = true;
					if (DropdownShown)
					{
						HideDropdown();
					}
					else
					{
						if (!string.IsNullOrEmpty(txtInput.Text))
						{
							txtInput.Clear();
							txtInput.Select(0, 0);
						}
						else
						{
							EventHandler evt = EmptyEscapePressed;
							if (evt != null) evt(this, EventArgs.Empty);
						}
					}

					break;
				case Keys.Enter:
					TelemetryHelper.RecordFeature("Search", "StartKeyboard");
					log.Debug("UI - Search enter pressed");
					e.SuppressKeyPress = true;
					if (delayTimer.Enabled)
					{
						delayTimer.Stop();
						RefreshResults();
					}
					NavigationBase currentWork = resultWindow.GetSelection();
					if (currentWork != null)
					{
						currentWork.Navigate();
					}

					if (!string.IsNullOrEmpty(txtInput.Text))
					{
						txtInput.Clear();
						txtInput.Select(0, 0);
					}
					break;
			}
		}

		private void HandleInputKeyReleasing(object sender, KeyEventArgs e)
		{
			switch (e.KeyCode)
			{
				case Keys.Up:
				case Keys.Down:
					e.SuppressKeyPress = true;
					break;
			}
		}

		private void HandleLostFocus(object sender, EventArgs e)
		{
			if (!forceDropdownFocus)
			{
				HideResults();
			}
		}

		private void HandlePainting(object sender, PaintEventArgs e)
		{
			e.Graphics.FillRectangle(foreBrush, 0, 0, Width, Height);
			e.Graphics.FillRectangle(shadowBrush, 0, 0, Width, 2);
			if (leftEdge == null || rightEdge == null) return;
			e.Graphics.DrawImage(leftEdge, Point.Empty);
			e.Graphics.DrawImage(rightEdge, new Point(Width - rightEdge.Width, 0));
		}

		private void CreateDefaultList()
		{
			ShowResults(RecentHelper.GetRecents());
		}

		private void HideResults()
		{
			if (DropdownShown)
			{
				resultWindow.Hide();
				resultWindow.SelectedIndex = -1;
				EventHandler evt = OnDropdownHidden;
				if (evt != null) evt(this, EventArgs.Empty);
			}
		}

		private void RefreshResults()
		{
			List<WorkDataWithParentNames> results = matcher.GetMatches(Text)
				.Select(matchedId => clientMenuLookup.GetWorkDataWithParentNames(matchedId))
				.Where(matched => matched != null)
				.Where(matched => 
					ConfigManager.LocalSettingsForUser.ShowDynamicWorks 
					|| (matched.WorkData != null 
						&& matched.WorkData.Id != null
						&&!clientMenuLookup.IsDynamicWork(matched.WorkData.Id.Value)))
				.Take(MaxItems).ToList();

			if (results.Any() && txtInput.Focused)
			{
				ShowResults(results);
			}
			else
			{
				HideResults();
			}
		}

		private void ShowResults(IEnumerable<WorkDataWithParentNames> results)
		{
			WorkDataWithParentNames[] resultsArr = results as WorkDataWithParentNames[] ?? results.ToArray();
			if (!DropdownShown && resultsArr.Any())
			{
				Point position = PointToScreen(Point.Empty);
				resultWindow.DesktopLocation = new Point(position.X, position.Y + Height);
				resultWindow.Offset = resultWindow.DesktopLocation;
				resultWindow.Width = Screen.FromPoint(Location).WorkingArea.Width;
				forceDropdownFocus = true;
				resultWindow.Show(this);
				txtInput.Focus();
				forceDropdownFocus = false;
				TelemetryHelper.RecordFeature("Search", "ShowResults");
			}

			resultWindow.Populate(resultsArr);
			txtInput.Focus();
		}


		private void UpdateMatcher(IEnumerable<WorkDataWithParentNames> works)
		{
			matcher.Clear();
			foreach (var workWithParent in works)
			{
				matcher.Add(workWithParent.WorkData.Id.Value, workWithParent.FullName);
				//we cannot search for WorkDataWithParentNames.DefaultSeparator if it is in the name (but I can live with that)
			}
		}

		internal void UpdateAllWorks(List<WorkData> allWorkDatas)
		{
			clientMenuLookup = new ClientMenuLookup { ClientMenu = new ClientMenu { Works = allWorkDatas } };
			UpdateMatcher(clientMenuLookup.WorkDataById.Where(kv => !IsOwnWork(kv.Value) || GetOwnWork(kv.Value).WorkData.IsVisibleInMenu).Select(kv => kv.Value));
		}

		private bool IsOwnWork(WorkDataWithParentNames work)
		{
			Debug.Assert(work != null && work.WorkData != null && work.WorkData.Id.HasValue);
			return clientMenuLookupOwn.WorkDataById.ContainsKey(work.WorkData.Id.Value);
		}

		private WorkDataWithParentNames GetOwnWork(WorkDataWithParentNames work)
		{
			Debug.Assert(work != null && work.WorkData != null && work.WorkData.Id.HasValue);
			return clientMenuLookupOwn.GetWorkDataWithParentNames(work.WorkData.Id.Value);
		}

		private void HandleDelayTimerTick(object sender, EventArgs e)
		{
			delayTimer.Stop();
			RefreshResults();
		}

		private void HandleLoad(object sender, EventArgs e)
		{
			resultWindow.Owner = ParentForm;
		}
	}
}