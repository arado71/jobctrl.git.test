using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JcExtract
{
	internal class CaptureEventArgs : EventArgs
	{
		public Capture Capture { get; set; }
	}
}
