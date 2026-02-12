using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Automation;
using log4net;

namespace JCAutomation.SystemAdapter
{
	public static class AutomationHelper
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public static IEnumerable<AutomationElement> GetParents(AutomationElement element)
		{	
			var result = new List<AutomationElement>();
			try
			{
				var walker = TreeWalker.ControlViewWalker;
				AutomationElement elementParent;
				var node = element;
				if (node == AutomationElement.RootElement) result.Add(node);
				do
				{
					elementParent = walker.GetParent(node);
					if (elementParent == AutomationElement.RootElement) break;
					result.Add(elementParent);
					node = elementParent;
				} while (true);
			}
			catch (Exception ex)
			{
				log.Warn("Failed to get parents", ex);
			}

			return result;
		}

		public static AutomationElement GetParent(AutomationElement element)
		{
			if (element == null) return null;
			try
			{
				var walker = TreeWalker.ControlViewWalker;
				var elementParent = walker.GetParent(element);
				if (elementParent == AutomationElement.RootElement) return null;
				return elementParent;
			}
			catch (Exception ex)
			{
				log.Warn("Failed to get parents", ex);
			}

			return null;
		}

		public static IEnumerable<AutomationElement> GetChildren(AutomationElement element)
		{
			if (element == null) return Enumerable.Empty<AutomationElement>();
			var result = new List<AutomationElement>();
			try
			{
				var walker = TreeWalker.ControlViewWalker;
				var elementChild = walker.GetFirstChild(element);
				while (elementChild != null)
				{
					result.Add(elementChild);
					elementChild = walker.GetNextSibling(elementChild);
				}
			}
			catch (Exception ex)
			{
				log.Warn("Failed to get children", ex);
			}

			return result;
		}
	}
}
