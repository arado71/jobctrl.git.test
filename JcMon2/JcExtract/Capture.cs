using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JcExtract
{
	public class Capture
	{
		public IntPtr WindowHandle { get; set; }
		public string ProcessName { get; set; }
		public List<CapturedValue> Values { get; set; }
	}
}
