using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// ReSharper disable BuiltInTypeReferenceStyle

namespace JavaAccessibility
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
	}
}
