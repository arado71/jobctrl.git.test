using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MonoMac.CoreFoundation;
using MonoMac.Foundation;

namespace Tct.ActivityRecorderClient.Capturing.Desktop.Mac.Accessibility
{
	public class AXObject : CFWrapper //I don't know how SafeHandles are implemented by mono... but they migh be better (todo research)
	{
		public AXObject(IntPtr handle, bool owns)
			: base(handle, owns)
		{
		}

		public static bool IsApiEnabled()
		{
			return AXAPIEnabled();
		}

		public static AXObject CreateFromApplication(int pid)
		{
			IntPtr axApp = AXUIElementCreateApplication(pid);
			if (axApp == IntPtr.Zero)
				throw new Exception("Cannot create accessible object from pid " + pid);
			return new AXObject(axApp, true);
		}

		public AXObject GetAttribute(AXAttribute attr, out AXError error)
		{
			IntPtr axObjHandle;
			error = AXUIElementCopyAttributeValue(Handle, attr.Handle, out axObjHandle);
			return new AXObject(axObjHandle, true);
		}

		public string GetStringValueForAttribute(AXAttribute attr, out AXError error)
		{
			IntPtr axObjHandle;
			error = AXUIElementCopyAttributeValue(Handle, attr.Handle, out axObjHandle);
			using (var axObj = new AXObject(axObjHandle, true))
			{
				return CFStringHelper.GetString(axObj.Handle);
			}
		}

		public IEnumerable<AXObject> GetChildrenInRole(string role)
		{
			AXError error;
			using (var axChildren = GetAttribute(AXAttribute.Children, out error))
			{
				if (error != AXError.Success)
					yield break; //log error
				var arrWinChPtrs = NSArray.ArrayFromHandleFunc<IntPtr>(axChildren.Handle, n => n);
				for (int i = arrWinChPtrs.Length - 1; i >= 0; i--)
				{
					using (var axChild = new AXObject(arrWinChPtrs[i], false))
					{
						using (var axRole = axChild.GetAttribute(AXAttribute.Role, out error))
						{
							if (error != AXError.Success)
								yield break; //log error
							if (CFStringHelper.StrEquals(axRole.Handle, role))
								yield return axChild;
						}
					}
				}
			}
		}

		[DllImport(LibraryMac.AppKit.Path)]
		private static extern IntPtr AXUIElementCreateApplication(int pid);

		[DllImport(LibraryMac.AppKit.Path)]
		private static extern AXError AXUIElementCopyAttributeValue(IntPtr element, IntPtr attribute, out IntPtr value);

		[DllImport(LibraryMac.AppKit.Path)]
		private static extern bool AXAPIEnabled();
	}
}

