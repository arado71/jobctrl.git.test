using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Automation;
using JcMon2.SystemAdapter;
using log4net;

namespace JcMon2
{
	[Serializable]
	public class ControlInfo : IEquatable<ControlInfo>, IHierarchical<ControlInfo>
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public string Name { get; set; }
		public string ClassName { get; set; }
		public string Text { get; set; }
		public string Value { get; set; }
		public string ControlType { get; set; }
		public string HelpText { get; set; }
		public string Selection { get; set; }
		public string AutomationId { get; set; }
		public ControlInfo Parent { get; set; }
		public ControlInfo[] Siblings { get; set; }
		public ControlInfo[] Children { get; set; }

		public WindowInfo Window { get; set; }

		[NonSerialized]
		private AutomationElement element;
		[NonSerialized]
		private IntPtr windowPointer;

		internal AutomationElement Element
		{
			get
			{
				return element;
			}

			set
			{
				element = value;
			}
		}

		internal IntPtr WindowHandle
		{
			get
			{
				return windowPointer;
			}

			set
			{
				windowPointer = value;
			}
		}

		public bool Equals(ControlInfo other)
		{
			if (ReferenceEquals(other, null)) return false;
			if (ReferenceEquals(this, other)) return true;
			return other.Name == Name && other.ClassName == ClassName && other.ControlType == ControlType &&
				   other.HelpText == HelpText && other.Value == Value && other.Text == Text && other.AutomationId == AutomationId;
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as ControlInfo);
		}

		public int GetHashCode(ControlInfo obj)
		{
			var hashCode = 17;
			hashCode = hashCode * 23 + (obj.Name ?? "").GetHashCode();
			hashCode = hashCode * 23 + (obj.ClassName ?? "").GetHashCode();
			hashCode = hashCode * 23 + (obj.Text ?? "").GetHashCode();
			hashCode = hashCode * 23 + (obj.Value ?? "").GetHashCode();
			hashCode = hashCode * 23 + (obj.ControlType ?? "").GetHashCode();
			hashCode = hashCode * 23 + (obj.HelpText ?? "").GetHashCode();
			//hashCode = hashCode*23 + (obj.Selection ?? "").GetHashCode();
			hashCode = hashCode * 23 + (obj.AutomationId ?? "").GetHashCode();
			return hashCode;
		}

		public WindowInfo GetWindowInfo()
		{
			if (Parent != null)
			{
				var parentResult = Parent.GetWindowInfo();
				if (parentResult != null) return parentResult;
			}

			if (Window != null)
			{
				return GetWindowInfo(Window);
			}

			return null;
		}

		private WindowInfo GetWindowInfo(WindowInfo windowInfo)
		{
			if (windowInfo.Parent != null)
			{
				var parentResult = GetWindowInfo(windowInfo.Parent);
				if (parentResult != null) return parentResult;
			}

			return windowInfo;
		}

		public static ControlInfo Extract(AutomationElement element)
		{
			if (element == null) return null;
			try
			{
				return new ControlInfo
				{
					Element = element,
					Name = element.Current.Name,
					ClassName = element.Current.ClassName,
					Text = AutomationElementHelper.GetText(element),
					HelpText = element.Current.HelpText,
					ControlType = element.Current.ControlType.LocalizedControlType,
					Selection = AutomationElementHelper.GetSelection(element),
					Value = AutomationElementHelper.GetValue(element),
					AutomationId = element.Current.AutomationId,
					WindowHandle = new IntPtr(element.Current.NativeWindowHandle),
				};
			}
			catch (Exception ex)
			{
				log.Warn("Failed to extract properties", ex);
			}

			return null;
		}

		public override string ToString()
		{
			return Name + " [" + ClassName + "]";
		}
	}
}
