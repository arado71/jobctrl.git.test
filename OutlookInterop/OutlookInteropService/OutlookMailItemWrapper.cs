using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using Microsoft.Office.Interop.Outlook;

namespace OutlookInteropService
{
	public class OutlookMailItemWrapper : OutlookItem
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public OutlookMailItemWrapper(object item, bool isInOutbox) : base(item, isInOutbox)
		{
			if (MailItem == null) return;
			if (!isInOutbox)
				try
				{
					MailItem.PropertyChange += MailItemOnPropertyChange;

				}
				catch (System.Exception ex)
				{
					log.Warn("Adding propertyChange event handler failed", ex);
				}
		}

		private void MailItemOnPropertyChange(string name)
		{
			PropertyChange?.Invoke(name);
		}

		public event ItemEvents_10_PropertyChangeEventHandler PropertyChange;

		public MailItem MailItem => Class != OlObjectClass.olMail ? null : InnerObject as MailItem;

		public override void Dispose()
		{
			try
			{
				if (MailItem != null && !IsInOutbox) MailItem.PropertyChange -= MailItemOnPropertyChange;
			}
			catch (System.Exception ex)
			{
				log.Warn("Removing propertyChange event handler failed", ex);
			}
			base.Dispose();
		}
	}
}
