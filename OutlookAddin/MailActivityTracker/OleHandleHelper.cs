using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace MailActivityTracker
{
	public static class OleHandleHelper
	{
		public static IntPtr GetHandle(object inspector)
		{
			var oleInterface = new Guid("00000114-0000-0000-C000-000000000046");
			var handler = IntPtr.Zero;
			var punk = Marshal.GetIUnknownForObject(inspector);

			IntPtr myInterface;
			Marshal.QueryInterface(punk, ref oleInterface, out myInterface);
			var window = (IOleWindow) Marshal.GetTypedObjectForIUnknown(myInterface, typeof(IOleWindow));

			if (window == null)
			{
				MessageBox.Show("Failure trying to get the IOleWindow interface", "OleHandleHelper");
			}
			else
			{
				try
				{
					window.GetWindow(ref handler);
				}
				catch (COMException ex)
				{
					MessageBox.Show("COMError calling IOleWindow.GetWindow HRESULT " + ex.ErrorCode, "OleHandleHelper");
				}
				finally
				{
					Marshal.Release(myInterface);
				}
			}
			return handler;
		}
	}
}
