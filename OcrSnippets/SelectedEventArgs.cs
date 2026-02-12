using System;
using System.Drawing;
using Tct.ActivityRecorderClient.Capturing.Plugins.Ocr;

namespace TcT.ActivityRecorderClient.SnippingTool
{
	public class SelectedEventArgs : EventArgs
	{
		private int Left { get; set; }
		private int Top { get; set; }
		private int Bottom { get; set; }
		private int Right { get; set; }
		public DeviceInfo Monitor { set; get; }

		public Rectangle Rectangle
		{
			get { return new Rectangle(Left, Top, Right - Left, Bottom - Top); }
			set
			{
				Left = value.Left;
				Top = value.Top;
				Right = value.Right;
				Bottom = value.Bottom;
			}
		}
	}
}