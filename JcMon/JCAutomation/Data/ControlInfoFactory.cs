using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Automation;
using JCAutomation.SystemAdapter;

namespace JCAutomation.Data
{
	public enum DetailLevel
	{
		None,
		Only,
		WithParents,
		WithSiblings,
	}

	public class ControlInfoFactory
	{
		private readonly Dictionary<AutomationElement, ControlInfo> controlCache = new Dictionary<AutomationElement, ControlInfo>();
		private readonly Dictionary<IntPtr, WindowInfo> windowCache = new Dictionary<IntPtr, WindowInfo>();

		public bool IncludeComInterface { get; set; }
		public bool IncludeScreenshots { get; set; }

		public ControlInfo Get(AutomationElement element, DetailLevel controlDetailLevel, DetailLevel windowDetailLevel)
		{
			if (controlDetailLevel == DetailLevel.None) throw new ArgumentException("controlDetailLevel can't be None");
			controlCache.Clear();
			windowCache.Clear();
			ControlInfo result = null;
			switch (controlDetailLevel)
			{
				case DetailLevel.Only:
					result = Get(element);
					break;
				case DetailLevel.WithParents:
					result = GetWithParents(element);
					break;
				case DetailLevel.WithSiblings:
					result = GetWithSiblings(element);
					break;
				default:
					Debug.Fail("Unknown control detail");
					break;
			}
			foreach (var controlInfo in controlCache.Values)
			{
				SetWindow(controlInfo, windowDetailLevel);
			}

			foreach (var windowInfo in windowCache.Values)
			{
				if (IncludeComInterface)
				{
					windowInfo.Notes = ComReflectionHelper.GetInfo(ComReflectionHelper.GetNativeObject(windowInfo.Handle));
				}

				if (IncludeScreenshots)
				{
					windowInfo.Image = WindowHelper.GetBitmap(windowInfo.Handle);
				}
			}

			return result;
		}

		private void SetWindow(ControlInfo control, DetailLevel windowDetailLevel)
		{
			switch (windowDetailLevel)
			{
				case DetailLevel.None:
					break;
				case DetailLevel.Only:
					control.Window = Get(control.WindowHandle);
					break;
				case DetailLevel.WithParents:
					control.Window = GetWithParents(control.WindowHandle);
					break;
				case DetailLevel.WithSiblings:
					control.Window = GetWithSiblings(control.WindowHandle);
					break;
				default:
					Debug.Fail("Unknown window detail");
					break;
			}
		}

		private ControlInfo Get(AutomationElement element)
		{
			return controlCache.GetOrCreate(element, ControlInfo.Extract);
		}

		private WindowInfo Get(IntPtr handle)
		{
			return windowCache.GetOrCreate(handle, WindowInfo.Extract);
		}

		private ControlInfo GetWithParents(AutomationElement element)
		{
			return HierarchyHelper.BuildParents(element, AutomationHelper.GetParent, Get);
		}

		private WindowInfo GetWithParents(IntPtr handle)
		{
			return HierarchyHelper.BuildParents(handle, WindowHelper.GetParent, Get);
		}

		private ControlInfo GetWithSiblings(AutomationElement element)
		{
			return HierarchyHelper.BuildPartialHierarchy(element, AutomationHelper.GetParent, AutomationHelper.GetChildren, Get);
		}

		private WindowInfo GetWithSiblings(IntPtr handle)
		{
			return HierarchyHelper.BuildPartialHierarchy(handle, WindowHelper.GetParent, WindowHelper.GetChildren, Get);
		}
	}
}
