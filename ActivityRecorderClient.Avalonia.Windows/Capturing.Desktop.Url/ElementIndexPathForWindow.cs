using Accessibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tct.ActivityRecorderClient.Capturing.Desktop.Windows.Url
{
	internal class ElementIndexPathForWindow
	{
		public IntPtr Hwnd { get; set; }
		public List<int> Path { get; set; }
		public IAccessible GetAccessibleElementFromPath()
		{
			var element = AccessibilityHelper.GetIAccessibleFromWindow(Hwnd, AccessibilityHelper.ObjId.CLIENT);
			foreach (int index in Path)
			{
				element = (IAccessible)element.accChild[index];
			}
			return element;
		}

		public AccessibleItem GetAccessibleItemFromPath()
		{
			var element = new AccessibleItem(AccessibilityHelper.GetIAccessibleFromWindow(Hwnd, AccessibilityHelper.ObjId.CLIENT), null, 0, 0);
			foreach (int index in Path)
			{
				element = element.GetChildElementAt(index);
			}
			return element;
		}
	}
}
