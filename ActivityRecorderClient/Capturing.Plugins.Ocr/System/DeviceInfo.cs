using System;
using System.Drawing;
using System.Windows.Forms;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Ocr
{
	public class DeviceInfo
	{
        public IntPtr Handle { set; get; }
        public string DeviceName { get; set; }
		public int VerticalResolution { get; set; }
		public int HorizontalResolution { get; set; }
		public Rectangle MonitorArea { get; set; }
		public double VScale { set; get; }
		public double HScale { set; get; }
		public Screen Screen { set; get; }
	    public Taskbar Taskbar { get; set; }
	}
}