using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;

namespace Tct.Java.Accessibility
{
	public class JabApiController
	{
		private static JabApiController instance;
		public static JabApiController Instance => instance ?? (instance = new JabApiController());
		private IJabApi jabApi;

		private JabApiController()
		{
			if (IntPtr.Size == 8)
			{
				jabApi = JabApi.Instance;
			}
			else
			{
				if (Environment.Is64BitOperatingSystem)
				{
					jabApi = JabApiX86.Instance;
				}
				else
				{
					jabApi = JabApi.Instance;
				}
			}
			jabApi.Initialize();
		}

		public bool IsJavaWindow(IntPtr hwnd)
		{
			return jabApi.IsJavaWindow(hwnd);
		}

		public AccessibleItem GetComponentTree(IntPtr hwnd, out int vmID)
		{
			return jabApi.GetComponentTree(hwnd, out vmID);
		}

		public void ReleaseJavaObject(int hwnd, dynamic pointer)
		{
			jabApi.ReleaseJavaObject(hwnd, pointer);
		}

		public AccessibleWrapper GetChildElementAt(AccessibleWrapper parent, int index)
		{

			AccessibleContextInfo contextInfo = jabApi.GetChildAt(parent.vmId, parent.pointer, index, out dynamic childPointer);
			return new AccessibleWrapper(parent.vmId, childPointer)
			{
				Description = contextInfo.description,
				Name = contextInfo.name,
				Role = contextInfo.role,
				Location = new Point(contextInfo.x, contextInfo.y),
				Width = contextInfo.width,
				Height = contextInfo.height,
				ChildrenCount = contextInfo.childrenCount,
				States = contextInfo.states_en_US
			};
		}

		public AccessibleWrapper GetContextFromHwnd(IntPtr hwnd)
		{
			AccessibleContextInfo info = jabApi.GetElementFromHwnd(hwnd, out dynamic pointer, out int vmId);
			return new AccessibleWrapper(vmId, pointer)
			{
				Description = info.description,
				Name = info.name,
				Role = info.role,
				Location = new Point(info.x, info.y),
				Width = info.width,
				Height = info.height,
				ChildrenCount = info.childrenCount,
				States = info.states_en_US
			};
		}

		public string GetTextElementFromAccessibleWrapper(AccessibleWrapper aw)
		{
			AccessibleTextItemsInfo info = jabApi.GetTextItemsInfo(aw.vmId, aw.pointer);
			return info.sentence;
		}

		public string GetTextValueFromHWndIEnumerable(IntPtr hwnd, IEnumerable<int> indexes)
		{
			var aw = GetContextFromHwnd(hwnd);
			foreach (var index in indexes)
			{
				aw = GetChildElementAt(aw, index);
			}
			return GetTextElementFromAccessibleWrapper(aw);
		}

		public string GetNameValueFromHWndIEnumerable(IntPtr hwnd, IEnumerable<int> indexes)
		{
			var aw = GetContextFromHwnd(hwnd);
			foreach (var index in indexes)
			{
				aw = GetChildElementAt(aw, index);
			}
			return aw.Name;
		}

		public string GetComboValueFromAccessibleWrapper(AccessibleWrapper wrapper)
		{
			var ac = jabApi.GetActiveDescendent(wrapper.vmId, wrapper.pointer, out dynamic descendantPtr);
			var resultWrapper = new AccessibleWrapper(wrapper.vmId, descendantPtr, ac);
			return resultWrapper.Name;
		}

		public string GetTableValueFromAccessibleWrapper(AccessibleWrapper tableWrapper, int colNumber)
		{
			int childCount = tableWrapper.ChildrenCount;
			StringBuilder result = new StringBuilder();
			for (int i = 0; i < childCount; i++)
			{
				result.Append("\"");
				AccessibleWrapper aw = GetChildElementAt(tableWrapper, i);
				string value = string.IsNullOrEmpty(aw.Name) ? GetTextElementFromAccessibleWrapper(aw) : aw.Name;

				result.Append(value);
				result.Append("\"");
				if ((i + 1) % colNumber == 0)
				{
					result.Append(Environment.NewLine);
				}
				else
				{
					result.Append(",");
				}
			}
			return result.ToString();
		}
	}
}
