using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace MonitoringClient
{
	public static class VisualTreeHelperEx
	{
		public static T GetParentOfType<T>(DependencyObject reference) where T : DependencyObject
		{
			var result = reference;
			while (result != null)
			{
				result = VisualTreeHelper.GetParent(result);
				if (result is T) return (T)result;
			}
			return null;
		}

		public static IEnumerable<DependencyObject> GetParents(DependencyObject reference)
		{
			var result = reference;
			while (result != null)
			{
				result = VisualTreeHelper.GetParent(result);
				if (result != null) yield return result;
			}
		}

		/// <summary>
		/// Retrieves all the visual children of a framework element.
		/// </summary>
		/// <param name="parent">The parent framework element.</param>
		/// <returns>The visual children of the framework element.</returns>
		internal static IEnumerable<DependencyObject> GetVisualChildren(this DependencyObject parent)
		{
			Debug.Assert(parent != null, "The parent cannot be null.");

			int childCount = VisualTreeHelper.GetChildrenCount(parent);
			for (int counter = 0; counter < childCount; counter++)
			{
				yield return VisualTreeHelper.GetChild(parent, counter);
			}
		}

		/// <summary>
		/// Retrieves all the logical children of a framework element using a 
		/// breadth-first search.  A visual element is assumed to be a logical 
		/// child of another visual element if they are in the same namescope.
		/// For performance reasons this method manually manages the queue 
		/// instead of using recursion.
		/// </summary>
		/// <param name="parent">The parent framework element.</param>
		/// <returns>The logical children of the framework element.</returns>
		internal static IEnumerable<FrameworkElement> GetLogicalChildrenBreadthFirst(this FrameworkElement parent)
		{
			Debug.Assert(parent != null, "The parent cannot be null.");

			var queue = new Queue<FrameworkElement>(parent.GetVisualChildren().OfType<FrameworkElement>());

			while (queue.Count > 0)
			{
				FrameworkElement element = queue.Dequeue();
				yield return element;

				foreach (FrameworkElement visualChild in element.GetVisualChildren().OfType<FrameworkElement>())
				{
					queue.Enqueue(visualChild);
				}
			}
		}
	}
}
