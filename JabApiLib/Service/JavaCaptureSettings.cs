using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Tct.Java.Accessibility;
using Tct.Java.Plugin;

namespace Tct.Java.Service
{
	[DataContract(Namespace = "http://jobctrl.com/java", Name = "JavaCaptureSettings")]
	public class JavaCaptureSettings
	{
		[DataMember]
		public string ProcessName { get; set; }

		[DataMember]
		public string CaptureName { get; set; }

		[DataMember]
		public IntPtr Hwnd { get; set; }

		[DataMember]
		public JavaPluginValueType JavaPluginValueType { get; set; }

		[DataMember]
		public int[] PathToElement { get; set; }

		[DataMember]
		public string Parameters { get; set; }
	}
}
