using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace JavaAccessibility
{
	class JabApiX86: IJabApi
	{
		private IX86JabApiEntryPoints _jabApiEntryPoints;

		private Action windowsRun;
		private Func<IntPtr, Int32> isJavaWindow;
		private Action<Int32, CustomPtr> releaseJavaObject;
		private GetAccessibleContextFromHwndDelegateX86 getAccessibleContextFromHwnd;
		private Func<Int32, CustomPtr, Int32, CustomPtr> getAccessibleChildFromContext;
		private Func<Int32, CustomPtr, IntPtr, Boolean> getAccessibleContextInfo;
		private Func<Int32, CustomPtr, Int32, CustomPtr, Boolean> getAccessibleTextAttributes;
		private Func<Int32, CustomPtr, IntPtr, Int32, Boolean> getAccessibleTextItems;
		private GetAccessibleContextAtX86 getAccessibleContextAt;
		private Func<Int32, CustomPtr, CustomPtr> getActiveDescendent;

		private bool initialized = false;

		private JabApiX86() { }

		private static JabApiX86 instance;

		public static JabApiX86 Instance => instance ?? (instance = new JabApiX86());

		/// <summary>
		/// Make sure to call this from the GUI thread!
		/// </summary>
		public void Initialize()
		{
			_jabApiEntryPoints = new JabApiEntryPointsX86();
			_jabApiEntryPoints.WindowsRun();
			windowsRun = _jabApiEntryPoints.WindowsRun;
			isJavaWindow = _jabApiEntryPoints.IsJavaWindow;
			releaseJavaObject = _jabApiEntryPoints.ReleaseJavaObject;
			getAccessibleContextFromHwnd = _jabApiEntryPoints.GetAccessibleContextFromHwnd;
			getAccessibleChildFromContext = _jabApiEntryPoints.GetAccessibleChildFromContext;
			getAccessibleContextInfo = _jabApiEntryPoints.GetAccessibleContextInfo;
			getAccessibleTextAttributes = _jabApiEntryPoints.GetAccessibleTextAttributes;
			getAccessibleTextItems = _jabApiEntryPoints.GetAccessibleTextItems;
			getAccessibleContextAt = _jabApiEntryPoints.GetAccessibleContextAt;
			getActiveDescendent = _jabApiEntryPoints.GetActiveDescendent;
			initialized = true;
		}

		public bool IsJavaWindow(IntPtr hwnd)
		{
			Debug.Assert(initialized, "IsJavaWindow() called before Initialize()!");
			return isJavaWindow(hwnd) == 1;
		}

		public AccessibleItem GetComponentTree(IntPtr hwnd, out int vmID)
		{
			Debug.Assert(initialized, "GetComponentTree called before Initialize()!");
			vmID = 0;
			if (IsJavaWindow(hwnd))
			{
				if (getAccessibleContextFromHwnd(hwnd, out vmID, out CustomPtr acPtr))
				{
					return getAccessibleContextForTree(vmID, acPtr, out _, null); // RECURSION SEED
				}
			}
			return null;
		}

		public void ReleaseJavaObject(int vmId, dynamic pointer)
		{
			releaseJavaObject(vmId, pointer);
		}

		public AccessibleContextInfo GetChildAt(int vmId, dynamic parentPointer, int index, out dynamic childPointer)
		{
			IntPtr acPtr = Marshal.AllocHGlobal(Marshal.SizeOf(new AccessibleContextInfo()));
			var ac = new AccessibleContextInfo();
			Marshal.StructureToPtr(ac, acPtr, true);
			childPointer = getAccessibleChildFromContext(vmId, parentPointer, index);
			if (getAccessibleContextInfo(vmId, childPointer, acPtr))
			{
				var result = (AccessibleContextInfo)Marshal.PtrToStructure(acPtr, typeof(AccessibleContextInfo));
				if (acPtr != IntPtr.Zero)
					Marshal.FreeHGlobal(acPtr);
				return result;
			}
			throw new Exception($"Couldn't process element in vm: {vmId}. GetChildAt function failed.");
		}

		public AccessibleContextInfo GetElementFromHwnd(IntPtr hwnd, out dynamic pointer, out int vmId)
		{
			IntPtr acPtr = Marshal.AllocHGlobal(Marshal.SizeOf(new AccessibleContextInfo()));
			var ac = new AccessibleContextInfo();
			Marshal.StructureToPtr(ac, acPtr, true);
			if (getAccessibleContextFromHwnd(hwnd, out vmId, out CustomPtr ptr))
			{
				if (getAccessibleContextInfo(vmId, ptr, acPtr))
				{
					pointer = ptr;
					var result = (AccessibleContextInfo)Marshal.PtrToStructure(acPtr, typeof(AccessibleContextInfo));
					if(acPtr != IntPtr.Zero)
						Marshal.FreeHGlobal(acPtr);
					return result;
				}
			}
			throw new Exception($"Couldn't process element at hwnd: {hwnd}. GetElementFromHwnd function failed.");
		}

		public AccessibleTextItemsInfo GetTextItemsInfo(int vmId, dynamic contextPointer)
		{
			IntPtr ati = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(AccessibleTextItemsInfo)));
			getAccessibleTextItems(vmId, contextPointer, ati, 0);
			AccessibleTextItemsInfo atInfo = (AccessibleTextItemsInfo)Marshal.PtrToStructure(ati, typeof(AccessibleTextItemsInfo));
			if (ati != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(ati);
			}
			return atInfo;
		}

		private AccessibleTextItemsInfo getAccessibleTextInfo(Int32 vmID, CustomPtr ac)
		{
			//Reserve memory
			IntPtr ati = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(AccessibleTextItemsInfo)));
			//Call DLL.
			getAccessibleTextItems(vmID, ac, ati, 0);
			//Create object
			AccessibleTextItemsInfo atInfo = (AccessibleTextItemsInfo)Marshal.PtrToStructure(ati, typeof(AccessibleTextItemsInfo));
			//Free memory       
			if (ati != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(ati);
			}
			return atInfo;
		}

		private AccessibleItem getAccessibleContextForTree(Int32 vmID, CustomPtr currentPtr, out AccessibleContextInfo currentContext, AccessibleItem parentItem)
		{
			// Allocate global memory space for the size of AccessibleContextInfo and store the address in acPtr
			IntPtr acPtr = Marshal.AllocHGlobal(Marshal.SizeOf(new AccessibleContextInfo()));
			try
			{
				var ac = new AccessibleContextInfo();
				Marshal.StructureToPtr(ac, acPtr, true);
				if (getAccessibleContextInfo(vmID, currentPtr, acPtr))
				{
					currentContext = (AccessibleContextInfo)Marshal.PtrToStructure(acPtr, typeof(AccessibleContextInfo));
					AccessibleItem newItem = BuildAccessibleTree(currentContext, parentItem, currentPtr);

					if (ReferenceEquals(newItem, null)) return null;
					//Checks to see if current object has any text items.
					if (currentContext.accessibleText)
					{
						//Gets text items.
						AccessibleTextItemsInfo textItem = getAccessibleTextInfo(vmID, currentPtr);
						newItem.TextValue = textItem.sentence;
					}

					//Start collecting children
					for (int i = 0; i < currentContext.childrenCount; i++)
					{
						if (currentContext.role_en_US != "unknown") // Note the optomization here, I found this get me to an acceptable speed
						{
							CustomPtr childPtr = getAccessibleChildFromContext(vmID, currentPtr, i);

							getAccessibleContextForTree(vmID, childPtr, out _, newItem);
						}

					}

					return newItem;
				}
				else
				{
					currentContext = new AccessibleContextInfo();
				}
				GC.KeepAlive(ac);
			}
			finally
			{
				if (acPtr != IntPtr.Zero)
					Marshal.FreeHGlobal(acPtr);
			}
			return null;
		}

		private static AccessibleItem BuildAccessibleTree(AccessibleContextInfo acInfo, AccessibleItem parentItem, CustomPtr acPtr)
		{
			AccessibleItem item = new AccessibleItem(acInfo);
			parentItem?.Children.Add(item);
			item.Parent = parentItem;
			return item;
		}

		public AccessibleContextInfo GetAccessibleContextAt(int vmID, dynamic acParent, int x, int y, out dynamic pointer)
		{
			IntPtr acPtr = Marshal.AllocHGlobal(Marshal.SizeOf(new AccessibleContextInfo()));
			var ac = new AccessibleContextInfo();
			Marshal.StructureToPtr(ac, acPtr, true);
			if (getAccessibleContextAt(vmID, acParent, x, y, out CustomPtr ptr))
			{
					pointer = ptr;
					var result = (AccessibleContextInfo)Marshal.PtrToStructure(acPtr, typeof(AccessibleContextInfo));
					if (acPtr != IntPtr.Zero)
						Marshal.FreeHGlobal(acPtr);
					return result;
			}
			throw new Exception($"Couldn't process element at x: {x}, y: {y}. GetAccessibleContextAt function failed.");
		}

		public AccessibleContextInfo GetActiveDescendent(Int32 vmID, dynamic ac, out dynamic descendantPtr)
		{
			var tmp = getActiveDescendent(vmID, ac);
			descendantPtr = tmp;
			var result = (AccessibleContextInfo)Marshal.PtrToStructure(tmp, typeof(AccessibleContextInfo));
			return result;
		}
	}
}
