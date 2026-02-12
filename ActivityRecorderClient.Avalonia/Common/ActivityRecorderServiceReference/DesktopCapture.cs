using System;
using System.Collections.Generic;
using System.Linq;

namespace Tct.ActivityRecorderClient.ActivityRecorderServiceReference
{
	sealed partial class DesktopCapture : ICaptureEquatable<DesktopCapture>
	{
		public Dictionary<string, string> GlobalVariables { get; set; } 

		public bool CaptureEquals(DesktopCapture other)
		{
			if (Object.ReferenceEquals(other, null)) return false;
			if (Object.ReferenceEquals(this, other)) return true;
			return ListsCaptureEqual(this.DesktopWindows, other.DesktopWindows)
				   && ListsCaptureEqual(this.Screens, other.Screens);
		}

		private static bool ListsCaptureEqual<T>(IList<T> first, IList<T> second) where T : ICaptureEquatable<T>
		{
			var firstNullOrEmpty = Object.ReferenceEquals(first, null) || first.Count == 0;
			var secondNullOrEmpty = Object.ReferenceEquals(second, null) || second.Count == 0;
			if (firstNullOrEmpty ^ secondNullOrEmpty) return false; //if one is empty and the other is not
			if (firstNullOrEmpty) return true; //if both are empty
			if (first.Count != second.Count) return false;
			for (int i = 0; i < first.Count; i++)
			{
				if (!first[i].CaptureEquals(second[i])) return false;
			}
			return true;
		}

		public override string ToString()
		{
			var actDw = this.GetActiveWindow();
			return "DC (" + (this.DesktopWindows == null ? "null" : this.DesktopWindows.Count.ToString()) + ") [" + (actDw == null ? "" : actDw.ToString()) + "]";
		}
	}
}