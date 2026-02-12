using System;
using System.Collections.Generic;
using MonoMac.AppKit;
using MonoMac.Foundation;

namespace Tct.ActivityRecorderClient.ViewMac
{
	public class ControllerCollection
	{
		private readonly List<NSWindowController> ctrls = new List<NSWindowController>();

		public int Count { get { return ctrls.Count; } }

		public void Add(NSWindowController ctrl)
		{
			Add(ctrl, null);
		}

		public void Add(NSWindowController ctrl, EventHandler closed)
		{
			ctrls.Add(ctrl);
			if (closed != null)
			{
				ctrl.Window.WillClose += closed;
			}
			ctrl.Window.WillClose += HandleCtrlWindowWillClose;
		}

		void HandleCtrlWindowWillClose(object sender, EventArgs e)
		{
			var ctrl = ((NSWindowController)((NSWindow)((NSNotification)sender).Object).WindowController);
			ctrls.Remove(ctrl);
			ctrl.Dispose();
		}
	}
}

