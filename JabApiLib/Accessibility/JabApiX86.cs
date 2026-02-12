using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace Tct.Java.Accessibility
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
		private Func<Int32, CustomPtr, CustomPtr> getActiveDescendent;
		private GetAccessibleActionsDelegateX86 getAccessibleActions;
		private DoAccessibleActionsDelegateX86 doAccessibleActions;
		private Func<Int32, CustomPtr, string, bool> setTextContents;
		private Func<Int32, CustomPtr, bool> requestFocus;

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
			getActiveDescendent = _jabApiEntryPoints.GetActiveDescendent;
			getAccessibleActions = _jabApiEntryPoints.GetAccessibleActions;
			doAccessibleActions = _jabApiEntryPoints.DoAccessibleActions;
			setTextContents = _jabApiEntryPoints.SetTextContents;
			requestFocus = _jabApiEntryPoints.RequestFocus;
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

		public void GetAccessibleActions(int vmId, dynamic ac, out string[] actions)
		{
			if (!getAccessibleActions(vmId, ac, out AccessibleActions actionsStr))
				throw new Exception($"Couldn't do actions in vm: {vmId}. GetAccessibleActions function failed.");
			actions = actionsStr.actionInfo.Take(actionsStr.actionsCount).Select(a => a.name).ToArray();
		}

		public void DoActionAt(int vmId, dynamic parentPointer, string[] actions)
		{
			var actionsTodo = new AccessibleActionsToDo() { actionsCount = actions.Length, actions = new AccessibleActionInfo[32] };
			for (var i = 0; i < actions.Length; i++)
			{
				actionsTodo.actions[i].name = actions[i];
			}

			var failure = 0;
			if (!doAccessibleActions(vmId, parentPointer, ref actionsTodo, out failure))
				throw new Exception($"Couldn't do actions in vm: {vmId}. DoAccessibleActions function failed.");
		}

		public void SetTextContents(int vmId, dynamic ac, string text)
		{
			if (!setTextContents(vmId, ac, text))
				throw new Exception($"Couldn't do actions in vm: {vmId}. SetTextContents function failed.");
		}

		public void RequestFocus(int vmId, dynamic ac)
		{
			if (!requestFocus(vmId, ac))
				throw new Exception($"Couldn't do actions in vm: {vmId}. RequestFocus function failed.");
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
					newItem.DoAction = action => DoActionAt(vmID, currentPtr, new[] { action });
					newItem.SetText = action => SetTextContents(vmID, currentPtr, action);
					newItem.ActionsAccessor = () => { GetAccessibleActions(vmID, currentPtr, out var actions); return actions.ToList(); };
					newItem.RequestFocus = () => RequestFocus(vmID, currentPtr);

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
		
		public AccessibleContextInfo GetActiveDescendent(Int32 vmID, dynamic ac, out dynamic descendantPtr)
		{
			var tmp = getActiveDescendent(vmID, ac);
			descendantPtr = tmp;
			IntPtr acPtr = Marshal.AllocHGlobal(Marshal.SizeOf(new AccessibleContextInfo()));
			var acInfo = new AccessibleContextInfo();
			Marshal.StructureToPtr(acInfo, acPtr, true);
			if (getAccessibleContextInfo(vmID, descendantPtr, acPtr))
			{
				var result = (AccessibleContextInfo)Marshal.PtrToStructure(acPtr, typeof(AccessibleContextInfo));
				if (acPtr != IntPtr.Zero)
					Marshal.FreeHGlobal(acPtr);
				return result;
			}
			throw new Exception($"Couldn't process element at vmID: {vmID}. GetActiveDescendent function failed.");
		}
	}
}
