using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;
using Redemption;

namespace OutlookInteropService
{
	public static class SafeHandlerExtension
	{
		public static IntPtr GetHandle(object insexp)
		{
			if (insexp is Explorer explorer) return GetExplorerHandle(explorer);
			if (insexp is Inspector inspector) return GetInspectorHandle(inspector);
			throw new System.Exception("Nor inspector or explorer");
		}

		private static IntPtr GetExplorerHandle(Explorer explorer)
		{
			SafeExplorer safeExplorer = null;
			try
			{
				safeExplorer = RedemptionLoader.new_SafeExplorer();
				safeExplorer.Item = explorer;
				return ((IWin32Window)safeExplorer).Handle;
			}
			finally
			{
				if (safeExplorer != null) Marshal.ReleaseComObject(safeExplorer);
			}
		}

		private static IntPtr GetInspectorHandle(Inspector inspector)
		{
			SafeInspector safeInspector = null;
			try
			{
				safeInspector = RedemptionLoader.new_SafeInspector();
				safeInspector.Item = inspector;
				return ((IWin32Window)safeInspector).Handle;
			}
			finally
			{
				if (safeInspector != null) Marshal.ReleaseComObject(safeInspector);
			}
		}
	}
}
