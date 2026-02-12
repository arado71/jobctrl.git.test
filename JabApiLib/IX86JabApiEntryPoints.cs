using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JavaAccessibility
{
	interface IX86JabApiEntryPoints
	{
		void WindowsRun();
		Int32 IsJavaWindow(IntPtr hwnd);
		void ReleaseJavaObject(int vmId, CustomPtr javaObject);
		Boolean GetAccessibleContextFromHwnd(IntPtr hwnd, out int vmID, out CustomPtr ac);
		CustomPtr GetAccessibleChildFromContext(int vmID, CustomPtr ac, int index);
		Boolean GetAccessibleContextInfo(int vmID, CustomPtr accessibleContext, IntPtr acInfo);
		Boolean GetAccessibleTextAttributes(int vmID, CustomPtr accessibleContext, int index, CustomPtr attributes);
		Boolean GetAccessibleTextItems(int vmID, CustomPtr accessibleContext, IntPtr textItems, int index);
		bool GetAccessibleContextAt(int vmID, CustomPtr acparent, int x, int y, out CustomPtr ac);
		CustomPtr GetActiveDescendent(Int32 vmID, CustomPtr ac);
	}
}
