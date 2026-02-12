using System;

// ReSharper disable BuiltInTypeReferenceStyle

namespace Tct.Java.Accessibility
{
	interface IJabApiEntryPoints
	{
		void WindowsRun();
		Int32 IsJavaWindow(IntPtr hwnd);
		void ReleaseJavaObject(Int32 vmId, IntPtr javaObject);
		Boolean GetAccessibleContextFromHwnd(IntPtr hwnd, out Int32 vmID, out IntPtr ac);
		IntPtr GetAccessibleChildFromContext(Int32 vmID, IntPtr ac, Int32 index);
		Boolean GetAccessibleContextInfo(Int32 vmID, IntPtr accessibleContext, IntPtr acInfo);
		Boolean GetAccessibleTextAttributes(Int32 vmID, IntPtr accessibleContext, Int32 index, IntPtr attributes);
		Boolean GetAccessibleTextItems(Int32 vmID, IntPtr accessibleContext, IntPtr textItems, Int32 index);
		bool GetAccessibleContextAt(int vmID, IntPtr acparent, int x, int y, out IntPtr ac);
		IntPtr GetActiveDescendent(Int32 vmID, IntPtr ac);
		bool GetAccessibleActions(int vmId, IntPtr accessibleContext, out AccessibleActions actions);
		bool DoAccessibleActions(int vmId, IntPtr accessibleContext, ref AccessibleActionsToDo accessibleActionsToDo, out int failure);
		bool SetTextContents(Int32 vmId, IntPtr ac, string text);
		bool RequestFocus(int vmId, IntPtr accessibleContext);
	}
}
