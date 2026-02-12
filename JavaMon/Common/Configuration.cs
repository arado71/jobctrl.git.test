using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tct.JcMon.Common
{
	static class Configuration
	{
		public static int CaptureInterval { get; set; } = 200;
		public static List<Func<IntPtr, CaptureResult>> CaptureFuncs { get; set; } = new List<Func<IntPtr, CaptureResult>>();
		public static IntPtr Hwnd { get; internal set; }
	}
}
