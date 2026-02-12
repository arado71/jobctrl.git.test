using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace JavaAccessibility
{
	class JabApiEntryPointsX64: IJabApiEntryPoints
	{
		private const String WinAccessBridgeDll = "WindowsAccessBridge-64.dll";

		[DllImport(WinAccessBridgeDll, SetLastError = true, ThrowOnUnmappableChar = true, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
		private static extern void Windows_run();

		[DllImport(WinAccessBridgeDll, SetLastError = true, ThrowOnUnmappableChar = true, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
		private static extern Int32 isJavaWindow(IntPtr hwnd);

		[DllImport(WinAccessBridgeDll, SetLastError = true, ThrowOnUnmappableChar = true, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
		private static extern void releaseJavaObject(Int32 vmID, IntPtr javaObject);

		[DllImport(WinAccessBridgeDll, SetLastError = true, ThrowOnUnmappableChar = true, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
		private static extern Boolean getAccessibleContextFromHWND(IntPtr hwnd, out Int32 vmID, out IntPtr ac);

		[DllImport(WinAccessBridgeDll, SetLastError = true, ThrowOnUnmappableChar = true, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
		private static extern IntPtr getAccessibleChildFromContext(Int32 vmID, IntPtr ac, Int32 index);

		[DllImport(WinAccessBridgeDll, SetLastError = true, ThrowOnUnmappableChar = true, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
		private static extern Boolean getAccessibleContextInfo(Int32 vmID, IntPtr accessibleContext, IntPtr acInfo);

		[DllImport(WinAccessBridgeDll, SetLastError = true, ThrowOnUnmappableChar = true, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
		private static extern Boolean getAccessibleTextAttributes(Int32 vmID, IntPtr accessibleContext, Int32 index, IntPtr attributes);

		[DllImport(WinAccessBridgeDll, SetLastError = true, ThrowOnUnmappableChar = true, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
		public static extern Boolean getAccessibleTextItems(Int32 vmID, IntPtr accessibleContext, IntPtr textItems, Int32 index);

		[DllImport(WinAccessBridgeDll, SetLastError = true, ThrowOnUnmappableChar = true, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
		public static extern Boolean getAccessibleContextAt(Int32 vmID, IntPtr acparent, Int32 x, Int32 y, out IntPtr ac);

		[DllImport(WinAccessBridgeDll, SetLastError = true, ThrowOnUnmappableChar = true, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
		public static extern IntPtr getActiveDescendent(Int32 vmID, IntPtr ac);

		public Boolean GetAccessibleTextItems(Int32 vmID, IntPtr accessibleContext, IntPtr textItems, Int32 index)
		{
			return getAccessibleTextItems(vmID, accessibleContext, textItems, index);
		}

		public IntPtr GetAccessibleChildFromContext(int vmID, IntPtr ac, int index)
		{
			return getAccessibleChildFromContext(vmID, ac, index);
		}

		public bool GetAccessibleContextFromHwnd(IntPtr hwnd, out int vmID, out IntPtr ac)
		{
			return getAccessibleContextFromHWND(hwnd, out vmID, out ac);
		}

		public bool GetAccessibleContextInfo(int vmID, IntPtr accessibleContext, IntPtr acInfo)
		{
			return getAccessibleContextInfo(vmID, accessibleContext, acInfo);
		}

		public bool GetAccessibleTextAttributes(int vmID, IntPtr accessibleContext, int index, IntPtr attributes)
		{
			return getAccessibleTextAttributes(vmID, accessibleContext, index, attributes);
		}

		public int IsJavaWindow(IntPtr hwnd)
		{
			return isJavaWindow(hwnd);
		}

		public void ReleaseJavaObject(int vmId, IntPtr javaObject)
		{
			releaseJavaObject(vmId, javaObject);
		}

		public void WindowsRun()
		{
			Windows_run();
		}

		public bool GetAccessibleContextAt(int vmID, IntPtr acparent, int x, int y, out IntPtr ac)
		{
			return getAccessibleContextAt(vmID, acparent, x, y, out ac);
		}

		public IntPtr GetActiveDescendent(Int32 vmID, IntPtr ac)
		{
			return getActiveDescendent(vmID, ac);
		}
	}
}
