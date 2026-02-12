using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Menu;
using Tct.ActivityRecorderClient.Properties;
using Tct.ActivityRecorderClient.Telemetry;
using Tct.ActivityRecorderClient.View.Navigation;

namespace Tct.ActivityRecorderClient.View.Controls
{
	public partial class CurrentWork : UserControl, ISelectionProvider<WorkDataWithParentNames>, ILocalizableControl
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private bool active = true;
		private WorkData currentWork = null;
		private bool isHovered = false;
		private NavigationBase navigationBase = null;

		public event EventHandler<WorkDataEventArgs> WorkClick;

		public NavigationFactory NavigationFactory { get; set; }

		public bool Active
		{
			get { return active; }

			set
			{
				if (active == value) return;
				active = value;
				workIcon.Color = active && navigationBase != null
					? StyleUtils.GetColor(navigationBase.Id)
					: StyleUtils.Shadow;
				SetActiveStateColors();
				Invalidate();
			}
		}

		private void SetActiveStateColors()
		{
			lblCompletion.BackColor = SystemInformation.HighContrast ? SystemColors.Window : active ? StyleUtils.Background : StyleUtils.BackgroundInactive;
			lblDeadline.BackColor = SystemInformation.HighContrast ? SystemColors.Window : active ? StyleUtils.Background : StyleUtils.BackgroundInactive;
			lblTask.BackColor = SystemInformation.HighContrast ? SystemColors.Window : active ? StyleUtils.Background : StyleUtils.BackgroundInactive;
			lblPrio.BackColor = SystemInformation.HighContrast ? SystemColors.Window : active ? StyleUtils.Background : StyleUtils.BackgroundInactive;
			lblTask.RenderText();
			lblDeadline.RenderText();
			lblCompletion.RenderText();
			BackColor = SystemInformation.HighContrast ? SystemColors.Window : active ? StyleUtils.Background : StyleUtils.BackgroundInactive;
		}

		public CurrentWork()
		{
			InitializeComponent();
			SetColorScheme();
			lblPrio.Font = StyleUtils.GetFont(FontStyle.Regular, 10.0f);
			workIcon.Visible = false;
			lblTask.Visible = false;
			lblPrio.Visible = false;
			pPrioIcon.Visible = false;
			Active = false;
			ApplyRecursive(this, c =>
			{
				c.MouseEnter += HandleMouseEntered;
				c.MouseLeave += HandleMouseLeft;
			});
			Localize();
		}

		public void Localize()
		{
			toolTip1.SetToolTip(btnFavorite, Labels.MenuFavorite);
			toolTip1.SetToolTip(pEdit, Labels.MenuDetails);
			RenderDeadline();
			if (navigationBase != null) SetTooltip();
		}

		public void SetColorScheme()
		{
			if (SystemInformation.HighContrast)
			{
				lblCompletion.ForeColor = SystemColors.WindowText;
				lblCompletion.ForeColorAlternative = SystemColors.HighlightText;
				lblDeadline.ForeColor = SystemColors.WindowText;
				lblDeadline.ForeColorAlternative = SystemColors.HighlightText;
				lblTask.ForeColor = SystemColors.WindowText;
				lblTask.ForeColorAlternative = SystemColors.HighlightText;
				lblPrio.BackColor = SystemColors.Window;
				lblPrio.ForeColor = SystemColors.WindowText;
				BackColor = SystemColors.Window;
			}
			else
			{
				lblCompletion.ForeColor = StyleUtils.ForegroundDark;
				lblCompletion.ForeColorAlternative = StyleUtils.ForegroundLight;
				lblDeadline.ForeColor = StyleUtils.ForegroundDark;
				lblDeadline.ForeColorAlternative = StyleUtils.ForegroundLight;
				lblTask.ForeColor = StyleUtils.Foreground;
				lblTask.ForeColorAlternative = StyleUtils.ForegroundDark;
				lblPrio.BackColor = Color.Transparent;
				lblPrio.ForeColor = StyleUtils.Foreground;
			}
			SetActiveStateColors();
		}

		public WorkDataWithParentNames Selection
		{
			get
			{
				if (!isHovered) return null;
				var navWithWork = navigationBase as INavigationWithWork;
				Debug.Assert(navWithWork != null);
				return navWithWork.Work;
			}
		}

		public void ClearSelection()
		{
			OnLeave();
		}

