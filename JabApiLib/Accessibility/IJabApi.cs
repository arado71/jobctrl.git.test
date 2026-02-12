using System;

namespace Tct.Java.Accessibility
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
		AccessibleContextInfo GetActiveDescendent(Int32 vmID, dynamic ac, out dynamic descendantPtr);
		void GetAccessibleActions(int vmId, dynamic ac, out string[] actions);
		void DoActionAt(int vmId, dynamic parentPointer, string[] actions);
		void SetTextContents(int vmId, dynamic ac, string text);
		void RequestFocus(int vmId, dynamic ac);
	}
}
