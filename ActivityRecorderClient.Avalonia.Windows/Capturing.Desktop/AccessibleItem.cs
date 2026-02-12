using Accessibility;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Tct.ActivityRecorderClient.Capturing.Desktop.Windows
{
	public class AccessibleItem
	{
		private const int CHILDID_SELF = 0;
		private IAccessible item;
		private int childrenCount = -1;

		public Size Size { get; private set; }
		public int X { get; private set; }
		public int Y { get; private set; }
		public int Width { get; private set; }
		public int Height { get; private set; }
		public Rectangle Bounds { get; private set; }
		public IAccessible Item { get { return item; } }
		public AccessibleItem Parent { get; }
		public string Name { get; private set; }
		public string TextValue { get; private set; }
		public string Role { get; private set; }
		public string Description { get; private set; }
		public int ChildIndex { get; private set; }
		public int ChildId { get; private set; }
		public List<int> Path { get; }

		public int ChildrenCount { get { return childrenCount; } }

		public AccessibleItem(IAccessible item, AccessibleItem parent, int childIndex, int childid)
		{
			this.item = item;
			ChildIndex = childIndex;
			ChildId = childid;
			if (parent == null)
			{
				Path = new List<int>();
			}
			else
			{
				Path = new List<int>(parent.Path);
				Path.Add(childIndex);
			}
			Parent = parent;
			int x = -1, y = -1, width = -1, height = -1;
			try
			{
				item.accLocation(out x, out y, out width, out height, childid);
			}
			catch (COMException) { }
			catch (UnauthorizedAccessException) { }
			X = x;
			Y = y;
			Width = width;
			Height = height;
			if (width >= 0 && height >= 0)
			{
				Size = new Size(width, height);
			}
			Bounds = new Rectangle(x, y, width, height);
			childrenCount = item.accChildCount;
			try
			{
				Name = item.accName[childid];
				TextValue = item.accValue[childid];
				Role = item.accRole[childid].ToString();
				Description = item.accDescription[childid];
			}
			catch (COMException ex) { }
			catch (UnauthorizedAccessException) { }
			catch (NotImplementedException) { }
		}

		public void tryRefresh()
		{
			int x = -1, y = -1, width = -1, height = -1;
			try
			{
				item.accLocation(out x, out y, out width, out height, ChildId);
			}
			catch (COMException) { }
			catch (UnauthorizedAccessException) { }
			X = x;
			Y = y;
			Width = width;
			Height = height;
			if (width >= 0 && height >= 0)
			{
				Size = new Size(width, height);
			}
			Bounds = new Rectangle(x, y, width, height);
			childrenCount = item.accChildCount;
			try
			{
				Name = item.accName[ChildId];
				TextValue = item.accValue[ChildId];
				Role = item.accRole[ChildId].ToString();
				Description = item.accDescription[ChildId];
			}
			catch (COMException ex) { }
			catch (UnauthorizedAccessException) { }
			catch (NotImplementedException) { }
		}

		public IEnumerable<AccessibleItem> getChildren()
		{
			AccessibleItem[] accessible;
			accessible = null;
			if (ChildId != CHILDID_SELF)
			{
				accessible = new AccessibleItem[] { };
				return accessible;
			}

			int childrenCount = Item.accChildCount;
			if (childrenCount < 0)
			{
				throw new Exception($"Invalid childCount: {childrenCount}");
			}

			object[] children = new object[childrenCount];

			int hr = AccessibleChildren(Item, 0, childrenCount, children, out childrenCount);

			if (hr == 0)
			{
				accessible = new AccessibleItem[childrenCount];
				int i = 0;
				foreach (object child in children)
				{
					if (child != null)
					{
						if (child is IAccessible)
						{
							accessible[i++] = new AccessibleItem((IAccessible)child, this, i, CHILDID_SELF);
						}
						else if (child is int)
						{
							accessible[i++] = new AccessibleItem(Item, this, i, (int)child);
						}
					}
				}

				// null children don't occur very often but if they do it stops us from going on
				// So keep track of them so we can reallocate the array if necessary
				if (childrenCount != i)
				{
					// if we had some null chilren create a smaller array to send the 
					// children back in.
					AccessibleItem[] accessibleNew = new AccessibleItem[i];
					Array.Copy(accessible, accessibleNew, i);
					accessible = accessibleNew;
				}
			}

			return accessible;
		}

		public AccessibleItem GetChildElementAt(int index)
		{
			var childrenn = getChildren();
			int i = 0;
			foreach (var c in childrenn)
			{
				if (i++ == index) return c;
			};
			return null;
		}

		[DllImport("oleacc.dll")]
		internal static extern int AccessibleChildren(Accessibility.IAccessible paccContainer,
			int iChildStart,
			int cChildren,
			[MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2), In, Out] object[] rgvarChildren,
			out int pcObtained);
	}
}
