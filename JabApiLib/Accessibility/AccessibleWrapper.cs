using System.Drawing;

namespace Tct.Java.Accessibility
{
	public class AccessibleWrapper
	{
		internal dynamic pointer;
		internal int vmId;
		public string Name { get; set; }
		public string Description { get; set; }
		public string Role { get; set; }
		public Point Location { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
		public int ChildrenCount { get; set; }
		public string States { get; set; }

		public AccessibleWrapper(int vmId, dynamic pointer)
		{
			this.pointer = pointer;
			this.vmId = vmId;
		}

		public AccessibleWrapper(int vmId, dynamic pointer, AccessibleContextInfo info)
		{
			this.pointer = pointer;
			this.vmId = vmId;
			Description = info.description;
			Name = info.name;
			Role = info.role;
			Location = new Point(info.x, info.y);
			Width = info.width;
			Height = info.height;
			ChildrenCount = info.childrenCount;
			States = info.states_en_US;
		}

		public bool IsSame(AccessibleWrapper other)
		{
			if (other.Location == Location &&
			    other.Width == Width &&
			    other.Height == Height &&
			    other.Name == Name &&
			    other.Role == Role)
				return true;
			return false;
		}

		~AccessibleWrapper()
		{
			if(pointer != null)
				JabApiController.Instance.ReleaseJavaObject(vmId, pointer);
		}
	}
}
