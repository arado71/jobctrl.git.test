using System;
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
	public sealed partial class WorkRowLong : WorkRowBase
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private const double DragDeadzone = 8;
		private readonly WorkGrid container;
		private Point? dragStart = null;

		public Color Color
		{
			get { return workIcon1.Color; }

			private set { workIcon1.Color = value; }
		}

		public WorkRowLong()
		{
			InitializeComponent();
			BackColor = StyleUtils.Background;
			lblSmart.BackColor = StyleUtils.Background;
			lblSmart.ForeColor = StyleUtils.Foreground;
			lblSmart.ForeColorAlternative = StyleUtils.ForegroundLight;
			Localize();
		}

		public WorkRowLong(WorkGrid parent, NavigationBase navigation)
		{
			container = parent;
			InitializeComponent();
			SetColorScheme();

			Localize();
			Navigation = navigation;
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
			}
			else
			{
				BackColor = StyleUtils.Background;
				lblSmart.BackColor = StyleUtils.Background;
				lblSmart.ForeColor = StyleUtils.Foreground;
				lblSmart.ForeColorAlternative = StyleUtils.Shadow;
			}
		}

		protected override void OnCanFavoriteChanged()
		{
			btnFav.Visible = selected && navigation.CanFavorite;
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
			FormatTooltip();
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
			lblSmart.BackColor = selected ? StyleUtils.BackgroundHighlight : StyleUtils.Background;
			lblSmart.RenderText();
			BackColor = selected ? StyleUtils.BackgroundHighlight : StyleUtils.Background;
			pEdit.Visible = selected;
			btnFav.Visible = selected && navigation.CanFavorite;

			Invalidate();
		}

		protected override void RenderValue()
		{
		}

		private void MouseEntered()
		{
			if (Selected || !ClientRectangle.Contains(PointToClient(MousePosition)) ||
			    !Parent.Parent.ClientRectangle.Contains(Parent.Parent.PointToClient(MousePosition))) return;
			Selected = true;
		}

		private void MouseLeft()
		{
			if (ClientRectangle.Contains(PointToClient(MousePosition)) &&
			    Parent.Parent.ClientRectangle.Contains(Parent.Parent.PointToClient(MousePosition))) return;
			Selected = false;
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

		private void HandleEditMouseEnter(object sender, EventArgs e)
		{
			pEdit.BackgroundImage = Resources.details_hover;
			MouseEntered();
		}

		private void HandleEditMouseLeft(object sender, EventArgs e)
		{
			pEdit.BackgroundImage = Resources.details;
			MouseLeft();
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
			MouseEntered();
		}

		private void HandleMouseLeft(object sender, EventArgs e)
		{
			MouseLeft();
		}

		private void HandleMouseMove(object sender, MouseEventArgs e)
		{
			if ((container != null && container.Reorderable) && e.Button == MouseButtons.Left)
			{
				Point point = PointToClient(((Control) sender).PointToScreen(new Point(e.X, e.Y)));
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
			lblSmart.Clear().StartLineLimit(3).StartLineLimit(2);
			if (navigation.Path != null && navigation.Path.Any())
			{
				lblSmart.AddColorChange();
				foreach (var project in navigation.Path)
				{
					lblSmart.AddText(project + WorkDataWithParentNames.DefaultSeparator);
				}

				lblSmart.AddLineBreak().AddColorChange();
			}
			lblSmart.AddWeightChange().EndLineLimit().StartLineLimit(2).AddText(navigation.Name, true);
			lblSmart.RenderText();
			workIcon1.Initials = WorkIcon.GetInitials(navigation.Name);
			AccessibleName = navigation.Name;
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