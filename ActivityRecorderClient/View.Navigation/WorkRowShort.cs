using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using log4net;
using Tct.ActivityRecorderClient.Menu;
using Tct.ActivityRecorderClient.Properties;
using Tct.ActivityRecorderClient.View.Controls;

namespace Tct.ActivityRecorderClient.View.Navigation
{
	public sealed partial class WorkRowShort : WorkRowBase
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private const double DragDeadzone = 8;

		private readonly WorkGrid container;
		private Point? dragStart = null;
		private Color unselectedBackgroundColor = StyleUtils.Background;
		private Color selectedBackgroundColor = StyleUtils.BackgroundHighlight;

		public Color Color
		{
			get { return workIcon1.Color; }

			private set { workIcon1.Color = value; }
		}

		public Color UnselectedBackgroundColor
		{
			get { return unselectedBackgroundColor; }
			set
			{
				unselectedBackgroundColor = value;
				if (selected) return;
				lblSmart.BackColor = value;
				lblSmart.RenderText();
				BackColor = value;
			}
		}

		public Color SelectedBackgroundColor
		{
			get { return selectedBackgroundColor; }
			set
			{
				selectedBackgroundColor = value;
				if (!selected) return;
				lblSmart.BackColor = value;
				lblSmart.RenderText();
				BackColor = value;
			}
		}

		public WorkRowShort()
		{
			InitializeComponent();
			BackColor = unselectedBackgroundColor;
			lblSmart.BackColor = unselectedBackgroundColor;
			lblSmart.ForeColor = StyleUtils.Foreground;
			lblSmart.ForeColorAlternative = StyleUtils.ForegroundLight;
			Localize();
		}

		public WorkRowShort(WorkGrid parent, NavigationBase navigation)
		{
			container = parent;
			InitializeComponent();
			SetColorScheme();
			Navigation = navigation;
			RenderValue();
			Localize();
		}

		public override void Localize()
		{
			base.Localize();
			toolTip1.SetToolTip(btnFav, Labels.MenuFavorite);
			toolTip1.SetToolTip(pEdit, Labels.MenuDetails);
			if (navigation != null) FormatTooltip();
		}

		public override void SetColorScheme()
		{
			if (SystemInformation.HighContrast)
			{
				BackColor = SystemColors.Window;
				lblSmart.BackColor = SystemColors.Window;
				lblSmart.ForeColor = SystemColors.WindowText;
				lblSmart.ForeColorAlternative = SystemColors.HighlightText;
				unselectedBackgroundColor = SystemColors.Window;
				selectedBackgroundColor = SystemColors.Highlight;
			}
			else
			{
				BackColor = unselectedBackgroundColor;
				lblSmart.BackColor = unselectedBackgroundColor;
				lblSmart.ForeColor = StyleUtils.Foreground;
				lblSmart.ForeColorAlternative = StyleUtils.Shadow;
				unselectedBackgroundColor = StyleUtils.Background;
				selectedBackgroundColor = StyleUtils.BackgroundHighlight;
			}
		}

		protected override void OnCanFavoriteChanged()
		{
			btnFav.Visible = navigation.CanFavorite && selected;
		}

		protected override void OnEndDateChanged()
		{
			FormatTooltip();
		}

		protected override void OnIconChanged()
		{
			workIcon1.Visible = navigation.Icon == null;
			pbIcon.Image = navigation.Icon;
			pbIcon.Visible = navigation.Icon != null;
		}

		protected override void OnIdChanged()
		{
			Color = StyleUtils.GetColor(navigation.Id);
		}

		protected override void OnIsFavoriteChanged()
		{
			btnFav.IsFavorite = navigation.IsFavorite;
		}

		protected override void OnIsWorkChanged()
		{
			workIcon1.AlternativeStyle = !navigation.IsWork;
			FormatLabel();
			RenderSelection();
		}

		protected override void OnNameChanged()
		{
			FormatLabel();
			FormatTooltip();
		}

		protected override void OnPathChanged()
		{
			FormatLabel();
			FormatTooltip();
		}

		protected override void OnPriorityChanged()
		{
			FormatTooltip();
		}

		protected override void OnStartDateChanged()
		{
			FormatTooltip();
		}

		protected override void OnTotalTimeChanged()
		{
			FormatTooltip();
		}

		protected override void OnUsedTimeChanged()
		{
			FormatTooltip();
		}

