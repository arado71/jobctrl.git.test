using System;
using System.Collections.Generic;
using AppKit;
using Foundation;

namespace Tct.ActivityRecorderClient.ViewMac
{
	// TODO: mac
	public class ControllerCollection
	{
		private readonly List<object> ctrls = new List<object>();

		public int Count { get { return ctrls.Count; } }

		public void Add(object ctrl)
		{
			Add(ctrl, null);
		}

		public void Add(object ctrl, EventHandler closed)
		{
			ctrls.Add(ctrl);
			if (closed != null)
			{
				//ctrl.Window.WillClose += closed;
			}
			//ctrl.Window.WillClose += HandleCtrlWindowWillClose;
		}

		void HandleCtrlWindowWillClose(object sender, EventArgs e)
		{
			//var ctrl = ((NSWindowController)((NSWindow)((NSNotification)sender).Object).WindowController);
			//ctrls.Remove(ctrl);
			//ctrl.Dispose();
		}
	}
}

