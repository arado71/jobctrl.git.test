using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JavaAccessibility
{
	interface IJabApi
	{
		void Initialize();
		bool IsJavaWindow(IntPtr hwnd);
		AccessibleItem GetComponentTree(IntPtr hwnd, out Int32 vmID);
		void ReleaseJavaObject(int vmId, dynamic pointer);
		AccessibleContextInfo GetChildAt(int vmId, dynamic parentPointer, int index, out dynamic childPointer);
		AccessibleContextInfo GetElementFromHwnd(IntPtr hwnd, out dynamic pointer, out int vmId);
		AccessibleTextItemsInfo GetTextItemsInfo(int vmId, dynamic contextPointer);
		AccessibleContextInfo GetAccessibleContextAt(int vmID, dynamic acParent, Int32 x, Int32 y, out dynamic pointer);
		AccessibleContextInfo GetActiveDescendent(Int32 vmID, dynamic ac, out dynamic descendantPtr);
	}
}