		protected override void RenderSelection()
		{
			Debug.Assert(navigation != null);
			lblSmart.BackColor = selected ? SelectedBackgroundColor : UnselectedBackgroundColor;
			lblSmart.RenderText();
			BackColor = selected ? SelectedBackgroundColor : UnselectedBackgroundColor;
			pEdit.Visible = selected;
			btnFav.Visible = navigation.CanFavorite && selected;
			Invalidate();
		}

		protected override void RenderValue()
		{
		}

		private void HandleClicked(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				log.Debug("UI - Element clicked");
				Selected = false;
				navigation.Navigate();
			}
			else
			{
				log.DebugFormat("UI - Element wrongly clicked with {0}", e.Button);
			}
		}

		private void HandleEditClicked(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				log.Debug("UI - Edit clicked");
				ClientMenuLookup lookup = MenuQuery.Instance.ClientMenuLookup.Value;
				if (lookup.WorkDataById.ContainsKey(navigation.Id))
				{
					Capturing.Core.CaptureCoordinator.Instance.WorkManagementService.DisplayWorkDetailsGui(lookup.WorkDataById[navigation.Id].WorkData);
				}
			}
			else
			{
				log.DebugFormat("UI - Edit wrongly clicked with {0}", e.Button);
			}
		}

		private void HandleEditMouseEntered(object sender, EventArgs e)
		{
			pEdit.BackgroundImage = Resources.details_hover;
		}

		private void HandleEditMouseLeft(object sender, EventArgs e)
		{
			pEdit.BackgroundImage = Resources.details;
		}

		private void HandleFavoriteClicked(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				log.Debug("UI - Favorite clicked");
				navigation.IsFavorite = !navigation.IsFavorite;
			}
			else
			{
				log.DebugFormat("UI - Favorite wrongly clicked with {0}", e.Button);
			}
		}

		private void HandleMouseEntered(object sender, EventArgs e)
		{
			if (Selected || !ClientRectangle.Contains(PointToClient(MousePosition)) ||
			    !Parent.Parent.ClientRectangle.Contains(Parent.Parent.PointToClient(MousePosition))) return;
			Selected = true;
		}

		private void HandleMouseLeft(object sender, EventArgs e)
		{
			if (ClientRectangle.Contains(PointToClient(MousePosition)) &&
			    Parent.Parent.ClientRectangle.Contains(Parent.Parent.PointToClient(MousePosition))) return;
			Selected = false;
		}

		private void HandleMouseMove(object sender, MouseEventArgs e)
		{
			if ((container != null && container.Reorderable) && e.Button == MouseButtons.Left)
			{
				Point point = PointToClient(((Control)sender).PointToScreen(new Point(e.X, e.Y)));
				if (dragStart == null)
				{
					dragStart = point;
					return;
				}

				if (GetDistance(dragStart.Value, point) > DragDeadzone)
				{
					btnFav.Visible = false;
					pEdit.Visible = false;
					container.StartDrag(this, dragStart);
					btnFav.Visible = true;
					pEdit.Visible = true;
				}
			}

			if (e.Button != MouseButtons.Left)
			{
				dragStart = null;
			}
		}

		private void FormatLabel()
		{
			lblSmart.Clear();
			lblSmart.AddWeightChange().StartLineLimit(2).AddText(navigation.Name, true).RenderText();
			workIcon1.Initials = WorkIcon.GetInitials(navigation.Name);
			AccessibleName = navigation?.Name;
		}

		protected override void OnGotFocus(EventArgs e)
		{
			base.OnGotFocus(e);
			if (navigation == null) return;
			Selected = true;
		}

		protected override void OnLostFocus(EventArgs e)
		{
			base.OnLostFocus(e);
			if (navigation == null) return;
			Selected = false;
		}

		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			base.OnKeyPress(e);
			if (e.KeyChar == '\r' || e.KeyChar == ' ')
			{
				e.Handled = true;
				log.Debug("UI - Element pressed");
				Selected = false;
				navigation.Navigate();
			}
		}

		private void FormatTooltip()
		{
			var toolTip = navigation.IsWork ? FormatHelper.GetDesc(navigation) : string.Empty;
			toolTip1.SetToolTip(this, toolTip);
			toolTip1.SetToolTip(lblSmart, toolTip);
			toolTip1.SetToolTip(workIcon1, toolTip);
		}

		private double GetDistance(Point p1, Point p2)
		{
			return Math.Round(Math.Sqrt(Math.Pow((p2.X - p1.X), 2) + Math.Pow((p2.Y - p1.Y), 2)), 1);
		}
	}
}