		public void SetWork(WorkData work)
		{
			var oldBase = navigationBase;
			if (oldBase != null)
			{
				oldBase.PropertyChanged -= HandleNavigationChanged;
				NavigationFactory.Release(oldBase);
				navigationBase = null;
			}

			currentWork = work;
			if (work == null)
			{
				pEdit.Visible = false;
				btnFavorite.Visible = false;
				lblTask.Visible = false;
				workIcon.Visible = false;
				pPrioIcon.Visible = false;
				lblPrio.Visible = false;
				lblDeadline.Visible = false;
				pbDeadline.Visible = false;
				lblCompletion.Visible = false;
				pbCompletion.Visible = false;
				return;
			}

			Debug.Assert(work.Id != null);
			navigationBase = NavigationFactory.Get(LocationKey.CreateFrom(work));
			if (navigationBase != null)
			{
				navigationBase.SimulateChange(oldBase, HandleNavigationChanged);
				navigationBase.PropertyChanged += HandleNavigationChanged;
			}

			lblTask.Visible = true;
			workIcon.Visible = true;
		}

		protected virtual void OnEndDateChanged()
		{
			RenderDeadline();
		}

		protected virtual void OnIdChanged()
		{
			Debug.Assert(navigationBase != null);
			workIcon.Color = active
				? StyleUtils.GetColor(navigationBase.Id)
				: StyleUtils.Shadow;
		}

		protected virtual void OnIsFavoriteChanged()
		{
			Debug.Assert(navigationBase != null);
			btnFavorite.IsFavorite = navigationBase.IsFavorite;
		}

		protected virtual void OnNameChanged()
		{
			Debug.Assert(navigationBase != null);
			RenderProjectName(navigationBase.Path, navigationBase.Name);
			SetTooltip();
		}

		protected virtual void OnPathChanged()
		{
			Debug.Assert(navigationBase != null);
			RenderProjectName(navigationBase.Path, navigationBase.Name);
			SetTooltip();
		}

		private void SetTooltip()
		{
			var tooltip = FormatHelper.GetDesc(navigationBase);
			toolTip1.SetToolTip(lblTask, tooltip);
			toolTip1.SetToolTip(this, tooltip);
			toolTip1.SetToolTip(workIcon, tooltip);
		}

		protected virtual void OnPriorityChanged()
		{
			Debug.Assert(navigationBase != null);
			RenderPriority();
			SetTooltip();
		}

		protected virtual void OnRemainingTimeChanged()
		{
			Debug.Assert(navigationBase != null);
			RenderDeadline();
			SetTooltip();
		}

		protected virtual void OnStartDateChanged()
		{
			Debug.Assert(navigationBase != null);
			RenderDeadline();
			SetTooltip();
		}

		protected virtual void OnTotalTimeChanged()
		{
			Debug.Assert(navigationBase != null);
			RenderProgress();
			SetTooltip();
		}

		protected virtual void OnUsedTimeChanged()
		{
			Debug.Assert(navigationBase != null);
			RenderProgress();
			SetTooltip();
		}

		protected virtual void OnIsEditableChanged()
		{
			if (navigationBase == null) return;
			pEdit.Visible = isHovered;
		}

