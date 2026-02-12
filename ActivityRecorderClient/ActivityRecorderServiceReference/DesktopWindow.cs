using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.Capturing.Plugins;

namespace Tct.ActivityRecorderClient.ActivityRecorderServiceReference
{
	sealed partial class DesktopWindow : ICaptureEquatable<DesktopWindow>
	{
		[NonSerialized]
		private Rectangle clientRect;
		public Rectangle ClientRect 		//this excludes the titlebar
		{
			get { return clientRect; }
			set { clientRect = value; }
		}

		//public bool Minimized { get; set; } //not sure if we need it
		//public string ClassName { get; set; } //not sure if we need it

		[NonSerialized]
		private IntPtr handle;
		public IntPtr Handle
		{
			get { return handle; }
			set { handle = value; }
		}

		[NonSerialized]
		private int processId;
		public int ProcessId
		{
			get { return processId; }
			set { processId = value; }
		}

		[NonSerialized]
		private bool isMaximized;
		public bool IsMaximized
		{
			get { return isMaximized; }
			set { isMaximized = value; }
		}

		[NonSerialized]
		private bool isAdditionalWindow;
		public bool IsAdditionalWindow
		{
			get { return isAdditionalWindow; }
			set { isAdditionalWindow = value; }
		}

		[NonSerialized]
		private Dictionary<CaptureExtensionKey, string> captureExtensions;
		public IDictionary<CaptureExtensionKey, string> CaptureExtensions
		{
			get { return captureExtensions; }
		}

		public void SetCaptureExtension(CaptureExtensionKey key, string value)
		{
			if (captureExtensions == null)
			{
				captureExtensions = new Dictionary<CaptureExtensionKey, string>();
			}
			captureExtensions[key] = value;
		}

		public Rectangle WindowRect
		{
			get { return new Rectangle(X, Y, Width, Height); }
			set
			{
				X = (short)value.X;
				Y = (short)value.Y;
				Width = (short)value.Width;
				Height = (short)value.Height;
			}
		}

		public bool CaptureEquals(DesktopWindow other)
		{
			if (Object.ReferenceEquals(other, null)) return false;
			if (Object.ReferenceEquals(this, other)) return true;
			return this.Title == other.Title
				&& this.IsActive == other.IsActive
				&& this.ProcessName == other.ProcessName
				&& this.Url == other.Url
				&& this.VisibleClientArea == other.VisibleClientArea
				&& this.X == other.X
				&& this.Y == other.Y
				&& this.Width == other.Width
				&& this.Height == other.Height
				&& (Object.ReferenceEquals(this.CaptureExtensions, other.CaptureExtensions)
					|| (this.CaptureExtensions != null
						&& other.CaptureExtensions != null
						&& this.CaptureExtensions.Count == other.CaptureExtensions.Count
						&& this.CaptureExtensions.All(n => n.Value == other.CaptureExtensions[n.Key]))
					)
				;
		}

		public override string ToString()
		{
			return this.ProcessName
				+ " " + this.Title
				+ " " + this.Url
				+ GetCaptureExtensionsToString()
				+ " V:" + this.VisibleClientArea
				+ " X:" + this.X
				+ " Y:" + this.Y
				+ " W:" + this.Width
				+ " H:" + this.Height
				;
		}

		private string GetCaptureExtensionsToString()
		{
			if (CaptureExtensions == null || CaptureExtensions.Count == 0) return "";
			return GetCaptureExtensionsToString(CaptureExtensions);
		}

		public static string GetCaptureExtensionsToString(IEnumerable<KeyValuePair<CaptureExtensionKey, string>> extensionRules)
		{
			var sb = new StringBuilder();
			foreach (var extensionByKeyId in extensionRules.GroupBy(n => n.Key.Id))
			{
				sb.Append(" ").Append(extensionByKeyId.Key).Append(" (");
				var first = true;
				foreach (var keyValue in extensionByKeyId)
				{
					if (!first) sb.Append(", ");
					first = false;
					sb.Append(keyValue.Key.Key).Append(":").Append(keyValue.Value);
				}
				sb.Append(")");
			}
			return sb.ToString();
		}
	}
}
