using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Forms;
using log4net;
using Tct.ActivityRecorderClient.Menu;
using Tct.ActivityRecorderClient.Properties;
using Tct.ActivityRecorderClient.View.Controls;

namespace Tct.ActivityRecorderClient.View.Navigation
{
	public sealed partial class WorkRowDetailed : WorkRowBase
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private readonly WorkGrid container;
		private readonly NavigationBase.RenderHint hint;
		private string additional;

		public Color Color
		{
			get { return workIcon1.Color; }

			private set { workIcon1.Color = value; }
		}

		public string Additional
		{
			get { return additional; }

			set
			{
				additional = value;
				FormatAdditional();
			}
		}

		public bool NegativeAdditional
		{
			set { lblAdditional.ForeColor = value ? StyleUtils.Negative : StyleUtils.ForegroundHighlight; }
		}

		public WorkRowDetailed()
		{
			InitializeComponent();
			BackColor = StyleUtils.Background;
			lblSmart.BackColor = StyleUtils.Background;
			lblSmart.ForeColor = StyleUtils.Foreground;
			lblSmart.ForeColorAlternative = StyleUtils.ForegroundLight;
			lblAdditional.ForeColor = StyleUtils.ForegroundHighlight;
			lblAdditional.BackColor = StyleUtils.Background;
			Localize();
		}

		public WorkRowDetailed(WorkGrid parent, NavigationBase navigation, NavigationBase.RenderHint hint)
		{
			container = parent;
			this.hint = hint;
			BackColor = StyleUtils.Background;
			InitializeComponent();
			SetColorScheme();
			RenderValue();
			Localize();
			Navigation = navigation;
		}

		public override void Localize()
		{
			base.Localize();
			toolTip1.SetToolTip(favBtn, Labels.MenuFavorite);
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
				lblSmart.ForeColorAlternative = SystemColors.WindowText;
				lblAdditional.ForeColor = SystemColors.HighlightText;
				lblAdditional.BackColor = SystemColors.Window;
			}
			else
			{
				BackColor = StyleUtils.Background;
				lblSmart.BackColor = StyleUtils.Background;
				lblSmart.ForeColor = StyleUtils.Foreground;
				lblSmart.ForeColorAlternative = StyleUtils.Shadow;
				lblAdditional.ForeColor = StyleUtils.ForegroundHighlight;
				lblAdditional.BackColor = StyleUtils.Background;
			}
		}

		protected override void OnCanFavoriteChanged()
		{
			favBtn.Visible = selected && navigation.CanFavorite;
		}

		protected override void OnIdChanged()
		{
			Color = StyleUtils.GetColor(navigation.Id);
			FormatTooltip();
		}

		protected override void OnIsFavoriteChanged()
		{
			favBtn.IsFavorite = navigation.IsFavorite;
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
			if (hint == NavigationBase.RenderHint.Priority)
			{
				Additional = navigation.Priority != null
					? string.Format("{1}: {0}", navigation.Priority.Value, Labels.MenuPriority)
					: string.Empty;
				NegativeAdditional = false;
			}

			FormatTooltip();
		}

		protected override void OnRemainingTimeChanged()
		{
			if (hint == NavigationBase.RenderHint.Remaining)
			{
				Additional = navigation.RemainingTime != null
					? FormatHelper.GetRemainingTime(navigation.RemainingTime.Value)
					: string.Empty;
				NegativeAdditional = navigation.RemainingTime.HasValue && navigation.RemainingTime.Value.Ticks < 0;
			}

			FormatTooltip();
		}

		protected override void OnTotalTimeChanged()
		{
			ProgressChanged();
			FormatTooltip();
		}

		protected override void OnUsedTimeChanged()
		{
			ProgressChanged();
			FormatTooltip();
		}

		protected override void RenderSelection()
		{
			Debug.Assert(navigation != null);
			lblSmart.BackColor = selected ? StyleUtils.BackgroundHighlight : StyleUtils.Background;
			lblSmart.RenderText();
			lblAdditional.BackColor = selected ? StyleUtils.BackgroundHighlight : StyleUtils.Background;
			lblAdditional.RenderText();
			BackColor = selected ? StyleUtils.BackgroundHighlight : StyleUtils.Background;
			favBtn.Visible = selected && navigation.CanFavorite;
			pEdit.Visible = selected;
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
				log.DebugFormat("UI - Favorite wrongly clicked with {0}", e.Button);
			}
		}

		private void HandleEditMouseEntered(object sender, EventArgs e)
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

		private void FormatAdditional()
		{
			lblAdditional.Clear();
			if (!string.IsNullOrEmpty(additional))
			{
				lblAdditional.AddText(additional);
			}

			lblAdditional.RenderText();
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
			AccessibleName = navigation?.Name;
		}

		private void FormatTooltip()
		{
			var toolTip = FormatHelper.GetDesc(navigation);
			toolTip1.SetToolTip(this, toolTip);
			toolTip1.SetToolTip(lblSmart, toolTip);
			toolTip1.SetToolTip(lblAdditional, toolTip);
			toolTip1.SetToolTip(workIcon1, toolTip);
		}

		private void ProgressChanged()
		{
			if (hint == NavigationBase.RenderHint.Progress)
			{
				if (navigation.TotalTime != null)
				{
					var prog = navigation.UsedTime.Ticks / (double)navigation.TotalTime.Value.Ticks;
					Additional = string.Format("{1}: {0:P1}", prog, Labels.MenuCompleted);
					NegativeAdditional = prog > 1.0;
				}
				else
				{
					Additional = string.Empty;
				}
			}
		}
	}
}