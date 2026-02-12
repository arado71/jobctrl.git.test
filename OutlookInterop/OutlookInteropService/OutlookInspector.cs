using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace OutlookInteropService
{
	public class OutlookInspector : OutlookWindowWrapper
	{
		public Outlook.Inspector Window { get; private set; }
		public OutlookInspector(Outlook.Inspector inspector, IntPtr handle, Func<OutlookItem, string, string> getJcIdFromMailItem, Action<Action> contextPost, bool useRedemption) : base(getJcIdFromMailItem, contextPost, useRedemption)
		{
			Window = inspector;
			Handle = handle;
			((Outlook.InspectorEvents_Event)inspector).Close += InspectorEventsOnClose;
			System.Diagnostics.Debug.Assert(Item == null);
			Item = new OutlookMailItemWrapper(inspector.CurrentItem, false);
			if (Mail == null) return;
			Init();
		}

		private void InspectorEventsOnClose()
		{
			DeInit(Item);
			BeforeClose();
			Dispose();
		}

		public override void Dispose()
		{
			if (Window == null) return;
			((Outlook.InspectorEvents_Event)Window).Close -= InspectorEventsOnClose;
			Marshal.ReleaseComObject(Window);
			Window = null;
			base.Dispose();
		}
	}
}
