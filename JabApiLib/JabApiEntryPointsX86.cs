using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace JavaAccessibility
{
	class JabApiEntryPointsX86: IX86JabApiEntryPoints
	{
		private const String WinAccessBridgeDll = "WindowsAccessBridge-32.dll";

		[DllImport(WinAccessBridgeDll, SetLastError = true, ThrowOnUnmappableChar = true, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
		private static extern void Windows_run();

		[DllImport(WinAccessBridgeDll, SetLastError = true, ThrowOnUnmappableChar = true, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
		private static extern Int32 isJavaWindow(IntPtr hwnd);

		[DllImport(WinAccessBridgeDll, SetLastError = true, ThrowOnUnmappableChar = true, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
		private static extern void releaseJavaObject(Int32 vmID, CustomPtr javaObject);

		[DllImport(WinAccessBridgeDll, SetLastError = true, ThrowOnUnmappableChar = true, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
		private static extern Boolean getAccessibleContextFromHWND(IntPtr hwnd, out Int32 vmID, out CustomPtr ac);

		[DllImport(WinAccessBridgeDll, SetLastError = true, ThrowOnUnmappableChar = true, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
		private static extern CustomPtr getAccessibleChildFromContext(Int32 vmID, CustomPtr ac, Int32 index);

		[DllImport(WinAccessBridgeDll, SetLastError = true, ThrowOnUnmappableChar = true, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
		private static extern Boolean getAccessibleContextInfo(Int32 vmID, CustomPtr accessibleContext, IntPtr acInfo);

		[DllImport(WinAccessBridgeDll, SetLastError = true, ThrowOnUnmappableChar = true, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
		private static extern Boolean getAccessibleTextAttributes(Int32 vmID, CustomPtr accessibleContext, Int32 index, CustomPtr attributes);

		[DllImport(WinAccessBridgeDll, SetLastError = true, ThrowOnUnmappableChar = true, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
		public static extern Boolean getAccessibleTextItems(Int32 vmID, CustomPtr accessibleContext, IntPtr textItems, Int32 index);

		[DllImport(WinAccessBridgeDll, SetLastError = true, ThrowOnUnmappableChar = true, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
		public static extern Boolean getAccessibleContextAt(Int32 vmID, CustomPtr acparent, Int32 x, Int32 y, out CustomPtr ac);

		[DllImport(WinAccessBridgeDll, SetLastError = true, ThrowOnUnmappableChar = true, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
		public static extern CustomPtr getActiveDescendent(Int32 vmID, CustomPtr ac);

		public Boolean GetAccessibleTextItems(Int32 vmID, CustomPtr accessibleContext, IntPtr textItems, Int32 index)
		{
			return getAccessibleTextItems(vmID, accessibleContext, textItems, index);
		}

		public CustomPtr GetAccessibleChildFromContext(int vmID, CustomPtr ac, int index)
		{
			return getAccessibleChildFromContext(vmID, ac, index);
		}

		public bool GetAccessibleContextFromHwnd(IntPtr hwnd, out int vmID, out CustomPtr ac)
		{
			return getAccessibleContextFromHWND(hwnd, out vmID, out ac);
		}

		public bool GetAccessibleContextInfo(int vmID, CustomPtr accessibleContext, IntPtr acInfo)
		{
			return getAccessibleContextInfo(vmID, accessibleContext, acInfo);
		}

		public bool GetAccessibleTextAttributes(int vmID, CustomPtr accessibleContext, int index, CustomPtr attributes)
		{
			return getAccessibleTextAttributes(vmID, accessibleContext, index, attributes);
		}

		public int IsJavaWindow(IntPtr hwnd)
		{
			return isJavaWindow(hwnd);
		}

		public void ReleaseJavaObject(int vmId, CustomPtr javaObject)
		{
			releaseJavaObject(vmId, javaObject);
		}

		public void WindowsRun()
		{
			Windows_run();
		}

		public bool GetAccessibleContextAt(int vmID, CustomPtr acparent, int x, int y, out CustomPtr ac)
		{
			return getAccessibleContextAt(vmID, acparent, x, y, out ac);
		}

		public CustomPtr GetActiveDescendent(Int32 vmID, CustomPtr ac)
		{
			return getActiveDescendent(vmID, ac);
		}
	}
}
