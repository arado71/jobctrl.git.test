using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Menu;
using Tct.ActivityRecorderClient.View.Navigation;
using Screen = System.Windows.Forms.Screen;
using Message = System.Windows.Forms.Message;

namespace Tct.ActivityRecorderClient.View.Controls
{
	public partial class SearchResult : Form, ISelectionProvider<WorkDataWithParentNames>
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private const int RowPadding = 14;
		private INavigator navigator = null;
		private int selectedIndex = -1;

		public bool HasFocus { get; set; }

		public bool IsShown { get; set; }

		public Point Offset { get; set; }

		public bool IsForeground
		{
			get { return FromHandle(WinApi.GetForegroundWindow()) != null; }
		}

		public INavigator Navigator
		{
			get { return navigator; }

			set
			{
				if (navigator != null) navigator.OnNavigate -= HandleNavigated;
				navigator = value;
				if (navigator != null) navigator.OnNavigate += HandleNavigated;
			}
		}

		public int SelectedIndex
		{
			get { return selectedIndex; }

			set
			{
				if (selectedIndex == value) return;
				if (selectedIndex != -1 && selectedIndex <= tableLayoutPanel1.Controls.Count)
				{
					Debug.Assert(tableLayoutPanel1.Controls[selectedIndex] is ISelectable<NavigationBase>);
					((ISelectable<NavigationBase>)tableLayoutPanel1.Controls[selectedIndex]).Selected = false;
				}
				selectedIndex = value < tableLayoutPanel1.Controls.Count ? value : -1;
				if (selectedIndex != -1)
				{
					Debug.Assert(tableLayoutPanel1.Controls[selectedIndex] is ISelectable<NavigationBase>);
					((ISelectable<NavigationBase>)tableLayoutPanel1.Controls[selectedIndex]).Selected = true;
				}
			}
		}

		public int Count
		{
			get { return tableLayoutPanel1.RowCount; }
		}

		public SearchResult()
		{
			InitializeComponent();
			BackColor = StyleUtils.Shadow;
			tableLayoutPanel1.BackColor = StyleUtils.Background;
		}

		public WorkDataWithParentNames Selection
		{
			get
			{
				if (!Visible || selectedIndex == -1) return null;
				Debug.Assert(tableLayoutPanel1.Controls[selectedIndex] is WorkRowCompact);
				var compactRow = tableLayoutPanel1.Controls[selectedIndex] as WorkRowCompact;
				var navigation = compactRow.Value as INavigationWithWork;
				Debug.Assert(navigation != null);
				return navigation.Work;
			}
		}

		public void ClearSelection()
		{
			SelectedIndex = -1;
		}

		public NavigationBase GetSelection()
		{
			Debug.Assert(selectedIndex == -1 || tableLayoutPanel1.Controls[selectedIndex] is WorkRowCompact);
			return selectedIndex == -1 ? null : ((WorkRowCompact)tableLayoutPanel1.Controls[selectedIndex]).Navigation;
		}

		public void Populate(IEnumerable<WorkDataWithParentNames> elements)
		{
			this.SuspendLayout();
			tableLayoutPanel1.SuspendLayout();

			int proposedWidth = 0;
			tableLayoutPanel1.RowCount = 0;
			while (tableLayoutPanel1.Controls.Count > 0)
			{
				tableLayoutPanel1.Controls[0].Dispose();
			}
			
			foreach (WorkDataWithParentNames element in elements)
			{
				var row = new WorkRowCompact(new NavigationWork(Navigator, element)) { Width = Width };
				AddRow(row);
				if (row.GetPreferredWidth() + RowPadding > proposedWidth)
				{
					proposedWidth = row.GetPreferredWidth() + RowPadding;
				}

				row.Width = tableLayoutPanel1.Width;
				row.SelectionChanged += HandleSelectionChanged;
				row.MouseEnter += HandleMouseEntered;
				row.MouseLeave += HandleMouseLeft;
			}

			Height = tableLayoutPanel1.PreferredSize.Height + 2;
			SetWidth(proposedWidth + 2);
			if (selectedIndex == -1)
			{
				selectedIndex = 0;
			}

			if (selectedIndex >= Count)
			{
				selectedIndex = Count - 1;
			}

			Debug.Assert(selectedIndex == -1 || tableLayoutPanel1.Controls[selectedIndex] is ISelectable<NavigationBase>);
			if (selectedIndex != -1) ((ISelectable<NavigationBase>)tableLayoutPanel1.Controls[selectedIndex]).Selected = true;

			tableLayoutPanel1.ResumeLayout(true);
			this.ResumeLayout(true);
		}

		public void SelectNext()
		{
			if (SelectedIndex == -1 || SelectedIndex == Count - 1)
			{
				SelectedIndex = 0;
			}
			else
			{
				SelectedIndex++;
			}
		}

		public void SelectPrevious()
		{
			if (SelectedIndex == -1 || SelectedIndex == 0)
			{
				SelectedIndex = Count - 1;
			}
			else
			{
				SelectedIndex--;
			}
		}

		protected override void OnActivated(EventArgs e)
		{
			base.OnActivated(e);
			HasFocus = true;
		}

		protected override void OnDeactivate(EventArgs e)
		{
			base.OnDeactivate(e);
			HasFocus = false;
		}

		protected override void WndProc(ref Message m)
		{
			if (!Focused && m.Msg == (int)WinApi.Messages.WM_PARENTNOTIFY)
			{
				// Make this form auto-grab the focus when menu/controls are clicked
				Activate();
			}

			base.WndProc(ref m);
		}

		protected int GetIndex(WorkRowCompact row)
		{
			return tableLayoutPanel1.Controls.IndexOf(row);
		}

		protected void HandleNavigated(object sender, SingleValueEventArgs<NavigationBase> e)
		{
			Hide();
		}

		protected void HandleSelectionChanged(object sender, EventArgs e)
		{
			var o = (WorkRowCompact)sender;
			if (!o.Selected && selectedIndex == GetIndex(o))
			{
				selectedIndex = -1;
			}

			if (o.Selected)
			{
				SelectedIndex = GetIndex(o);
			}
		}

		private void HandleMouseClicked(object sender, MouseEventArgs e)
		{
			OnGotFocus(EventArgs.Empty);
		}

		private void HandleMouseEntered(object sender, EventArgs e)
		{
			HasFocus = true;
		}

		private void HandleMouseLeft(object sender, EventArgs e)
		{
			HasFocus = false;
		}

		private void AddRow(Control c)
		{
			int index = tableLayoutPanel1.RowCount++;
			var style = new RowStyle(SizeType.AutoSize);
			c.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
			c.Margin = new Padding(0);
			tableLayoutPanel1.RowStyles.Add(style);
			tableLayoutPanel1.Controls.Add(c, 0, index);
		}

		private void SetWidth(int width)
		{
			Rectangle screenBounds = Screen.FromPoint(Location).WorkingArea;
			Location = Offset.X + width > screenBounds.Width ? new Point(screenBounds.Width - width, Offset.Y) : Offset;
			Width = width;
		}
	}
}