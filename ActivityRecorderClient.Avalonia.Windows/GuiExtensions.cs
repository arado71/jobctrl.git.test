using System.Windows.Forms;

namespace Tct.ActivityRecorderClient
{
	public static class GuiExtensions
	{
		public static void FillAccessibilityFields(this Control control)
		{
			var q = new Queue<Control>();
			q.Enqueue(control);
			while (q.Any())
			{
				var ctrl = q.Dequeue();
				if (ctrl.AccessibleName == null) ctrl.AccessibleName = ctrl.Text;
				foreach (Control child in ctrl.Controls)
				{
					q.Enqueue(child);
				}
			}
		}
	}
}
