using System;
using System.Drawing;
using JCAutomation.SystemAdapter;
using log4net;

namespace JCAutomation.Data
{
	[Serializable]
	public class WindowInfo : IHierarchical<WindowInfo>
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		[NonSerialized]
		private Bitmap cachedImage = null;
		private string imageBackingField;
		[NonSerialized]
		private IntPtr handle;

		public Bitmap Image
		{
			get
			{
				if (cachedImage == null)
				{
					if (!string.IsNullOrEmpty(imageBackingField))
					{
						cachedImage = BitmapHelper.Base64ToBitmap(imageBackingField);
					}
				}

				return cachedImage;
			}

			set
			{
				cachedImage = value;
				imageBackingField = BitmapHelper.BitmapToBase64(value);
			}
		}

		public string Title { get; set; }
		public string ClassName { get; set; }
		public string ProcessName { get; set; }
		public WindowInfo Parent { get; set; }
		public WindowInfo[] Children { get; set; }
		public WindowInfo[] Siblings { get; set; }
		public string Notes { get; set; }
		internal IntPtr Handle { get { return handle; } }

		public WindowInfo()
		{
		}

		public WindowInfo(IntPtr hWnd)
		{
			handle = hWnd;
		}

		public static WindowInfo Extract(IntPtr hWnd)
		{
			if (hWnd == IntPtr.Zero) return null;
			try
			{
				string processName;
				var processId = WindowHelper.GetWindowProcessId(hWnd);
				ProcessNameHelper.TryGetProcessName(processId, out processName);
				return new WindowInfo(hWnd)
				{
					Title = WindowHelper.GetWindowText(hWnd),
					ClassName = WindowHelper.GetClassName(hWnd),
					ProcessName = processName,
				};
			}
			catch (Exception ex)
			{
				log.Warn("Failed to extract data", ex);
			}

			return null;
		}

		public override string ToString()
		{
			return Title + " [" + ClassName + "]";
		}
	}
}
