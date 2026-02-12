using System;
using System.Collections.Generic;
using System.Drawing;

namespace Tct.Java.Accessibility
{
	public class AccessibleItem
	{
		public string Name { get; set; } // the AccessibleName of the object
		public string Description { get; set; } // the AccessibleDescription of the object
		public string Role { get; set; } // localized AccesibleRole string
		public string RoleEnUs { get; set; } // AccesibleRole string in the en_US locale
		public string States { get; set; } // localized AccesibleStateSet string (comma separated)
		public string StatesEnUs { get; set; } // AccesibleStateSet string in the en_US locale (comma separated)
		public int IndexInParent { get; set; } // index of object in parent
		public int ChildrenCount { get; set; } // # of children, if any
		public int X { get; set; } // screen coords in pixel
		public int Y { get; set; } // "
		public int Width { get; set; } // pixel width of object
		public int Height { get; set; } // pixel height of object
		public bool AccessibleComponent { get; set; } // flags for various additional
		public bool AccessibleAction { get; set; } // Java Accessibility interfaces
		public bool AccessibleSelection { get; set; } // FALSE if this object doesn't
		public bool AccessibleText { get; set; } // implement the additional interface
		public bool AccessibleInterfaces { get; set; }
		public string TextValue { get; set; }
		public Action<string> DoAction { get; set; }
		public Action<string> SetText { get; set; }
		public Func<List<string>> ActionsAccessor { private get; set; }
		public Action RequestFocus { get; set; }

		public List<AccessibleItem> Children;
		public AccessibleItem Parent;

		private List<string> actions;

		public AccessibleItem()
		{
			Children = new List<AccessibleItem>();
		}

		public AccessibleItem(AccessibleContextInfo accessibleContextInfo)
			: this()
		{
			Name = accessibleContextInfo.name; // the AccessibleName of the object
			Description = accessibleContextInfo.description; // the AccessibleDescription of the object
			Role = accessibleContextInfo.role; // localized AccesibleRole string
			RoleEnUs = accessibleContextInfo.role_en_US; // AccesibleRole string in the en_US locale
			States = accessibleContextInfo.states; // localized AccesibleStateSet string (comma separated)
			StatesEnUs = accessibleContextInfo.states_en_US; // AccesibleStateSet string in the en_US locale (comma separated)
			IndexInParent = accessibleContextInfo.indexInParent; // index of object in parent
			ChildrenCount = accessibleContextInfo.childrenCount; // # of children, if any
			X = accessibleContextInfo.x; // screen coords in pixel
			Y = accessibleContextInfo.y; // "
			Width = accessibleContextInfo.width; // pixel width of object
			Height = accessibleContextInfo.height; // pixel height of object
			AccessibleComponent = accessibleContextInfo.accessibleComponent; // flags for various additional
			AccessibleAction = accessibleContextInfo.accessibleAction; // Java Accessibility interfaces
			AccessibleSelection = accessibleContextInfo.accessibleSelection; // FALSE if this object doesn't
			AccessibleText = accessibleContextInfo.accessibleText; // implement the additional interface
			AccessibleInterfaces = accessibleContextInfo.accessibleInterfaces;
		}

		public List<string> Actions => actions ?? (actions = ActionsAccessor?.Invoke());

		public Rectangle Bounds
		{
			get
			{
				return new Rectangle(X, Y, Width, Height);
			}
		}

		public Size Size => new Size(Width, Height);
		

		public int SquarePixels
		{
			get { return X * Y; }
		}
	}
}