		private void HandleClicked(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left) return;
			log.Debug("UI - Element clicked");
			TelemetryHelper.RecordFeature("CurrentWork", "Start");
			OnLeave();
			RaiseTaskClicked();
		}

		private void HandleEditClicked(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left && currentWork != null)
			{
				log.Debug("UI - Edit clicked");
				TelemetryHelper.RecordFeature("CurrentWork", "Details");
				OnLeave();
				var navigationWithWork = navigationBase as INavigationWithWork;
				if (navigationWithWork == null || navigationWithWork.Work == null || navigationWithWork.Work.WorkData == null)
				{
					log.WarnAndFail("Edit clicked in impossible state");
					return;
				}

				Capturing.Core.CaptureCoordinator.Instance.WorkManagementService.DisplayWorkDetailsGui(navigationWithWork.Work.WorkData);
			}
		}

		private void HandleEditMouseEnter(object sender, EventArgs e)
		{
			pEdit.BackgroundImage = Resources.details_hover;
		}

		private void HandleEditMouseLeave(object sender, EventArgs e)
		{
			pEdit.BackgroundImage = Resources.details;
		}

		private void HandleFavoriteClicked(object sender, MouseEventArgs e)
		{
			Debug.Assert(navigationBase != null);
			if (e.Button == MouseButtons.Left && navigationBase != null)
			{
				log.Debug("UI - Favorite clicked");
				navigationBase.IsFavorite = !navigationBase.IsFavorite;
				//isHovered = false;
			}
		}

		private void HandleMouseEntered(object sender, EventArgs e)
		{
			if (navigationBase == null) return;
			isHovered = true;
			btnFavorite.Visible = true;
			pEdit.Visible = true;
		}

		private void HandleMouseLeft(object sender, EventArgs e)
		{
			if (ClientRectangle.Contains(PointToClient(MousePosition))) return;
			OnLeave();
		}

		private void HandleNavigationChanged(object sender, PropertyChangedEventArgs e)
		{
			Debug.Assert(navigationBase != null);
			switch (e.PropertyName)
			{
				case "Id":
					OnIdChanged();
					break;
				case "Name":
					OnNameChanged();
					break;
				case "Path":
					OnPathChanged();
					break;
				case "IsFavorite":
					OnIsFavoriteChanged();
					break;
				case "TotalTime":
					OnTotalTimeChanged();
					break;
				case "UsedTime":
					OnUsedTimeChanged();
					break;
				case "RemainingTime":
					OnRemainingTimeChanged();
					break;
				case "StartDate":
					OnStartDateChanged();
					break;
				case "Priority":
					OnPriorityChanged();
					break;
				case "EndDate":
					OnEndDateChanged();
					break;
			}
		}

		private void ApplyRecursive(Control c, Action<Control> action)
		{
			action(c);
			foreach (Control child in c.Controls)
			{
				ApplyRecursive(child, action);
			}
		}

		private void OnLeave()
		{
			isHovered = false;
			btnFavorite.Visible = false;
			pEdit.Visible = false;
		}

		private void RaiseTaskClicked()
		{
			if (navigationBase == null || currentWork == null) return;
			var evt = WorkClick;
			if (evt != null)
				evt(this,
					new WorkDataEventArgs(currentWork,
						MenuQuery.Instance.ClientMenuLookup.Value.WorkDataById.ContainsKey(navigationBase.Id)));
		}

		private void RenderDeadline()
		{
			lblDeadline.Visible =
				pbDeadline.Visible = navigationBase != null && navigationBase.StartDate != null && navigationBase.EndDate != null;
			if (navigationBase == null) return;
			if (navigationBase.StartDate == null || navigationBase.EndDate == null) return;
			TimeSpan proc = DateTime.Now - navigationBase.StartDate.Value;
			TimeSpan total = navigationBase.EndDate.Value.AddDays(1) - navigationBase.StartDate.Value;
			pbDeadline.Value = (float)(proc.TotalMilliseconds / total.TotalMilliseconds);
			lblDeadline.Clear();
			lblDeadline.AddText(FormatHelper.GetRemainingTime(total - proc));
			if ((total - proc).Ticks > 0)
			{
				lblDeadline.AddColorChange();
				lblDeadline.AddWeightChange();
				lblDeadline.AddText(string.Format(" / {0}", FormatHelper.GetDays(total)));
			}

			lblDeadline.RenderText();
		}

		private void RenderPriority()
		{
			lblPrio.Visible = pPrioIcon.Visible = navigationBase != null && navigationBase.Priority.HasValue;
			if (navigationBase == null || navigationBase.Priority == null) return;
			lblPrio.Text = navigationBase.Priority.Value.ToString();
		}

		private void RenderProgress()
		{
			lblCompletion.Visible = pbCompletion.Visible = navigationBase != null && navigationBase.TotalTime.HasValue;
			if (navigationBase == null || !navigationBase.TotalTime.HasValue) return;
			pbCompletion.Value =
				(float)(navigationBase.UsedTime.TotalMilliseconds / navigationBase.TotalTime.Value.TotalMilliseconds);
			lblCompletion.Clear();
			lblCompletion.AddWeightChange();
			lblCompletion.AddText(navigationBase.UsedTime.ToHourMinuteString());
			lblCompletion.AddColorChange();
			lblCompletion.AddText(string.Format(" / {0}", navigationBase.TotalTime.Value.ToHourMinuteString()));
			lblCompletion.RenderText();
		}

		private void RenderProjectName(IEnumerable<string> path, string projectName)
		{
			lblTask.Clear().StartLineLimit(3).StartLineLimit(2);
			foreach (var project in path)
			{
				lblTask.AddText(project + WorkDataWithParentNames.DefaultSeparator);
			}

			lblTask.AddColorChange();
			lblTask.AddWeightChange();
			lblTask.AddLineBreak().EndLineLimit().StartLineLimit(2);
			lblTask.AddText(projectName, true);
			lblTask.RenderText();
			workIcon.Initials = WorkIcon.GetInitials(projectName);
		}
	}
}