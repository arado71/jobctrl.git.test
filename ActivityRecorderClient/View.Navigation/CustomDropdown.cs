using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Tct.ActivityRecorderClient.View.Controls;

namespace Tct.ActivityRecorderClient.View.Navigation
{
	public enum DropdownPosition
	{
		TopLeft,
		TopRight,
		BottomLeft,
		BottomRight,
		BottomMiddle
	}

	// todo Register leaks memory, unregister at dispose (applied to lifelong objects atm)
	public sealed partial class CustomDropdown<T> : Form, ISelectionProvider<T>, IDropdown
		where T : IEquatable<T>
	{
		public const int PaddingWidth = 6;
		public const int PaddingAll = 3;

		private Control parent = null;
		private DropdownPosition position;
		private SelectableControl<T> selectedRow = null;

		public CustomDropdown(IDropdownContainer parent)
		{
			InitializeComponent();
			parent.RegisterDropdown(this);
			BackColor = StyleUtils.Shadow;
			panel1.BackColor = StyleUtils.Background;
		}

		public event EventHandler Hidden;

		public bool IsShown { get; set; }

		public T Selection
		{
			get { return selectedRow != null ? selectedRow.Value : default(T); }
		}

		public IEnumerable<SelectableControl<T>> GetElements()
		{
			return tablePanel1.Controls.Cast<Control>().Select(x => x as SelectableControl<T>).Where(x => x != null);
		}

		public bool HasElements()
		{
			return tablePanel1.Controls.Count > 0;
		}

		public void Populate(IEnumerable<SelectableControl<T>> group) // Caller responsible for Dispose
		{
			ClearControls();
			int suggestedWidth = 0;
			PopulateGroup(group, ref suggestedWidth);
			Width = suggestedWidth + PaddingWidth + 2*PaddingAll;
			Height = tablePanel1.PreferredSize.Height + 2*PaddingAll;
			MoveToPosition();
			PlaceOnScreen();
		}

		public void Show(Control parent, DropdownPosition position)
		{
			if (tablePanel1.Controls.Count == 0) return;
			this.parent = parent;
			this.position = position;
			MoveToPosition();
			PlaceOnScreen();
			Size s = Size;
			IsShown = true;
			Show(); // First show messes up size
			Size = s;
			tablePanel1.Invalidate();
		}

		private void ClearControls()
		{
			selectedRow = null;
			tablePanel1.Controls.Clear();
		}

		private void HandleActivated(object sender, EventArgs e)
		{
			IsShown = true;
		}

		private void HandleDeactivated(object sender, EventArgs e)
		{
			if (selectedRow != null) selectedRow.Selected = false;
			selectedRow = null;
			Hide();
			IsShown = false;
			EventHandler evt = Hidden;
			if (evt != null) evt(this, EventArgs.Empty);
		}

		private void HandleRowClicked(object sender, EventArgs e)
		{
			parent.Focus();
		}

		private void HandleRowSelectionChanged(object sender, EventArgs e)
		{
			var senderRow = sender as SelectableControl<T>;
			if (senderRow == null || !senderRow.Selected || senderRow == selectedRow) return;
			if (selectedRow != null) selectedRow.Selected = false;
			selectedRow = senderRow;
			if (selectedRow != null) selectedRow.Selected = true;
		}

		private void AddRow(Control c)
		{
			int index = tablePanel1.RowCount++;
			var style = new RowStyle(SizeType.AutoSize);
			c.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
			c.Margin = new Padding(0);
			tablePanel1.RowStyles.Add(style);
			tablePanel1.Controls.Add(c, 0, index);
		}

		private void MoveToPosition()
		{
			if (parent == null) return;
			Point parentPos = parent.PointToScreen(Point.Empty);
			switch (position)
			{
				case DropdownPosition.TopLeft:
					DesktopLocation = new Point(parentPos.X + parent.Width - Width, parentPos.Y - Height);
					break;
				case DropdownPosition.TopRight:
					DesktopLocation = new Point(parentPos.X, parentPos.Y - Height);
					break;
				case DropdownPosition.BottomLeft:
					DesktopLocation = new Point(parentPos.X + parent.Width - Width, parentPos.Y + parent.Height);
					break;
				case DropdownPosition.BottomRight:
					DesktopLocation = new Point(parentPos.X, parentPos.Y + parent.Height);
					break;
				case DropdownPosition.BottomMiddle:
					DesktopLocation = new Point(parentPos.X + parent.Width/2 - Width/2, parentPos.Y + parent.Height);
					break;
			}
		}

		private void PlaceOnScreen()
		{
			Rectangle screenBounds = Screen.FromPoint(Location).WorkingArea;
			if (DesktopLocation.X + Width > screenBounds.Right)
			{
				DesktopLocation = new Point(Math.Max(screenBounds.Left, screenBounds.Right - Width), DesktopLocation.Y);
			}

			if (DesktopLocation.Y + Height > screenBounds.Bottom)
			{
				DesktopLocation = new Point(DesktopLocation.X, Math.Max(screenBounds.Top, screenBounds.Bottom - Height));
			}

			if (DesktopLocation.X < screenBounds.Left)
			{
				DesktopLocation = new Point(screenBounds.Left, DesktopLocation.Y);
			}

			if (DesktopLocation.Y < screenBounds.Top)
			{
				DesktopLocation = new Point(DesktopLocation.X, screenBounds.Top);
			}
		}

		private bool PopulateGroup(IEnumerable<SelectableControl<T>> group, ref int suggestedWidth)
		{
			bool hasElements = false;
			foreach (var option in group)
			{
				hasElements = true;
				option.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
				option.Width = tablePanel1.Width;
				Size suggestedSize = option.GetPreferredSize(Size);
				if (suggestedSize.Width > suggestedWidth)
				{
					suggestedWidth = suggestedSize.Width;
				}

				option.SelectionChanged += HandleRowSelectionChanged;
				option.Click += HandleRowClicked;
				AddRow(option);
			}

			return hasElements;
		}
	}
}