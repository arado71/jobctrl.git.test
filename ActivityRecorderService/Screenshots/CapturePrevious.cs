using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderService.Screenshots
{
	public class CapturePrevious
	{
		public readonly object ThisLock = new object(); //protects the access to this class
		public Bitmap Bitmap { get; set; }
		public int Id { get; set; }
	}
}
