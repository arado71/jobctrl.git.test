using System;
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming

namespace Tct.Java.Accessibility
{
	public struct CustomPtr
	{
		public static CustomPtr Zero = default(CustomPtr);

		private readonly long _value;

		public CustomPtr(long value)
		{
			_value = value;
		}

		public long Value => _value;

		public static bool operator ==(CustomPtr x, CustomPtr y)
		{
			return x._value == y._value;
		}

		public static bool operator !=(CustomPtr x, CustomPtr y)
		{
			return x._value == y._value;
		}

		public override bool Equals(object obj)
		{
			if (obj is CustomPtr)
			{
				return this == (CustomPtr)obj;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return _value.GetHashCode();
		}
	}

	public delegate Boolean GetAccessibleContextFromHwndDelegate(IntPtr hwnd, out Int32 vmId, out IntPtr ac);
	public delegate Boolean GetAccessibleContextFromHwndDelegateX86(IntPtr hwnd, out Int32 vmId, out CustomPtr ac);

	public delegate bool GetAccessibleContextAt(int vmId, IntPtr parentAc, int x, int y, out IntPtr ac);
	public delegate bool GetAccessibleContextAtX86(int vmId, CustomPtr parentAc, int x, int y, out CustomPtr ac);

	public delegate bool DoAccessibleActionsDelegate(int vmId, IntPtr accessibleContext, ref AccessibleActionsToDo actionsToDo, out int failure);
	public delegate bool DoAccessibleActionsDelegateX86(int vmId, CustomPtr accessibleContext, ref AccessibleActionsToDo actionsToDo, out int failure);

	public delegate bool GetAccessibleActionsDelegate(int vmId, IntPtr accessibleContext, out AccessibleActions actions);
	public delegate bool GetAccessibleActionsDelegateX86(int vmId, CustomPtr accessibleContext, out AccessibleActions actions);

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	public struct AccessibleContextInfo
	{
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
		public string name; // the AccessibleName of the object
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
		public string description; // the AccessibleDescription of the object

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
		public string role; // localized AccesibleRole string
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
		public string role_en_US; // AccesibleRole string in the en_US locale
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
		public string states; // localized AccesibleStateSet string (comma separated)
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
		public string states_en_US; // AccesibleStateSet string in the en_US locale (comma separated)

		public Int32 indexInParent; // index of object in parent
		public Int32 childrenCount; // # of children, if any

		public Int32 x; // screen coords in pixels
		public Int32 y; // "
		public Int32 width; // pixel width of object
		public Int32 height; // pixel height of object

		public Boolean accessibleComponent; // flags for various additional
		public Boolean accessibleAction; // Java Accessibility interfaces
		public Boolean accessibleSelection; // FALSE if this object doesn't
		public Boolean accessibleText; // implement the additional interface
		// in question

		// BOOL accessibleValue; // old BOOL indicating whether AccessibleValue is supported
		public Boolean accessibleInterfaces; // new bitfield containing additional interface flags
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	public struct AccessibleTextItemsInfo
	{
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1)]
		public String letter;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
		public String word;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
		public String sentence;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct AccessibleActionInfo
	{
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
		public string name;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct AccessibleActionsToDo
	{
		public int actionsCount;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
		public AccessibleActionInfo[] actions;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public class AccessibleActions
	{
		public int actionsCount;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
		public AccessibleActionInfo[] actionInfo;
	}

}
