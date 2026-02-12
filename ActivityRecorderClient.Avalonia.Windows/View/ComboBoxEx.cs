using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Tct.ActivityRecorderClient.View
{
	//based on: http://msdn.microsoft.com/en-us/library/ms996411
	//http://stackoverflow.com/questions/2395747/combo-box-dropdown-position
	[System.Reflection.Obfuscation(Exclude = true)]
	public class ComboBoxEx : ComboBox
	{
		private string cueBanner = "";

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string CueBanner
		{
			get { return cueBanner; }
			set
			{
				cueBanner = value ?? "";
				if (IsHandleCreated)
				{
					this.SetCueBanner(cueBanner);
				}
			}
		}

		public ComboBoxEx()
		{
			IntegralHeight = false; //workaround to use the MaxDropDownItems
			MaxDropDownItems = 30;
		}

		protected override void WndProc(ref Message m)
		{
			if (m.Msg == WM_CTLCOLORLISTBOX)
			{
				// Make sure we are inbounds of the screen
				var topLeft = this.PointToScreen(new Point(0, 0));
				var screen = Screen.AllScreens.Where(n => n.Bounds.Contains(topLeft)).FirstOrDefault();

				//Only do this if the dropdown is going off right edge of screen otherwise default implementation will work fine (we don't care about the left edge)
				if (screen != null && this.DropDownWidth > screen.Bounds.X + screen.Bounds.Width - topLeft.X)
				{
					// Get the current combo position and size
					Rectangle comboRect = this.RectangleToScreen(this.ClientRectangle);

					//Calculate dropped list height
					int dropHeight = ItemHeight * Math.Min(Math.Max(1, this.Items.Count), MaxDropDownItems);

					//Set top position of the dropped list if 
					//it goes off the bottom of the screen
					int topOfDropDown;
					if (dropHeight > screen.Bounds.Height - comboRect.Bottom)
					{
						topOfDropDown = comboRect.Top - dropHeight - 2;
					}
					else
					{
						topOfDropDown = comboRect.Bottom;
					}

					//Calculate shifted left position
					int leftOfDropDown = comboRect.Left - (this.DropDownWidth - (screen.Bounds.X + screen.Bounds.Width - topLeft.X));

					// Postioning/sizing the drop-down
					SetWindowPos(m.LParam, 0, leftOfDropDown, topOfDropDown, 0, 0, SWP_NOSIZE);
				}
			}
			base.WndProc(ref m);
		}

		//https://connect.microsoft.com/VisualStudio/feedback/details/355454/system-windows-forms-combobox-can-throw-system-argumentoutofrangeexception-in-rare-cases
		//fix when all works are closed and dropdown is re-opened: System.ArgumentOutOfRangeException: InvalidArgument=Value of '0' is not valid for 'index'.
		public override int SelectedIndex
		{
			get { return Items.Count > 0 ? base.SelectedIndex : -1; }
			set { base.SelectedIndex = value; }
		}

		public void BringToFrontAll()
		{
			this.BringToFront();
			if (IsHandleCreated)
			{
				var info = new COMBOBOXINFO();
				info.cbSize = Marshal.SizeOf(info);
				SendMessageCb(this.Handle, CB_GETCOMBOBOXINFO, IntPtr.Zero, out info);
				SetWindowPos(info.hwndList, 0, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);
			}
		}

		protected override void OnHandleCreated(EventArgs e)
		{
			this.SetCueBanner(cueBanner);
			this.SetComboScrollWidth();
			base.OnHandleCreated(e);
		}

		protected override void OnDrawItem(DrawItemEventArgs e)
		{
			base.OnDrawItem(e);
			e.DrawBackground();
			var item = this.Items[e.Index];
			ComboBoxItemEx comboItem = item as ComboBoxItemEx;
			Brush brush = comboItem != null ? new SolidBrush(comboItem.Color) : SystemBrushes.MenuText;
			e.Graphics.DrawString(item.ToString(), this.Font, brush, e.Bounds.X, e.Bounds.Y);
			if (comboItem != null)
				brush.Dispose();
		}

		private const uint WM_CTLCOLORLISTBOX = 0x0134;
		//private const int HWND_TOPMOST = -1;
		private const uint SWP_NOSIZE = 0x0001;
		private const uint SWP_NOMOVE = 0x0002;
		private const uint SWP_NOACTIVATE = 0x0010;
		private const int CB_GETCOMBOBOXINFO = 0x164;

		[DllImport("user32.dll", EntryPoint = "SetWindowPos")]
		static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

		private struct COMBOBOXINFO
		{
			public Int32 cbSize;
			public RECT rcItem, rcButton;
			public int buttonState;
			public IntPtr hwndCombo, hwndEdit, hwndList;
		}
		private struct RECT
		{
			public int Left, Top, Right, Bottom;
		}
		[DllImport("user32.dll", EntryPoint = "SendMessageW", CharSet = CharSet.Unicode)]
		private static extern IntPtr SendMessageCb(IntPtr hWnd, int msg, IntPtr wp, out COMBOBOXINFO lp);
		[DllImport("user32.dll", EntryPoint = "GetWindowRect")]
		private static extern bool GetWindowRect(IntPtr hWnd, out RECT rc);

	}

	[System.Reflection.Obfuscation(Exclude = true)]
	public class ComboBoxItemEx
	{
		public object Value { get; set; }
		public Color Color { get; set; }

		public ComboBoxItemEx()
		{
		}

		public ComboBoxItemEx(object value, Color color)
		{
			Value = value;
			Color = color;
		}

		public override string ToString()
		{
			return Value.ToString();
		}
	}
}
